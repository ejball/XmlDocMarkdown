using System.Reflection;
using XmlDocGen.Core.Nodes;
using XmlDocGen.Core.Xml;

namespace XmlDocGen.Core.Pages;

/// <summary>A generated documentation page before it is rendered.</summary>
public sealed class XmlDocPage
{
	/// <summary>Initializes a new instance of the <see cref="XmlDocPage"/> class.</summary>
	public XmlDocPage(string path, IEnumerable<XmlDocNode> nodes)
	{
		Path = path;
		Nodes = [.. nodes];
	}

	/// <summary>Gets the extensionless site-relative page path.</summary>
	public string Path { get; }

	/// <summary>Gets the nodes documented on this page.</summary>
	public IReadOnlyList<XmlDocNode> Nodes { get; }
}

/// <summary>Maps documentation nodes to extensionless page paths.</summary>
public abstract class XmlDocPageMap
{
	/// <summary>Gets the default one-file-per-member map.</summary>
	public static XmlDocPageMap PerMember { get; } = new PresetPageMap(GetPerMemberPath);

	/// <summary>Gets a one-file-per-type map.</summary>
	public static XmlDocPageMap PerType { get; } = new PresetPageMap(GetPerTypePath);

	/// <summary>Gets a one-file-per-namespace map.</summary>
	public static XmlDocPageMap PerNamespace { get; } = new PresetPageMap(node => node is XmlDocAssemblyNode ? GetAssemblyPath(node.Assembly) : GetNamespacePath(GetNamespace(node)));

	/// <summary>Gets a one-file-per-assembly map.</summary>
	public static XmlDocPageMap PerAssembly { get; } = new PresetPageMap(node => GetAssemblyPath(node.Assembly));

	/// <summary>Gets a single-page map.</summary>
	public static XmlDocPageMap SinglePage { get; } = new PresetPageMap(_ => "index");

	/// <summary>Gets the extensionless page path for a node.</summary>
	public abstract string GetPagePath(XmlDocNode node);

	/// <summary>Returns a URL-safe file-name component for a node.</summary>
	public static string GetSafeName(XmlDocNode node) => GetSafeName(node.Name);

	/// <summary>Returns a URL-safe file-name component.</summary>
	public static string GetSafeName(string name) => name.Replace('<', '-').Replace('>', '-').Replace('`', '-').Replace(' ', '-');

	private static string GetPerMemberPath(XmlDocNode node)
	{
		return node switch
		{
			XmlDocAssemblyNode assembly => GetAssemblyPath(assembly),
			XmlDocNamespaceNode namespaceNode => GetNamespacePath(namespaceNode),
			XmlDocTypeNode typeNode => GetTypePath(typeNode),
			XmlDocMemberNode memberNode => GetTypePath((XmlDocTypeNode) memberNode.Parent!) + "/" + GetSafeName(memberNode),
			_ => GetSafeName(node),
		};
	}

	private static string GetPerTypePath(XmlDocNode node)
	{
		return node switch
		{
			XmlDocAssemblyNode assembly => GetAssemblyPath(assembly),
			XmlDocNamespaceNode namespaceNode => GetNamespacePath(namespaceNode),
			XmlDocMemberNode memberNode => GetTypePath((XmlDocTypeNode) memberNode.Parent!),
			XmlDocTypeNode typeNode => GetTypePath(typeNode),
			_ => GetSafeName(node),
		};
	}

	private static string GetAssemblyPath(XmlDocAssemblyNode assembly) => GetSafeName(assembly.Name);

	private static string GetNamespacePath(XmlDocNode node) => GetAssemblyPath(node.Assembly) + "/" + GetSafeName(node.Name);

	private static string GetTypePath(XmlDocTypeNode type) => GetNamespacePath(GetNamespace(type)) + "/" + GetSafeName(type);

	private static XmlDocNamespaceNode GetNamespace(XmlDocNode node)
	{
		var current = node;
		while (current is not XmlDocNamespaceNode)
			current = current.Parent ?? throw new InvalidOperationException("Node is not under a namespace.");
		return (XmlDocNamespaceNode) current;
	}

	private sealed class PresetPageMap(Func<XmlDocNode, string> getPath) : XmlDocPageMap
	{
		public override string GetPagePath(XmlDocNode node) => getPath(node);
	}
}

/// <summary>Builds pages from a tree and page map.</summary>
public sealed class XmlDocPageBuilder
{
	/// <summary>Initializes a new instance of the <see cref="XmlDocPageBuilder"/> class.</summary>
	public XmlDocPageBuilder(XmlDocPageMap? pageMap = null, XmlDocNodeVisibility? visibility = null)
	{
		PageMap = pageMap ?? XmlDocPageMap.PerMember;
		Visibility = visibility ?? XmlDocNodeVisibility.Protected;
	}

	/// <summary>Gets the page map.</summary>
	public XmlDocPageMap PageMap { get; }

	/// <summary>Gets the visibility filter.</summary>
	public XmlDocNodeVisibility Visibility { get; }

	/// <summary>Builds pages from a tree.</summary>
	public IReadOnlyList<XmlDocPage> Build(XmlDocTree tree) =>
		[.. tree.DescendantsAndSelf()
			.Where(Visibility.Includes)
			.GroupBy(PageMap.GetPagePath)
			.OrderBy(x => x.Key, StringComparer.OrdinalIgnoreCase)
			.Select(x => new XmlDocPage(x.Key, x))];
}

/// <summary>A rendered page file.</summary>
public sealed record XmlDocRenderedFile(string Path, string Text);

