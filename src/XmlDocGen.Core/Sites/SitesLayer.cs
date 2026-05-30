using XmlDocGen.Core.Nodes;
using XmlDocGen.Core.Pages;

namespace XmlDocGen.Core.Sites;

/// <summary>A generated documentation site.</summary>
public sealed class XmlDocSite
{
	/// <summary>Initializes a new instance of the <see cref="XmlDocSite"/> class.</summary>
	public XmlDocSite(IEnumerable<XmlDocSiteFile> files)
	{
		Files = [.. files.OrderBy(x => x.Path, StringComparer.OrdinalIgnoreCase)];
		m_filesByPath = Files.ToDictionary(x => x.Path, StringComparer.OrdinalIgnoreCase);
	}

	/// <summary>Gets generated files in deterministic order.</summary>
	public IReadOnlyList<XmlDocSiteFile> Files { get; }

	/// <summary>Finds a generated file by path.</summary>
	public XmlDocSiteFile? FindFile(string path) => m_filesByPath.GetValueOrDefault(path);

	private readonly IReadOnlyDictionary<string, XmlDocSiteFile> m_filesByPath;
}

/// <summary>A single generated output file.</summary>
public sealed record XmlDocSiteFile(string Path, string Text);

/// <summary>Builds a generated documentation site.</summary>
public sealed class XmlDocSiteBuilder
{
	/// <summary>Initializes a new instance of the <see cref="XmlDocSiteBuilder"/> class.</summary>
	public XmlDocSiteBuilder(XmlDocPageRenderer renderer, XmlDocSiteBuilderSettings? settings = null)
	{
		Renderer = renderer;
		Settings = settings ?? new XmlDocSiteBuilderSettings();
	}

	/// <summary>Gets the page renderer.</summary>
	public XmlDocPageRenderer Renderer { get; }

	/// <summary>Gets the site-builder settings.</summary>
	public XmlDocSiteBuilderSettings Settings { get; }

	/// <summary>Builds a site from a tree.</summary>
	public XmlDocSite Build(XmlDocTree tree)
	{
		var pageBuilder = new XmlDocPageBuilder(Settings.PageMap, Settings.Visibility);
		var pages = pageBuilder.Build(tree);
		var files = new List<XmlDocSiteFile>();
		foreach (var page in pages)
		{
			var context = new XmlDocPageContext(tree, pages, page, Settings.UrlMapper ?? XmlDocUrlMapper.GitHub, Settings.ExternalLinks ?? XmlDocExternalLinkResolver.DotNetApi, Settings.SourceLinks);
			var rendered = Renderer.RenderPage(page, context);
			files.Add(new XmlDocSiteFile(rendered.Path, NormalizeNewLines(rendered.Text, Settings.NewLine)));
		}
		return new XmlDocSite(files);
	}

	private static string NormalizeNewLines(string text, string? newLine) => newLine is null ? text : text.ReplaceLineEndings(newLine);
}

/// <summary>Settings for building a site.</summary>
public sealed class XmlDocSiteBuilderSettings
{
	/// <summary>Gets or sets the visibility filter.</summary>
	public XmlDocNodeVisibility? Visibility { get; set; }

	/// <summary>Gets or sets the page map.</summary>
	public XmlDocPageMap? PageMap { get; set; }

	/// <summary>Gets or sets the URL mapper.</summary>
	public XmlDocUrlMapper? UrlMapper { get; set; }

	/// <summary>Gets or sets the external-link resolver.</summary>
	public XmlDocExternalLinkResolver? ExternalLinks { get; set; }

	/// <summary>Gets or sets the source-link resolver.</summary>
	public XmlDocSourceLinks? SourceLinks { get; set; }

	/// <summary>Gets or sets the generated newline sequence.</summary>
	public string? NewLine { get; set; }
}