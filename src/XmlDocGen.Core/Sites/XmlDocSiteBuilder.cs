using XmlDocGen.Core.Nodes;
using XmlDocGen.Core.Pages;

namespace XmlDocGen.Core.Sites;

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
		var pages = XmlDocPageBuilder.CreatePages(tree, Settings.Visibility ?? XmlDocNodeVisibility.Protected, Settings.PageMap ?? XmlDocPageMap.PerMember);
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
