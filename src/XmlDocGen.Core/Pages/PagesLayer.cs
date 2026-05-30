using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Text.Json;
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
	public static string GetSafeName(string name) => string.Concat(name.Select(ch => char.IsLetterOrDigit(ch) || ch is '.' or '-' or '_' ? ch : '-')).Trim('-');

	private static string GetPerMemberPath(XmlDocNode node)
	{
		return node switch
		{
			XmlDocAssemblyNode assembly => GetAssemblyPath(assembly),
			XmlDocNamespaceNode namespaceNode => GetNamespacePath(namespaceNode),
			XmlDocTypeNode typeNode => GetTypePath(typeNode),
			XmlDocMemberNode memberNode => GetTypePath((XmlDocTypeNode) memberNode.Parent!) + "/" + GetMemberSafeName(memberNode),
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

	private static string GetMemberSafeName(XmlDocMemberNode member)
	{
		var sameNameCount = member.Parent?.Children.OfType<XmlDocMemberNode>().Count(x => x.Name == member.Name) ?? 0;
		return sameNameCount <= 1 ? GetSafeName(member.Name) : GetSafeName(XmlDocPageHeadings.GetHeadingText(member));
	}

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
public static class XmlDocPageBuilder
{
	/// <summary>Groups visible nodes into logical pages.</summary>
	public static IReadOnlyList<XmlDocPage> CreatePages(XmlDocTree tree, XmlDocNodeVisibility visibility, XmlDocPageMap map) =>
		[.. tree.DescendantsAndSelf(visibility)
			.GroupBy(map.GetPagePath)
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
			var fragment = targetNode == targetPage.Nodes[0] ? "" : "#" + Slug(XmlDocPageHeadings.GetHeadingText(targetPage, targetNode));
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

internal static class XmlDocPageHeadings
{
	public static string GetHeadingText(XmlDocPage page, XmlDocNode node)
	{
		return page.Nodes.Count(x => x.Name == node.Name) <= 1 ? node.Name : GetHeadingText(node);
	}

	public static string GetHeadingText(XmlDocNode node)
	{
		return node is XmlDocMemberNode member ? member.Name + GetGenericSuffix(member.Member) + GetParameterSuffix(member.Member) : node.Name;
	}

	private static string GetGenericSuffix(MemberInfo member)
	{
		return member is MethodInfo method && method.GetGenericArguments().Length != 0 ? "<" + string.Join(", ", method.GetGenericArguments().Select(x => x.Name)) + ">" : "";
	}

	private static string GetParameterSuffix(MemberInfo member)
	{
		var parameters = member switch
		{
			ConstructorInfo constructor => constructor.GetParameters(),
			MethodInfo method => method.GetParameters(),
			PropertyInfo property => property.GetIndexParameters(),
			_ => [],
		};
		return parameters.Length == 0 ? "()" : "(" + string.Join(", ", parameters.Select(x => GetShortTypeName(x.ParameterType))) + ")";
	}

	private static string GetShortTypeName(Type type)
	{
		if (type.IsByRef)
			return GetShortTypeName(type.GetElementType()!) + "&";
		if (type.IsArray)
			return GetShortTypeName(type.GetElementType()!) + "[]";
		if (!type.IsGenericType)
			return type.Name;
		return type.Name[..type.Name.IndexOf('`', StringComparison.Ordinal)] + "<" + string.Join(", ", type.GetGenericArguments().Select(GetShortTypeName)) + ">";
	}
}

/// <summary>Resolves links to documentation outside the current tree.</summary>
public abstract class XmlDocExternalLinkResolver
{
	/// <summary>Gets a resolver for Microsoft Learn .NET API documentation.</summary>
	public static XmlDocExternalLinkResolver DotNetApi { get; } = new DotNetApiResolver();

	/// <summary>Creates a resolver from a URL format where <c>{ref}</c> is the XML documentation reference.</summary>
	public static XmlDocExternalLinkResolver UrlPattern(string urlFormat) => new PatternResolver(urlFormat);

	/// <summary>Combines resolvers, using the first non-null URL.</summary>
	public static XmlDocExternalLinkResolver Combine(params XmlDocExternalLinkResolver[] resolvers) => new CombinedResolver(resolvers);

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

	private sealed class PatternResolver(string urlFormat) : XmlDocExternalLinkResolver
	{
		public override string TryGetUrl(XmlDocRef reference, MemberInfo? member) => urlFormat.Replace("{ref}", Uri.EscapeDataString(reference.Value), StringComparison.Ordinal).Replace("{name}", Uri.EscapeDataString(reference.Value[2..]), StringComparison.Ordinal);
	}

	private sealed class CombinedResolver(IReadOnlyList<XmlDocExternalLinkResolver> resolvers) : XmlDocExternalLinkResolver
	{
		public override string? TryGetUrl(XmlDocRef reference, MemberInfo? member)
		{
			foreach (var resolver in resolvers)
			{
				if (resolver.TryGetUrl(reference, member) is { } url)
					return url;
			}
			return null;
		}
	}
}

/// <summary>Provides source-link URLs for reflected members.</summary>
public sealed class XmlDocSourceLinks
{
	private XmlDocSourceLinks(Assembly assembly, IReadOnlyDictionary<int, string> urlsByMetadataToken)
	{
		Assembly = assembly;
		m_urlsByMetadataToken = urlsByMetadataToken;
	}

	/// <summary>Gets the assembly this source-link resolver was created for.</summary>
	public Assembly Assembly { get; }

	/// <summary>Attempts to create source links for an assembly.</summary>
	public static XmlDocSourceLinks? TryCreate(Assembly assembly)
	{
		ArgumentNullException.ThrowIfNull(assembly);
		if (string.IsNullOrEmpty(assembly.Location))
			return null;

		var pdbPath = Path.ChangeExtension(assembly.Location, ".pdb");
		if (!File.Exists(pdbPath))
			return TryCreateFromEmbeddedPdb(assembly);

		using var stream = File.OpenRead(pdbPath);
		using var provider = MetadataReaderProvider.FromPortablePdbStream(stream);
		return TryCreateFromReader(assembly, provider.GetMetadataReader());
	}

	/// <summary>Attempts to get a source URL for a member.</summary>
	public string? TryGetUrl(MemberInfo member)
	{
		if (member is TypeInfo type)
			member = type.DeclaredConstructors.FirstOrDefault(x => !x.IsStatic) ?? type.DeclaredMethods.FirstOrDefault() ?? member;
		if (member is PropertyInfo property)
			member = property.GetMethod ?? property.SetMethod ?? member;
		if (member is EventInfo @event)
			member = @event.AddMethod ?? @event.RemoveMethod ?? member;
		return m_urlsByMetadataToken.GetValueOrDefault(member.MetadataToken);
	}

	private static XmlDocSourceLinks? TryCreateFromEmbeddedPdb(Assembly assembly)
	{
		using var stream = File.OpenRead(assembly.Location);
		using var peReader = new PEReader(stream);
		foreach (var entry in peReader.ReadDebugDirectory())
		{
			if (entry.Type != DebugDirectoryEntryType.EmbeddedPortablePdb)
				continue;

			using var provider = peReader.ReadEmbeddedPortablePdbDebugDirectoryData(entry);
			return TryCreateFromReader(assembly, provider.GetMetadataReader());
		}
		return null;
	}

	private static XmlDocSourceLinks? TryCreateFromReader(Assembly assembly, MetadataReader reader)
	{
		var documents = ReadSourceLinkDocuments(reader);
		if (documents.Count == 0)
			return null;

		var urlsByMetadataToken = new Dictionary<int, string>();
		foreach (var handle in reader.MethodDebugInformation)
		{
			var methodDebugInfo = reader.GetMethodDebugInformation(handle);
			if (methodDebugInfo.Document.IsNil)
				continue;

			var firstSequencePoint = methodDebugInfo.GetSequencePoints().FirstOrDefault(x => !x.IsHidden);
			if (firstSequencePoint.Equals(default(SequencePoint)))
				continue;

			var documentName = reader.GetString(reader.GetDocument(methodDebugInfo.Document).Name);
			if (TryGetSourceUrl(documents, documentName, firstSequencePoint.StartLine) is { } sourceUrl)
			{
				var rowNumber = MetadataTokens.GetRowNumber(handle);
				urlsByMetadataToken[MetadataTokens.GetToken(MetadataTokens.MethodDefinitionHandle(rowNumber))] = sourceUrl;
			}
		}

		return urlsByMetadataToken.Count == 0 ? null : new XmlDocSourceLinks(assembly, urlsByMetadataToken);
	}

	private static IReadOnlyDictionary<string, string> ReadSourceLinkDocuments(MetadataReader reader)
	{
		foreach (var handle in reader.CustomDebugInformation)
		{
			var customDebugInformation = reader.GetCustomDebugInformation(handle);
			if (reader.GetGuid(customDebugInformation.Kind) != s_sourceLinkId)
				continue;

			var bytes = reader.GetBlobBytes(customDebugInformation.Value);
			using var document = JsonDocument.Parse(Encoding.UTF8.GetString(bytes));
			if (!document.RootElement.TryGetProperty("documents", out var documentsElement))
				return new Dictionary<string, string>();

			return documentsElement.EnumerateObject().ToDictionary(x => NormalizePath(x.Name), x => x.Value.GetString() ?? "", StringComparer.OrdinalIgnoreCase);
		}
		return new Dictionary<string, string>();
	}

	private static string? TryGetSourceUrl(IReadOnlyDictionary<string, string> documents, string documentName, int line)
	{
		var normalizedDocumentName = NormalizePath(documentName);
		foreach (var (pattern, urlPattern) in documents)
		{
			var starIndex = pattern.IndexOf('*', StringComparison.Ordinal);
			if (starIndex == -1)
			{
				if (string.Equals(pattern, normalizedDocumentName, StringComparison.OrdinalIgnoreCase))
					return urlPattern + "#L" + line;
				continue;
			}

			var prefix = pattern[..starIndex];
			var suffix = pattern[(starIndex + 1)..];
			if (normalizedDocumentName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) && normalizedDocumentName.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
			{
				var wildcard = normalizedDocumentName[prefix.Length..^suffix.Length];
				return urlPattern.Replace("*", wildcard, StringComparison.Ordinal) + "#L" + line;
			}
		}
		return null;
	}

	private static string NormalizePath(string path) => path.Replace('\\', '/');

	private static readonly Guid s_sourceLinkId = new("CC110556-A091-4D38-9FEC-25AB9A351A6A");
	private readonly IReadOnlyDictionary<int, string> m_urlsByMetadataToken;
}
