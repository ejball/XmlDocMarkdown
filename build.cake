#addin "Cake.Git"
#tool "nuget:?package=XmlDocMarkdown&version=0.5.6"

using System.Text.RegularExpressions;

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");
var nugetApiKey = Argument("nugetApiKey", "");
var trigger = Argument("trigger", "");
var versionSuffix = Argument("versionSuffix", "");

var solutionFileName = "ProjectName.sln";
var docsAssembly = File($"src/ProjectName/bin/{configuration}/net461/ProjectName.dll").ToString();
var docsRepoUri = "https://github.com/ejball/RepoName.git";
var docsSourceUri = "https://github.com/ejball/RepoName/tree/master/src/ProjectName";

var nugetSource = "https://api.nuget.org/v3/index.json";
var buildBotUserName = "ejball";
var buildBotPassword = EnvironmentVariable("BUILD_BOT_PASSWORD");

Task("Clean")
	.Does(() =>
	{
		CleanDirectories("src/**/bin");
		CleanDirectories("src/**/obj");
		CleanDirectories("tests/**/bin");
		CleanDirectories("tests/**/obj");
		CleanDirectories("release");
	});

Task("Build")
	.Does(() =>
	{
		DotNetCoreRestore(solutionFileName);
		DotNetCoreBuild(solutionFileName, new DotNetCoreBuildSettings { Configuration = configuration, ArgumentCustomization = args => args.Append("--verbosity normal") });
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
		var branchName = "gh-pages";
		var docsDirectory = new DirectoryPath(branchName);
		GitClone(docsRepoUri, docsDirectory, new GitCloneSettings { BranchName = branchName });
		var exePath = File("cake/xmldocmarkdown.0.5.6/XmlDocMarkdown/tools/XmlDocMarkdown.exe").ToString();
		var arguments = $@"{docsAssembly} {branchName}{System.IO.Path.DirectorySeparatorChar} --source ""{docsSourceUri}"" --newline lf --clean";
		int exitCode = StartProcess(exePath, arguments);
		if (exitCode != 0)
			throw new InvalidOperationException($"Docs generation failed with exit code {exitCode}.");
		if (GitHasUncommitedChanges(docsDirectory))
		{
			Information("Committing all documentation changes.");
			GitAddAll(docsDirectory);
			GitCommit(docsDirectory, "ejball", "ejball@gmail.com", "Automatic documentation update.");
			Information("Pushing updated documentation to GitHub.");
			GitPush(docsDirectory, buildBotUserName, buildBotPassword, branchName);
		}
		else
		{
			Information("No documentation changes detected.");
		}
	});

Task("Test")
	.IsDependentOn("Build")
	.Does(() =>
	{
		foreach (var projectPath in GetFiles("tests/**/*.csproj").Select(x => x.FullPath))
			DotNetCoreTest(projectPath, new DotNetCoreTestSettings { Configuration = configuration });
	});

Task("NuGetPackage")
	.IsDependentOn("Rebuild")
	.IsDependentOn("Test")
	.IsDependentOn("UpdateDocs")
	.Does(() =>
	{
		if (string.IsNullOrEmpty(versionSuffix) && !string.IsNullOrEmpty(trigger))
			versionSuffix = Regex.Match(trigger, @"^v[^\.]+\.[^\.]+\.[^\.]+-(.+)").Groups[1].ToString();
		foreach (var projectPath in GetFiles("src/**/*.csproj").Select(x => x.FullPath))
			DotNetCorePack(projectPath, new DotNetCorePackSettings { Configuration = configuration, OutputDirectory = "release", VersionSuffix = versionSuffix });
	});

Task("NuGetPublish")
	.IsDependentOn("NuGetPackage")
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
		}
		else
		{
			Information("To publish this package, push this git tag: v" + version);
		}
	});

Task("Default")
	.IsDependentOn("Test");

void ExecuteProcess(string exePath, string arguments)
{
	int exitCode = StartProcess(exePath, arguments);
	if (exitCode != 0)
		throw new InvalidOperationException($"{exePath} failed with exit code {exitCode}.");
}

RunTarget(target);
