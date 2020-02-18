using System;
using Faithlife.Build;
using static Faithlife.Build.AppRunner;

internal static class Build
{
	public static int Main(string[] args) => BuildRunner.Execute(args, build =>
	{
		var dotNetBuildSettings = new DotNetBuildSettings
		{
			NuGetApiKey = Environment.GetEnvironmentVariable("NUGET_API_KEY"),
		};
		build.AddDotNetTargets(dotNetBuildSettings);

		build.Target("generate-docs")
			.Describe("Generates documentation")
			.DependsOn("build")
			.Does(() => GenerateDocs(dotNetBuildSettings, verify: false));

		build.Target("verify-docs")
			.Describe("Verifies generated documentation")
			.DependsOn("build")
			.Does(() => GenerateDocs(dotNetBuildSettings, verify: true));

		build.Target("test")
			.DependsOn("verify-docs");
	});

	private static void GenerateDocs(DotNetBuildSettings dotNetBuildSettings, bool verify)
	{
		var configuration = dotNetBuildSettings.BuildOptions.ConfigurationOption.Value;
		var projects = new[]
		{
			new[] { $"tools/XmlDocTarget/bin/{configuration}/net472/XmlDocMarkdown.Core.dll", "docs", verify ? "--verify" : null, "--source", "../src/XmlDocMarkdown.Core", "--newline", "lf", "--clean" },
			new[] { $"tools/XmlDocTarget/bin/{configuration}/net472/Cake.XmlDocMarkdown.dll", "docs", verify ? "--verify" : null, "--source", "../src/Cake.XmlDocMarkdown", "--external", "XmlDocMarkdown.Core", "--newline", "lf", "--clean" },
			new[] { $"tools/XmlDocTarget/bin/{configuration}/net472/ExampleAssembly.dll", "docs", verify ? "--verify" : null, "--source", "../tests/ExampleAssembly", "--newline", "lf", "--clean" },
		};
		foreach (var args in projects)
			RunDotNetFrameworkApp($"src/XmlDocMarkdown/bin/{configuration}/XmlDocMarkdown.exe", args);
	}
}
