using System;
using System.Linq;
using System.Runtime.InteropServices;
using Faithlife.Build;
using static Faithlife.Build.AppRunner;

internal static class Build
{
	public static int Main(string[] args) => BuildRunner.Execute(args, build =>
	{
		var dotNetBuildSettings = new DotNetBuildSettings();
		build.AddDotNetTargets(dotNetBuildSettings);

		build.Target("generate-docs")
			.Describe("Generates documentation")
			.DependsOn("build")
			.Does(() => generateDocs(verify: false));

		build.Target("verify-docs")
			.Describe("Verifies generated documentation")
			.DependsOn("build")
			.Does(() => generateDocs(verify: false));

		build.Target("test")
			.DependsOn("verify-docs");

		build.Target("default").DependsOn("build");

		void generateDocs(bool verify)
		{
			var configuration = dotNetBuildSettings.BuildOptions.ConfigurationOption.Value;
			foreach ((string assemblyPath, string settingsPath) in new[]
			{
				($"tools/XmlDocTarget/bin/{configuration}/net47/XmlDocMarkdown.Core.dll", "src/XmlDocMarkdown.Core/XmlDocSettings.json"),
				($"tools/XmlDocTarget/bin/{configuration}/net47/Cake.XmlDocMarkdown.dll", "src/Cake.XmlDocMarkdown/XmlDocSettings.json"),
				($"tools/XmlDocTarget/bin/{configuration}/net47/ExampleAssembly.dll", "tools/ExampleAssembly/XmlDocSettings.json"),
			})
			{
				RunDotNetFrameworkApp($"src/XmlDocMarkdown/bin/{configuration}/XmlDocMarkdown.exe",
					assemblyPath, "docs", "--settings", settingsPath, verify ? "--verify" : null);
			}
		}
	});

	private static void RunDotNetFrameworkApp(string path, params string[] args)
	{
		if (IsPlatform(OSPlatform.OSX) || IsPlatform(OSPlatform.Linux))
		{
			args = new[] { path }.Concat(args).ToArray();
			path = "mono";
		}

		RunApp(path, args);
	}

	private static bool IsPlatform(OSPlatform platform)
	{
		try
		{
			return RuntimeInformation.IsOSPlatform(platform);
		}
		catch (PlatformNotSupportedException)
		{
			return false;
		}
	}
}
