using System.Reflection;
using System.Xml.Linq;
using XmlDocGen.Core.IO;
using XmlDocGen.Core.Nodes;
using XmlDocGen.Core.Sites;
using XmlDocGen.Core.Xml;

namespace XmlDocGen.Core;

/// <summary>Command-line entry point for documentation generation.</summary>
public sealed class XmlDocGenApp
{
	/// <summary>Runs the command-line application.</summary>
	public static int Run(IReadOnlyList<string> args, Action<XmlDocGenAppContext>? configure = null)
	{
		try
		{
			var reader = new XmlDocArgsReader(args);
			var isHelp = reader.ReadFlag("help|h|?");
			var isVerify = reader.ReadFlag("verify");
			var writerSettings = new XmlDocSiteWriterSettings
			{
				ShouldClean = reader.ReadFlag("clean"),
				IsQuiet = reader.ReadFlag("quiet"),
				IsDryRun = isVerify || reader.ReadFlag("dryrun"),
			};
			if (isHelp)
			{
				var helpContext = new XmlDocGenAppContext([], "", reader, writerSettings);
				configure?.Invoke(helpContext);
				WriteUsage(Console.Out, helpContext.HelpLines);
				return 0;
			}

			var positionals = reader.ReadRemainingArguments();
			if (positionals.Count < 2)
				throw new XmlDocArgsReaderException("Expected one or more input assemblies and an output directory.");

			var context = new XmlDocGenAppContext(positionals.Take(positionals.Count - 1).ToList(), positionals[^1], reader, writerSettings);
			configure?.Invoke(context);
			reader.VerifyComplete();

			var inputs = context.AssemblyNames.Select(LoadInput);
			var tree = new XmlDocTree(inputs);
			var siteSettings = new XmlDocSiteBuilderSettings
			{
				Visibility = context.Visibility,
				PageMap = context.PageMap,
				UrlMapper = context.UrlMapper,
				ExternalLinks = context.ExternalLinks,
				SourceLinks = context.SourceLinks,
				NewLine = context.NewLine,
			};
			var site = new XmlDocSiteBuilder(context.Renderer, siteSettings).Build(tree);
			var result = new XmlDocSiteWriter(context.WriterSettings).Write(site, context.OutputPath);
			foreach (var message in result.Messages)
				Console.WriteLine(message);

			return isVerify && result.HasChanges ? 1 : 0;
		}
		catch (Exception exception) when (exception is XmlDocArgsReaderException or ApplicationException or IOException or UnauthorizedAccessException)
		{
			Console.Error.WriteLine(exception.Message);
			return exception is XmlDocArgsReaderException ? 2 : 3;
		}
	}

	private static (Assembly Assembly, XmlDocXmlFile Xml) LoadInput(string assemblyName)
	{
		var assembly = Assembly.Load(assemblyName);
		var xmlPath = Path.ChangeExtension(assembly.Location, ".xml");
		if (!File.Exists(xmlPath))
		{
			var altXmlPath = Path.ChangeExtension(assembly.Location, ".XML");
			if (File.Exists(altXmlPath))
				xmlPath = altXmlPath;
			else
				throw new ApplicationException($"Missing XML file: {xmlPath}");
		}
		return (assembly, new XmlDocXmlFile(XDocument.Load(xmlPath)));
	}

	private static void WriteUsage(TextWriter writer, IEnumerable<string> extraLines)
	{
		writer.WriteLine("Generates documentation from .NET XML documentation comments.");
		writer.WriteLine();
		writer.WriteLine($"Usage: {Assembly.GetEntryAssembly()?.GetName().Name ?? "XmlDocGen"} <input-assembly>... <output-dir> [options]");
		writer.WriteLine("  --clean     Delete previously generated files that are no longer used.");
		writer.WriteLine("  --dryrun    Run without writing to the file system.");
		writer.WriteLine("  --quiet     Suppress normal console output.");
		writer.WriteLine("  --verify    Exit with code 1 if changes are needed.");
		writer.WriteLine("  --help, -h, -?");
		foreach (var line in extraLines)
			writer.WriteLine(line);
	}

	private XmlDocGenApp()
	{
	}
}
