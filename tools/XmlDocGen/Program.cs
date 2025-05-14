using XmlDocMarkdown.Core;

return XmlDocMarkdownApp.Run(args,
	(assembly, settings) =>
	{
		settings.NewLine = "\n";
		settings.ShouldClean = true;
	});
