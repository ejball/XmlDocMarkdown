using System.Security.Cryptography;
using System.Text;
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
			var outputPath = Path.Combine("artifacts", "samples", sampleName);
			if (Directory.Exists(outputPath))
				Directory.Delete(outputPath, recursive: true);

			RunDotNet([samplePath, .. GetSampleArgs(sampleName, outputPath)]);

			var actual = CreateSampleSnapshot(sampleName, outputPath);
			var expectedPath = Path.Combine("samples", sampleName, "expected-output.txt");
			if (!verify)
			{
				File.WriteAllText(expectedPath, actual);
			}
			else if (!File.Exists(expectedPath) || File.ReadAllText(expectedPath) != actual)
			{
				throw new InvalidOperationException($"Sample output is out of date: {sampleName}. Run './build.ps1 generate-samples'.");
			}
		}
	}

	static string[] GetSampleArgs(string sampleName, string outputPath)
	{
		return sampleName switch
		{
			"Samples.LibraryApi" => ["ExampleAssembly", outputPath],
			"Samples.MultiAssembly" => ["ExampleAssembly", "XmlDocGen.Core", outputPath, "--clean", "--quiet"],
			"Samples.CustomCliOptions" => ["ExampleAssembly", outputPath, "--clean", "--quiet", "--public-only"],
			_ => ["ExampleAssembly", outputPath, "--clean", "--quiet"],
		};
	}

	static string CreateSampleSnapshot(string sampleName, string outputPath)
	{
		var files = Directory.EnumerateFiles(outputPath, "*", SearchOption.AllDirectories)
			.Select(path => Path.GetRelativePath(outputPath, path).Replace('\\', '/'))
			.Where(path => path != ".xmldocgen-manifest")
			.Order(StringComparer.Ordinal)
			.ToList();
		using var sha256 = SHA256.Create();
		foreach (var file in files)
		{
			var text = NormalizeSampleText(sampleName, File.ReadAllText(Path.Combine(outputPath, file.Replace('/', Path.DirectorySeparatorChar))).ReplaceLineEndings("\n"));
			var bytes = Encoding.UTF8.GetBytes(file + "\n" + text + "\n");
			sha256.TransformBlock(bytes, 0, bytes.Length, null, 0);
		}
		sha256.TransformFinalBlock([], 0, 0);

		var builder = new StringBuilder();
		builder.AppendLine("sample: " + sampleName);
		builder.AppendLine("files: " + files.Count);
		builder.AppendLine("sha256: " + Convert.ToHexString(sha256.Hash!).ToLowerInvariant());
		builder.AppendLine("preview:");
		foreach (var file in files.Take(12))
			builder.AppendLine("- " + file);
		return builder.ToString();
	}

	static string NormalizeSampleText(string sampleName, string text) => sampleName == "Samples.SourceLinks" ? Regex.Replace(text, "(XmlDocMarkdown/)[0-9a-f]{40}/", "$1{commit}/", RegexOptions.IgnoreCase) : text;
});