/// <summary>Renders a page to a file.</summary>
public abstract class XmlDocPageRenderer
{
	/// <summary>Renders a page to a file.</summary>
	public abstract XmlDocRenderedFile RenderPage(XmlDocPage page, XmlDocPageContext context);
}

/// <summary>Context available while rendering a page.</summary>
public sealed class XmlDocPageContext
{
	/// <summary>Initializes a new instance of the <see cref="XmlDocPageContext"/> class.</summary>
	public XmlDocPageContext(XmlDocTree tree, IReadOnlyList<XmlDocPage> pages, XmlDocPage currentPage, XmlDocUrlMapper urlMapper, XmlDocExternalLinkResolver externalLinks, XmlDocSourceLinks? sourceLinks)
	{
		Tree = tree;
		Pages = pages;
		CurrentPage = currentPage;
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
	public XmlDocPage CurrentPage { get; }

	/// <summary>Gets the URL mapper.</summary>
	public XmlDocUrlMapper UrlMapper { get; }

	/// <summary>Gets the external-link resolver.</summary>
	public XmlDocExternalLinkResolver ExternalLinks { get; }

	/// <summary>Gets the source-link resolver, if enabled.</summary>
	public XmlDocSourceLinks? SourceLinks { get; }

	/// <summary>Gets a URL for a reflected member.</summary>
	public string? GetLinkUrl(MemberInfo member) => GetLinkUrl(XmlDocRef.ForMember(member));

	/// <summary>Gets a URL for an XML documentation reference.</summary>
	public string? GetLinkUrl(XmlDocRef reference)
	{
		var targetNode = Tree.FindNode(reference);
		if (targetNode is not null && m_pagesByNode.TryGetValue(targetNode, out var targetPage))
			return UrlMapper.GetUrl(CurrentPage, targetPage, targetNode);

		return ExternalLinks.TryGetUrl(reference, null);
	}

	private readonly IReadOnlyDictionary<XmlDocNode, XmlDocPage> m_pagesByNode;
}

/// <summary>Maps rendered pages and nodes to URLs.</summary>
public abstract class XmlDocUrlMapper
{
	/// <summary>Gets the GitHub-style URL mapper.</summary>
	public static XmlDocUrlMapper GitHub { get; } = new RelativeUrlMapper(".md");

	/// <summary>Gets the Docusaurus-style URL mapper.</summary>
	public static XmlDocUrlMapper Docusaurus { get; } = new RelativeUrlMapper("");

	/// <summary>Gets a URL from one page to a target node.</summary>
	public abstract string GetUrl(XmlDocPage fromPage, XmlDocPage targetPage, XmlDocNode targetNode);

	private sealed class RelativeUrlMapper(string extension) : XmlDocUrlMapper
	{
		public override string GetUrl(XmlDocPage fromPage, XmlDocPage targetPage, XmlDocNode targetNode)
		{
			var relative = MakeRelative(fromPage.Path + extension, targetPage.Path + extension);
			var fragment = targetPage.Nodes.Count > 1 ? "#" + Slug(targetNode.Name) : "";
			return relative + fragment;
		}

		private static string MakeRelative(string fromPath, string toPath)
		{
			var from = new Uri("file:///" + fromPath.Replace('\\', '/'));
			var to = new Uri("file:///" + toPath.Replace('\\', '/'));
			var value = from.MakeRelativeUri(to).OriginalString;
			return value.Length == 0 ? "./" + to.Segments.Last() : value[0] == '.' ? value : "./" + value;
		}

		private static string Slug(string value) => string.Concat(value.ToLowerInvariant().Select(ch => char.IsLetterOrDigit(ch) ? ch : '-')).Trim('-');
	}
}

/// <summary>Resolves links to documentation outside the current tree.</summary>
public abstract class XmlDocExternalLinkResolver
{
	/// <summary>Gets a resolver for Microsoft Learn .NET API documentation.</summary>
	public static XmlDocExternalLinkResolver DotNetApi { get; } = new DotNetApiResolver();

	/// <summary>Tries to resolve a URL for an external reference.</summary>
	public abstract string? TryGetUrl(XmlDocRef reference, MemberInfo? member);

	private sealed class DotNetApiResolver : XmlDocExternalLinkResolver
	{
		public override string? TryGetUrl(XmlDocRef reference, MemberInfo? member)
		{
			var value = reference.Value;
			if (value.StartsWith("N:System", StringComparison.Ordinal))
				return "https://learn.microsoft.com/dotnet/api/" + value[2..].ToLowerInvariant();
			if (value.StartsWith("T:System", StringComparison.Ordinal))
				return "https://learn.microsoft.com/dotnet/api/" + value[2..].Replace('`', '-').ToLowerInvariant();
			return member?.DeclaringType?.Namespace?.StartsWith("System", StringComparison.Ordinal) == true ? "https://learn.microsoft.com/dotnet/api/" + member.DeclaringType.FullName?.ToLowerInvariant() : null;
		}
	}
}

/// <summary>Provides source-link URLs for reflected members.</summary>
public sealed class XmlDocSourceLinks
{
	private XmlDocSourceLinks(Assembly assembly)
	{
		Assembly = assembly;
	}

	/// <summary>Gets the assembly this source-link resolver was created for.</summary>
	public Assembly Assembly { get; }

	/// <summary>Attempts to create source links for an assembly.</summary>
	public static XmlDocSourceLinks? TryCreate(Assembly assembly) => string.IsNullOrEmpty(assembly.Location) ? null : new XmlDocSourceLinks(assembly);

	/// <summary>Attempts to get a source URL for a member.</summary>
	public string? TryGetUrl(MemberInfo member) => null;
}