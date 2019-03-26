using System;
using Faithlife.Build;
using static Faithlife.Build.AppRunner;

internal static class Build
{
	public static int Main(string[] args) => BuildRunner.Execute(args, build =>
	{
		var dotNetBuildSettings = new DotNetBuildSettings
		{
			DocsSettings = new DotNetDocsSettings
			{
				GitLogin = new GitLoginInfo("ejball", Environment.GetEnvironmentVariable("BUILD_BOT_PASSWORD") ?? ""),
				GitAuthor = new GitAuthorInfo("ejball", "ejball@gmail.com"),
				SourceCodeUrl = "https://github.com/ejball/ArgsReading/tree/master/src",
			},
		};

		build.AddDotNetTargets(dotNetBuildSettings);

		build.Target("generate-example")
			.Describe("Generates documentation for ExampleAssembly")
			.DependsOn("build")
			.Does(() => generateDocs(verify: false));

		build.Target("verify-example")
			.Describe("Generates documentation for ExampleAssembly")
			.DependsOn("build")
			.Does(() => generateDocs(verify: false));

		build.Target("test")
			.DependsOn("verify-example");

		build.Target("default").DependsOn("build");

		void generateDocs(bool verify)
		{
			var configuration = dotNetBuildSettings.BuildOptions.ConfigurationOption.Value;
			RunApp($"src/XmlDocMarkdown/bin/{configuration}/XmlDocMarkdown.exe",
				$"tools/XmlDocTarget/bin/{configuration}/net47/ExampleAssembly.dll", "docs",
				"--settings", "tools/ExampleAssembly/XmlDocSettings.json", verify ? "--verify" : null);
		}
	});
}
