using XmlDocGen.Core.Nodes;

namespace XmlDocGen.Core.Pages;

/// <summary>Builds pages from a tree and page map.</summary>
public static class XmlDocPageBuilder
{
	/// <summary>Groups visible nodes into logical pages.</summary>
	public static IReadOnlyList<XmlDocPage> CreatePages(XmlDocTree tree, XmlDocNodeVisibility visibility, XmlDocPageMap map) =>
		[.. tree.EnumerateNodes(visibility)
			.GroupBy(map.GetPagePath)
			.OrderBy(x => x.Key, StringComparer.OrdinalIgnoreCase)
			.Select(x => new XmlDocPage(x.Key, x))];
}
