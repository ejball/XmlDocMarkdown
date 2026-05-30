using System.Reflection;
using XmlDocGen.Core.IO;
using XmlDocGen.Core.Markdown;
using XmlDocGen.Core.Nodes;
using XmlDocGen.Core.Pages;
using XmlDocGen.Core.Sites;
using XmlDocGen.Core.Xml;

namespace XmlDocGen.Core;

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
