using Faithlife.Build;
using static Faithlife.Build.AppRunner;

internal static class Build
{
	public static int Main(string[] args) => BuildRunner.Execute(args, build =>
	{
		var dotNetBuildSettings = new DotNetBuildSettings
		{
			SourceLinkSettings = new SourceLinkSettings
			{ 
				ShouldTestPackage = name => name == "XmlDocMarkdown.Core" || name == "Cake.XmlDocMarkdown",
			},
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
			($"tools/XmlDocTarget/bin/{configuration}/net47/XmlDocMarkdown.Core.dll", "src/XmlDocMarkdown.Core/XmlDocSettings.json"),
			($"tools/XmlDocTarget/bin/{configuration}/net47/Cake.XmlDocMarkdown.dll", "src/Cake.XmlDocMarkdown/XmlDocSettings.json"),
			($"tools/XmlDocTarget/bin/{configuration}/net47/ExampleAssembly.dll", "tools/ExampleAssembly/XmlDocSettings.json"),
		};
		foreach ((string assemblyPath, string settingsPath) in projects)
			RunDotNetFrameworkApp($"src/XmlDocMarkdown/bin/{configuration}/XmlDocMarkdown.exe", assemblyPath, "docs", "--settings", settingsPath, verify ? "--verify" : null);
	}
}
