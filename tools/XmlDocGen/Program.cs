using XmlDocMarkdown.Core;

return XmlDocMarkdownApp.Run(args,
	(assembly, settings) =>
	{
		settings.NewLine = "\n";

		if (assembly == "ExampleAssembly")
		{
			settings.SourceCodePath = "../tests/ExampleAssembly";
		}
		else if (assembly == "XmlDocMarkdown.Core")
		{
			settings.SourceCodePath = "../src/XmlDocMarkdown.Core";
		}
		else
		{
			throw new InvalidOperationException($"Unexpected assembly: {assembly}");
		}
	});
