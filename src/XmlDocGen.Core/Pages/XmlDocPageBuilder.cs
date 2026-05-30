using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Text.Json;
using XmlDocGen.Core.Nodes;
using XmlDocGen.Core.Xml;

namespace XmlDocGen.Core.Pages;

/// <summary>Builds pages from a tree and page map.</summary>
public static class XmlDocPageBuilder
{
	/// <summary>Groups visible nodes into logical pages.</summary>
	public static IReadOnlyList<XmlDocPage> CreatePages(XmlDocTree tree, XmlDocNodeVisibility visibility, XmlDocPageMap map) =>
		[.. tree.DescendantsAndSelf(visibility)
			.GroupBy(map.GetPagePath)
			.OrderBy(x => x.Key, StringComparer.OrdinalIgnoreCase)
			.Select(x => new XmlDocPage(x.Key, x))];
}
