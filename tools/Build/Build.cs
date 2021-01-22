using System;
using System.Linq;
using Faithlife.Build;
using static Faithlife.Build.BuildUtility;
using static Faithlife.Build.DotNetRunner;

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
			("XmlDocMarkdown.Core", "../src/XmlDocMarkdown.Core"),
			("ExampleAssembly", "../tests/ExampleAssembly"),
		};
		var xmlDocGenPath = FindFiles($"tools/XmlDocGen/bin/{configuration}/net*/XmlDocGen.dll").First();
		foreach (var (assembly, sourcePath) in projects)
			RunDotNet(xmlDocGenPath, assembly, "docs", verify ? "--verify" : null, "--source", sourcePath, "--newline", "lf", "--clean");
	}
});
