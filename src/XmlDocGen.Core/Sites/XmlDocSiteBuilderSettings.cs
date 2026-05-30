using XmlDocGen.Core.Nodes;
using XmlDocGen.Core.Pages;

namespace XmlDocGen.Core.Sites;

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
