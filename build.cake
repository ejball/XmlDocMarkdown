#addin "nuget:?package=Cake.Git&version=0.10.0"
#addin "nuget:?package=Octokit&version=0.23.0"
#tool "nuget:?package=coveralls.io&version=1.3.4"
#tool "nuget:?package=NUnit.ConsoleRunner&version=3.5.0"
#tool "nuget:?package=PdbGit&version=3.0.32"
#tool "nuget:?package=OpenCover&version=4.6.519"
#tool "nuget:?package=ReportGenerator&version=2.5.0"

using LibGit2Sharp;

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");
var nugetApiKey = Argument("nugetApiKey", "");
var githubApiKey = Argument("githubApiKey", "");
var coverallsApiKey = Argument("coverallsApiKey", "");
var prerelease = Argument("prerelease", "");

var solutionFileName = "XmlDocMarkdown.sln";
var githubOwner = "ejball";
var githubRepo = "XmlDocMarkdown";
var githubRawUri = "http://raw.githubusercontent.com";
var nugetSource = "https://www.nuget.org/api/v2/package";
var coverageAssemblies = new[] { "XmlDocMarkdown.Core" };

var rootPath = MakeAbsolute(Directory(".")).FullPath;
var gitRepository = LibGit2Sharp.Repository.IsValid(rootPath) ? new LibGit2Sharp.Repository(rootPath) : null;

var githubClient = new Octokit.GitHubClient(new Octokit.ProductHeaderValue("build.cake"));
if (!string.IsNullOrEmpty(githubApiKey))
	githubClient.Credentials = new Octokit.Credentials(githubApiKey);

string headSha = null;
string version = null;

Task("Clean")
	.Does(() =>
	{
		CleanDirectories($"src/**/bin");
		CleanDirectories($"src/**/obj");
		CleanDirectories($"tests/**/bin");
		CleanDirectories($"tests/**/obj");
		CleanDirectories("release");
	});

Task("Build")
	.IsDependentOn("Clean")
	.Does(() =>
	{
		NuGetRestore(solutionFileName);
		MSBuild(solutionFileName, settings => settings.SetConfiguration(configuration));
	});

Task("GenerateDocs")
	.IsDependentOn("Build")
	.Does(() => GenerateExample(verify: false));

Task("VerifyGenerateDocs")
	.IsDependentOn("Build")
	.Does(() => GenerateExample(verify: true));

Task("Test")
	.IsDependentOn("VerifyGenerateDocs")
	.Does(() => NUnit3($"tests/**/bin/**/*.Tests.dll", new NUnit3Settings { NoResults = true }));

