using XmlDocMarkdown.Core;

return XmlDocMarkdownApp.Run(args,
	(assembly, settings) =>
	{
		settings.NewLine = "\n";
		settings.ShouldClean = true;
		settings.SourceCodePath = assembly switch
		{
			"ExampleAssembly" => "../tests/ExampleAssembly",
			"XmlDocMarkdown.Core" => "../src/XmlDocMarkdown.Core",
			_ => throw new InvalidOperationException($"Unexpected assembly: {assembly}"),
		};
	});
