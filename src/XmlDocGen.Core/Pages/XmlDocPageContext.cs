using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Text.Json;
using XmlDocGen.Core.Nodes;
using XmlDocGen.Core.Xml;

namespace XmlDocGen.Core.Pages;

/// <summary>Context available while rendering a page.</summary>
public sealed class XmlDocPageContext
{
	/// <summary>Initializes a new instance of the <see cref="XmlDocPageContext"/> class.</summary>
	public XmlDocPageContext(XmlDocTree tree, IReadOnlyList<XmlDocPage> pages, XmlDocPage currentPage, XmlDocUrlMapper urlMapper, XmlDocExternalLinkResolver externalLinks, XmlDocSourceLinks? sourceLinks)
	{
		Tree = tree;
		Pages = pages;
		Page = currentPage;
		UrlMapper = urlMapper;
		ExternalLinks = externalLinks;
		SourceLinks = sourceLinks;
		m_pagesByNode = pages.SelectMany(page => page.Nodes.Select(node => (node, page))).ToDictionary(x => x.node, x => x.page);
	}

	/// <summary>Gets the documentation tree.</summary>
	public XmlDocTree Tree { get; }

	/// <summary>Gets all pages.</summary>
	public IReadOnlyList<XmlDocPage> Pages { get; }

	/// <summary>Gets the page currently being rendered.</summary>
	public XmlDocPage Page { get; }

	/// <summary>Gets the page currently being rendered.</summary>
	public XmlDocPage CurrentPage => Page;

	/// <summary>Gets the URL mapper.</summary>
	public XmlDocUrlMapper UrlMapper { get; }

	/// <summary>Gets the external-link resolver.</summary>
	public XmlDocExternalLinkResolver ExternalLinks { get; }

	/// <summary>Gets the source-link resolver, if enabled.</summary>
	public XmlDocSourceLinks? SourceLinks { get; }

	/// <summary>Gets a URL for a reflected member.</summary>
	public string? GetLinkUrl(MemberInfo member)
	{
		var reference = XmlDocRef.ForMember(member);
		var targetNode = Tree.FindNode(reference);
		if (targetNode is not null && m_pagesByNode.TryGetValue(targetNode, out var targetPage))
			return UrlMapper.GetUrl(Page, targetPage, targetNode);

		return ExternalLinks.TryGetUrl(reference, member);
	}

	/// <summary>Finds the page containing a node.</summary>
	public XmlDocPage? FindPage(XmlDocNode node) => m_pagesByNode.GetValueOrDefault(node);

	/// <summary>Finds the page containing a reference.</summary>
	public XmlDocPage? FindPage(XmlDocRef reference) => Tree.FindNode(reference) is { } node ? FindPage(node) : null;

	/// <summary>Gets a URL for an XML documentation reference.</summary>
	public string? GetLinkUrl(XmlDocRef reference)
	{
		var targetNode = Tree.FindNode(reference);
		if (targetNode is not null && m_pagesByNode.TryGetValue(targetNode, out var targetPage))
			return UrlMapper.GetUrl(Page, targetPage, targetNode);

		return ExternalLinks.TryGetUrl(reference, null);
	}

	/// <summary>Gets a source URL for a reflected member.</summary>
	public string? GetSourceUrl(MemberInfo member) => SourceLinks?.TryGetUrl(member);

	private readonly IReadOnlyDictionary<XmlDocNode, XmlDocPage> m_pagesByNode;
}