Task("SourceIndex")
	.IsDependentOn("Test")
	.WithCriteria(() => configuration == "Release" && gitRepository != null)
	.Does(() =>
	{
		if (prerelease.Length == 0)
		{
			var dirtyEntry = gitRepository.RetrieveStatus().FirstOrDefault(x => x.State != FileStatus.Unaltered && x.State != FileStatus.Ignored);
			if (dirtyEntry != null)
				throw new InvalidOperationException($"The git working directory must be clean, but '{dirtyEntry.FilePath}' is dirty.");

			headSha = gitRepository.Head.Tip.Sha;
			try
			{
				githubClient.Repository.Commit.GetSha1(githubOwner, githubRepo, headSha).GetAwaiter().GetResult();
			}
			catch (Octokit.NotFoundException exception)
			{
				throw new InvalidOperationException($"The current commit '{headSha}' must be pushed to GitHub.", exception);
			}

			foreach (var pdbPath in GetFiles($"src/**/bin/**/*.pdb"))
				ExecuteProcess(@"cake\PdbGit\tools\PdbGit.exe", $@"""{pdbPath}"" -u {githubRawUri}/{githubOwner}/{githubRepo}");
		}
		else
		{
			Warning("Skipping source index for prerelease.");
		}

		version = GetSemVerFromFile(GetFiles($"src/**/bin/**/{coverageAssemblies[0]}.dll").First().ToString());
	});

Task("NuGetPackage")
	.IsDependentOn("SourceIndex")
	.Does(() =>
	{
		CreateDirectory("release");

		foreach (var nuspecPath in GetFiles($"src/**/*.nuspec"))
		{
			NuGetPack(nuspecPath, new NuGetPackSettings
			{
				Version = version,
				OutputDirectory = "release",
			});
		}
	});

Task("NuGetPublish")
	.IsDependentOn("NuGetPackage")
	.WithCriteria(() => !string.IsNullOrEmpty(nugetApiKey) && !string.IsNullOrEmpty(githubApiKey))
	.Does(() =>
	{
		foreach (var nupkgPath in GetFiles($"release/*.nupkg"))
		{
			NuGetPush(nupkgPath, new NuGetPushSettings
			{
				ApiKey = nugetApiKey,
				Source = nugetSource,
			});
		}

		if (headSha != null)
		{
			var tagName = $"nuget-{version}";
			Information($"Creating git tag '{tagName}'...");
			githubClient.Git.Reference.Create(githubOwner, githubRepo,
				new Octokit.NewReference($"refs/tags/{tagName}", headSha)).GetAwaiter().GetResult();
		}
		else
		{
			Warning("Skipping git tag for prerelease.");
		}
	});

Task("Coverage")
	.IsDependentOn("Build")
	.Does(() =>
	{
		CreateDirectory("release");
		if (FileExists("release/coverage.xml"))
			DeleteFile("release/coverage.xml");

		string filter = string.Concat(coverageAssemblies.Select(x => $@" ""-filter:+[{x}]*"""));

		foreach (var testDllPath in GetFiles($"tests/**/bin/**/*.Tests.dll"))
		{
			ExecuteProcess(@"cake\OpenCover\tools\OpenCover.Console.exe",
				$@"-register:user -mergeoutput ""-target:cake\NUnit.ConsoleRunner\tools\nunit3-console.exe"" ""-targetargs:{testDllPath} --noresult"" ""-output:release\coverage.xml"" -skipautoprops -returntargetcode" + filter);
		}
	});

Task("CoverageReport")
	.IsDependentOn("Coverage")
	.Does(() =>
	{
		ExecuteProcess(@"cake\ReportGenerator\tools\ReportGenerator.exe", $@"""-reports:release\coverage.xml"" ""-targetdir:release\coverage""");
	});

Task("CoveragePublish")
	.IsDependentOn("Coverage")
	.Does(() =>
	{
		ExecuteProcess(@"cake\coveralls.io\tools\coveralls.net.exe", $@"--opencover ""release\coverage.xml"" --full-sources --repo-token {coverallsApiKey}");
	});

Task("Default")
	.IsDependentOn("Test");

string GetSemVerFromFile(string path)
{
	var versionInfo = System.Diagnostics.FileVersionInfo.GetVersionInfo(path);
	var semver = $"{versionInfo.FileMajorPart}.{versionInfo.FileMinorPart}.{versionInfo.FileBuildPart}";
	if (prerelease.Length != 0)
		semver += $"-{prerelease}";
	return semver;
}

void GenerateExample(bool verify)
{
	int exitCode = StartProcess($@"src\XmlDocMarkdown\bin\{configuration}\XmlDocMarkdown.exe",
		$@"tests\ExampleAssembly\bin\{configuration}\ExampleAssembly.dll docs\ --clean --source ../tests/ExampleAssembly" + (verify ? " --verify" : ""));
	if (exitCode == 1 && verify)
		throw new InvalidOperationException("Generated docs don't match; use -target=GenerateDocs to regenerate.");
	else if (exitCode != 0)
		throw new InvalidOperationException($"Docs generation failed with exit code {exitCode}.");
}

void ExecuteProcess(string exePath, string arguments)
{
	int exitCode = StartProcess(exePath, arguments);
	if (exitCode != 0)
		throw new InvalidOperationException($"{exePath} failed with exit code {exitCode}.");
}

RunTarget(target);
