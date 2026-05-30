return BuildRunner.Execute(args, build =>
{
	var dotNetBuildSettings = new DotNetBuildSettings
	{
		NuGetApiKey = Environment.GetEnvironmentVariable("NUGET_API_KEY"),
		PackageSettings = new DotNetPackageSettings
		{
			PushTagOnPublish = x => $"v{x.Version}",
		},
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
		var xmlDocGenPath = FindFiles($"artifacts/bin/XmlDocGen/{configuration}/XmlDocGen.dll").First();
		RunDotNet(xmlDocGenPath, "ExampleAssembly", "docs", verify ? "--verify" : null);
		RunDotNet(xmlDocGenPath, "XmlDocGen.Core", "docs", verify ? "--verify" : null);
	}
});
