using System.Reflection;
using XmlDocGen.Core.Nodes;

namespace XmlDocGen.Core.Pages;

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
			XmlDocNamespaceNode namespaceNode => GetAssemblyPath(namespaceNode.Assembly),
			XmlDocTypeNode typeNode => GetTypePath(typeNode),
			XmlDocMemberNode { Parent: XmlDocTypeNode { Kind: XmlDocTypeKind.Enum } enumType } => GetTypePath(enumType),
			XmlDocMemberNode memberNode => GetTypePath((XmlDocTypeNode) memberNode.Parent!) + "/" + GetMemberSafeName(memberNode),
			_ => GetSafeName(node),
		};
	}

	private static string GetPerTypePath(XmlDocNode node)
	{
		return node switch
		{
			XmlDocAssemblyNode assembly => GetAssemblyPath(assembly),
			XmlDocNamespaceNode namespaceNode => GetAssemblyPath(namespaceNode.Assembly),
			XmlDocMemberNode memberNode => GetTypePath((XmlDocTypeNode) memberNode.Parent!),
			XmlDocTypeNode typeNode => GetTypePath(typeNode),
			_ => GetSafeName(node),
		};
	}

	private static string GetAssemblyPath(XmlDocAssemblyNode assembly) => GetSafeName(assembly.Name);

	private static string GetNamespacePath(XmlDocNode node) => GetSafeName(node.Name);

	private static string GetTypePath(XmlDocTypeNode type) => GetNamespacePath(GetNamespace(type)) + "/" + GetTypeSafeName(type.TypeInfo);

	private static string GetMemberSafeName(XmlDocMemberNode member)
	{
		return GetSafeName(ReflectionFacts.GetShortName(member.Member));
	}

	private static string GetTypeSafeName(TypeInfo type)
	{
		var parts = new Stack<string>();
		for (var current = type; current is not null; current = current.DeclaringType?.GetTypeInfo())
			parts.Push(GetSafeGenericName(current.Name));
		return string.Join('.', parts);
	}

	private static string GetSafeGenericName(string name) => GetSafeName(name.Replace('`', '-'));

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
