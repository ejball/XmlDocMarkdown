using XmlDocGen.Core.Nodes;
using XmlDocGen.Core.Sites;

namespace XmlDocGen.Core.Markdown;

/// <summary>Convenience builder for Markdown sites.</summary>
public sealed class MarkdownSiteBuilder
{
	/// <summary>Initializes a new instance of the <see cref="MarkdownSiteBuilder"/> class.</summary>
	public MarkdownSiteBuilder(XmlDocSiteBuilderSettings? settings = null, MarkdownPageRenderer? renderer = null)
	{
		Settings = settings ?? new XmlDocSiteBuilderSettings();
		Renderer = renderer ?? new MarkdownPageRenderer();
	}

	/// <summary>Gets the settings.</summary>
	public XmlDocSiteBuilderSettings Settings { get; }

	/// <summary>Gets the renderer.</summary>
	public MarkdownPageRenderer Renderer { get; }

	/// <summary>Builds a Markdown site.</summary>
	public XmlDocSite Build(XmlDocTree tree) => new XmlDocSiteBuilder(Renderer, Settings).Build(tree);
}
