using System.Text.RegularExpressions;

return BuildRunner.Execute(args, build =>
{
	var sampleNames = new[]
	{
		"Samples.Default",
		"Samples.PagePerType",
		"Samples.PagePerNamespace",
		"Samples.SinglePage",
		"Samples.CustomPageMap",
		"Samples.DocusaurusUrls",
		"Samples.CustomUrlMapper",
		"Samples.ExternalLinks",
		"Samples.SourceLinks",
		"Samples.FrontMatter",
		"Samples.CustomMarkdown",
		"Samples.HtmlRenderer",
		"Samples.CustomCliOptions",
		"Samples.MultiAssembly",
		"Samples.LibraryApi",
	};

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

	build.Target("generate-samples")
		.Describe("Generates sample output snapshots")
		.DependsOn("build")
		.Does(() => RunSamples(verify: false));

	build.Target("verify-samples")
		.Describe("Verifies sample output snapshots")
		.DependsOn("build")
		.Does(() => RunSamples(verify: true));

	build.Target("test")
		.DependsOn("verify-docs")
		.DependsOn("verify-samples");

	void GenerateDocs(bool verify)
	{
		var configuration = dotNetBuildSettings.GetConfiguration();
		var xmlDocGenPath = FindFiles($"artifacts/bin/XmlDocGen/{configuration}/XmlDocGen.dll").First();
		RunDotNet(xmlDocGenPath, "ExampleAssembly", "XmlDocGen.Core", "docs", verify ? "--verify" : null);
	}

	void RunSamples(bool verify)
	{
		var configuration = dotNetBuildSettings.GetConfiguration();
		foreach (var sampleName in sampleNames)
		{
			var samplePath = FindFiles($"artifacts/bin/{sampleName}/{configuration}/{sampleName}.dll").First();
			var expectedPath = Path.Combine("samples", sampleName, "output");
			var outputPath = verify ? Path.Combine("artifacts", "samples", sampleName) : expectedPath;
			if (Directory.Exists(outputPath))
				Directory.Delete(outputPath, recursive: true);

			RunDotNet([samplePath, .. GetSampleArgs(sampleName, outputPath)]);
			if (verify && !SampleOutputMatches(sampleName, outputPath, expectedPath))
			{
				throw new InvalidOperationException($"Sample output is out of date: {sampleName}. Run './build.ps1 generate-samples'.");
			}
		}
	}

	static string[] GetSampleArgs(string sampleName, string outputPath)
	{
		return sampleName switch
		{
			"Samples.LibraryApi" => ["SampleAssembly", outputPath],
			"Samples.MultiAssembly" => ["SampleAssembly", "SampleExtraAssembly", outputPath, "--clean", "--quiet"],
			"Samples.CustomCliOptions" => ["SampleAssembly", outputPath, "--clean", "--quiet", "--public-only"],
			_ => ["SampleAssembly", outputPath, "--clean", "--quiet"],
		};
	}

	static bool SampleOutputMatches(string sampleName, string actualPath, string expectedPath)
	{
		if (!Directory.Exists(expectedPath))
			return false;

		var actualFiles = GetSampleFiles(actualPath);
		var expectedFiles = GetSampleFiles(expectedPath);
		if (!actualFiles.SequenceEqual(expectedFiles, StringComparer.Ordinal))
			return false;

		foreach (var file in actualFiles)
		{
			var actual = NormalizeSampleText(sampleName, File.ReadAllText(Path.Combine(actualPath, file.Replace('/', Path.DirectorySeparatorChar))).ReplaceLineEndings("\n"));
			var expected = NormalizeSampleText(sampleName, File.ReadAllText(Path.Combine(expectedPath, file.Replace('/', Path.DirectorySeparatorChar))).ReplaceLineEndings("\n"));
			if (actual != expected)
				return false;
		}
		return true;
	}

	static List<string> GetSampleFiles(string outputPath)
	{
		return Directory.EnumerateFiles(outputPath, "*", SearchOption.AllDirectories)
			.Select(path => Path.GetRelativePath(outputPath, path).Replace('\\', '/'))
			.Order(StringComparer.Ordinal)
			.ToList();
	}

	static string NormalizeSampleText(string sampleName, string text) => sampleName == "Samples.SourceLinks" ? Regex.Replace(text, "(XmlDocMarkdown/)[0-9a-f]{40}/", "$1{commit}/", RegexOptions.IgnoreCase) : text;
});
