using System.Reflection;
using XmlDocGen.Core.IO;
using XmlDocGen.Core.Markdown;
using XmlDocGen.Core.Nodes;
using XmlDocGen.Core.Pages;
using XmlDocGen.Core.Sites;
using XmlDocGen.Core.Xml;

namespace XmlDocGen.Core;

/// <summary>Command-line entry point for documentation generation.</summary>
public static class XmlDocGenApp
{
	/// <summary>Runs the command-line application.</summary>
	public static int Run(IReadOnlyList<string> args, Action<XmlDocGenAppContext>? configure = null)
	{
		try
		{
			var reader = new XmlDocArgsReader(args);
			if (reader.ReadFlag("help|h|?"))
			{
				WriteUsage(Console.Out, []);
				return 0;
			}

			var isVerify = reader.ReadFlag("verify");
			var writerSettings = new XmlDocSiteWriterSettings
			{
				ShouldClean = reader.ReadFlag("clean"),
				IsQuiet = reader.ReadFlag("quiet"),
				IsDryRun = isVerify || reader.ReadFlag("dryrun"),
			};

			var positionals = reader.ReadRemainingArguments();
			if (positionals.Count < 2)
				throw new XmlDocArgsReaderException("Expected one or more input assemblies and an output directory.");

			var context = new XmlDocGenAppContext(positionals.Take(positionals.Count - 1).ToList(), positionals[^1], reader, writerSettings);
			configure?.Invoke(context);
			reader.VerifyComplete();

			var inputs = context.AssemblyNames.Select(LoadInput);
			var tree = XmlDocTree.Create(inputs);
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
		return (assembly, XmlDocXmlFile.Load(xmlPath));
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
}

/// <summary>Configuration context passed to host tools.</summary>
public sealed class XmlDocGenAppContext
{
	internal XmlDocGenAppContext(IReadOnlyList<string> assemblyNames, string outputPath, XmlDocArgsReader args, XmlDocSiteWriterSettings writerSettings)
	{
		AssemblyNames = assemblyNames;
		OutputPath = outputPath;
		Args = args;
		WriterSettings = writerSettings;
	}

	/// <summary>Gets the assembly names to document.</summary>
	public IReadOnlyList<string> AssemblyNames { get; }

	/// <summary>Gets the output path.</summary>
	public string OutputPath { get; }

	/// <summary>Gets or sets the visibility filter.</summary>
	public XmlDocNodeVisibility Visibility { get; set; } = XmlDocNodeVisibility.Protected;

	/// <summary>Gets or sets the page map.</summary>
	public XmlDocPageMap PageMap { get; set; } = XmlDocPageMap.PerMember;

	/// <summary>Gets or sets the page renderer.</summary>
	public XmlDocPageRenderer Renderer { get; set; } = new MarkdownPageRenderer();

	/// <summary>Gets or sets the URL mapper.</summary>
	public XmlDocUrlMapper UrlMapper { get; set; } = XmlDocUrlMapper.GitHub;

	/// <summary>Gets or sets the external-link resolver.</summary>
	public XmlDocExternalLinkResolver ExternalLinks { get; set; } = XmlDocExternalLinkResolver.DotNetApi;

	/// <summary>Gets or sets the source-link resolver.</summary>
	public XmlDocSourceLinks? SourceLinks { get; set; }

	/// <summary>Gets or sets the generated newline sequence.</summary>
	public string? NewLine { get; set; }

	/// <summary>Gets writer settings.</summary>
	public XmlDocSiteWriterSettings WriterSettings { get; }

	/// <summary>Gets the argument reader for host-tool options.</summary>
	public XmlDocArgsReader Args { get; }

	/// <summary>Gets extra usage lines for host-tool options.</summary>
	public IList<string> HelpLines { get; } = [];
}

/// <summary>Reads command-line arguments.</summary>
public sealed class XmlDocArgsReader
{
	/// <summary>Initializes a new instance of the <see cref="XmlDocArgsReader"/> class.</summary>
	public XmlDocArgsReader(IEnumerable<string> args)
	{
		m_args = [.. args];
	}

	/// <summary>Reads a flag by long or short name.</summary>
	public bool ReadFlag(string name)
	{
		var names = name.Split('|');
		if (names.Length > 1)
			return names.Any(ReadFlag);

		var index = FindOptionArgumentIndex(name);
		if (index == -1)
			return false;
		m_args.RemoveAt(index);
		return true;
	}

	/// <summary>Reads an option value by long or short name.</summary>
	public string? ReadOption(string name)
	{
		var names = name.Split('|');
		if (names.Length > 1)
			return names.Select(ReadOption).FirstOrDefault(x => x is not null);

		var index = FindOptionArgumentIndex(name);
		if (index == -1)
			return null;
		var value = index + 1 < m_args.Count ? m_args[index + 1] : null;
		if (value is null || IsOption(value))
			throw new XmlDocArgsReaderException($"Missing value after '{RenderOption(name)}'.");
		m_args.RemoveAt(index);
		m_args.RemoveAt(index);
		return value;
	}

	/// <summary>Reads one positional argument.</summary>
	public string? ReadArgument()
	{
		if (m_args.Count == 0)
			return null;
		var value = m_args[0];
		if (IsOption(value))
			throw new XmlDocArgsReaderException($"Unexpected option '{value}'.");
		m_args.RemoveAt(0);
		return value;
	}

	/// <summary>Reads remaining positional arguments.</summary>
	public IReadOnlyList<string> ReadRemainingArguments()
	{
		var arguments = new List<string>();
		while (m_args.Count != 0 && !IsOption(m_args[0]))
			arguments.Add(ReadArgument()!);
		return arguments;
	}

	/// <summary>Verifies that no unread arguments remain.</summary>
	public void VerifyComplete()
	{
		if (m_args.Count != 0)
			throw new XmlDocArgsReaderException($"Unexpected {(IsOption(m_args[0]) ? "option" : "argument")} '{m_args[0]}'.");
	}

	private static bool IsOption(string value) => value.Length >= 2 && value[0] == '-' && value != "--";

	private static string RenderOption(string name) => name.Length == 1 ? $"-{name}" : $"--{name}";

	private static bool IsOptionArgument(string optionName, string argument) => argument == RenderOption(optionName);

	private int FindOptionArgumentIndex(string optionName)
	{
		for (var index = 0; index < m_args.Count; index++)
		{
			if (IsOptionArgument(optionName, m_args[index]))
				return index;
		}
		return -1;
	}

	private readonly List<string> m_args;
}

/// <summary>An exception thrown for invalid command-line arguments.</summary>
public sealed class XmlDocArgsReaderException(string message) : Exception(message);
