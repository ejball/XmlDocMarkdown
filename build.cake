#addin nuget:?package=Cake.Git&version=0.19.0
#addin nuget:?package=Cake.XmlDocMarkdown&version=1.4.1

using System.Text.RegularExpressions;

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");
var nugetApiKey = Argument("nugetApiKey", "");
var trigger = Argument("trigger", "");
var versionSuffix = Argument("versionSuffix", "");

var solutionFileName = "RepoName.sln";
var docsProjects = new[] { "ProjectName" };
var docsRepoUri = "https://github.com/ejball/RepoName.git";
var docsSourceUri = "https://github.com/ejball/RepoName/tree/master/src";
var nugetIgnore = new string[0];

var nugetSource = "https://api.nuget.org/v3/index.json";
var buildBotUserName = "ejball";
var buildBotPassword = EnvironmentVariable("BUILD_BOT_PASSWORD");
var buildBotDisplayName = "ejball";
var buildBotEmail = "ejball@gmail.com";

var docsBranchName = "gh-pages";
DirectoryPath docsDirectory = null;

Task("Clean")
	.Does(() =>
	{
		CleanDirectories("src/**/bin");
		CleanDirectories("src/**/obj");
		CleanDirectories("tests/**/bin");
		CleanDirectories("tests/**/obj");
		CleanDirectories("release");
	});

Task("Restore")
	.Does(() =>
	{
		DotNetCoreRestore(solutionFileName);
	});

Task("Build")
	.IsDependentOn("Restore")
	.Does(() =>
	{
		DotNetCoreBuild(solutionFileName, new DotNetCoreBuildSettings { Configuration = configuration, NoRestore = true, ArgumentCustomization = args => args.Append("--verbosity normal") });
	});

Task("Rebuild")
	.IsDependentOn("Clean")
	.IsDependentOn("Build");

Task("UpdateDocs")
	.WithCriteria(!string.IsNullOrEmpty(buildBotPassword))
	.WithCriteria(EnvironmentVariable("APPVEYOR_REPO_BRANCH") == "master")
	.IsDependentOn("Build")
	.Does(() =>
	{
		docsDirectory = new DirectoryPath(docsBranchName);
		GitClone(docsRepoUri, docsDirectory, new GitCloneSettings { BranchName = docsBranchName });

		Information($"Updating documentation at {docsDirectory}.");
		foreach (var docsProject in docsProjects)
		{
			XmlDocMarkdownGenerate(File($"src/{docsProject}/bin/{configuration}/netstandard2.0/{docsProject}.dll").ToString(), $"{docsDirectory}{System.IO.Path.DirectorySeparatorChar}",
				new XmlDocMarkdownSettings { SourceCodePath = $"{docsSourceUri}/{docsProject}", NewLine = "\n", ShouldClean = true });
		}
	});

Task("Test")
	.IsDependentOn("Build")
	.Does(() =>
	{
		foreach (var projectPath in GetFiles("tests/**/*.csproj").Select(x => x.FullPath))
			DotNetCoreTest(projectPath, new DotNetCoreTestSettings { Configuration = configuration, NoBuild = true, NoRestore = true });
	});

Task("NuGetPackage")
	.IsDependentOn("Rebuild")
	.IsDependentOn("Test")
	.IsDependentOn("UpdateDocs")
	.Does(() =>
	{
		if (string.IsNullOrEmpty(versionSuffix) && !string.IsNullOrEmpty(trigger))
			versionSuffix = Regex.Match(trigger, @"^v[^\.]+\.[^\.]+\.[^\.]+-(.+)").Groups[1].ToString();
		foreach (var projectPath in GetFiles("src/**/*.csproj").Where(x => !nugetIgnore.Contains(x.GetFilenameWithoutExtension().ToString())).Select(x => x.FullPath))
			DotNetCorePack(projectPath, new DotNetCorePackSettings { Configuration = configuration, NoBuild = true, NoRestore = true, OutputDirectory = "release", VersionSuffix = versionSuffix });
	});

Task("NuGetPackageTest")
	.IsDependentOn("NuGetPackage")
	.Does(() =>
	{
		var firstProject = GetFiles("src/**/*.csproj").First().FullPath;
		foreach (var nupkg in GetFiles("release/**/*.nupkg").Select(x => x.FullPath))
			DotNetCoreTool(firstProject, "sourcelink", $"test {nupkg}");
	});

Task("NuGetPublish")
	.IsDependentOn("NuGetPackageTest")
	.Does(() =>
	{
		var nupkgPaths = GetFiles("release/*.nupkg").Select(x => x.FullPath).ToList();

		string version = null;
		foreach (var nupkgPath in nupkgPaths)
		{
			string nupkgVersion = Regex.Match(nupkgPath, @"\.([^\.]+\.[^\.]+\.[^\.]+)\.nupkg$").Groups[1].ToString();
			if (version == null)
				version = nupkgVersion;
			else if (version != nupkgVersion)
				throw new InvalidOperationException($"Mismatched package versions '{version}' and '{nupkgVersion}'.");
		}

		if (!string.IsNullOrEmpty(nugetApiKey) && (trigger == null || Regex.IsMatch(trigger, "^v[0-9]")))
		{
			if (trigger != null && trigger != $"v{version}")
				throw new InvalidOperationException($"Trigger '{trigger}' doesn't match package version '{version}'.");

			var pushSettings = new NuGetPushSettings { ApiKey = nugetApiKey, Source = nugetSource };
			foreach (var nupkgPath in nupkgPaths)
				NuGetPush(nupkgPath, pushSettings);
				
			if (docsDirectory != null && GitHasUncommitedChanges(docsDirectory))
			{
				Information("Pushing updated documentation to GitHub.");
				GitAddAll(docsDirectory);
				GitCommit(docsDirectory, buildBotDisplayName, buildBotEmail, $"Automatic documentation update for {version}.");
				GitPush(docsDirectory, buildBotUserName, buildBotPassword, docsBranchName);
			}
		}
		else
		{
			Information("To publish this package, push this git tag: v" + version);
		}
	});

Task("Default")
	.IsDependentOn("Test");

RunTarget(target);
