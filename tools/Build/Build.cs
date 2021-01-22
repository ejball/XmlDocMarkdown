using System;
using System.Linq;
using Faithlife.Build;
using static Faithlife.Build.AppRunner;
using static Faithlife.Build.BuildUtility;

return BuildRunner.Execute(args, build =>
{
	var dotNetBuildSettings = new DotNetBuildSettings
	{
		NuGetApiKey = Environment.GetEnvironmentVariable("NUGET_API_KEY"),
	};

	build.AddDotNetTargets(dotNetBuildSettings);

	build.Target("generate-docs")
		.Describe("Generates documentation")
		.DependsOn("build")
		.Does(() => GenerateDocs(verify: false));

	build.Target("verify-docs")
		.Describe("Verifies generated documentation")
		.DependsOn("build")
		.Does(() => GenerateDocs(verify: true));

	build.Target("test")
		.DependsOn("verify-docs");

	void GenerateDocs(bool verify)
	{
		var configuration = dotNetBuildSettings.GetConfiguration();
		var projects = new[]
		{
			new[] { "XmlDocMarkdown.Core", "docs", verify ? "--verify" : null, "--source", "../src/XmlDocMarkdown.Core", "--newline", "lf", "--clean" },
			new[] { "ExampleAssembly", "docs", verify ? "--verify" : null, "--source", "../tests/ExampleAssembly", "--newline", "lf", "--clean" },
		};
		var xmlDocGenPath = FindFiles($"tools/XmlDocGen/bin/{configuration}/net*/XmlDocGen.exe").First();
		foreach (var args in projects)
			RunApp(xmlDocGenPath, new AppRunnerSettings { Arguments = args, IsFrameworkApp = true });
	}
});
