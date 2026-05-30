using XmlDocGen.Core;

return XmlDocGenApp.Run(args,
	context =>
	{
		context.NewLine = "\n";
		context.WriterSettings.ShouldClean = true;
	});
