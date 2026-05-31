using System.Reflection;
using XmlDocGen.Core.Xml;

namespace XmlDocGen.Core.Nodes;

/// <summary>A namespace documentation node.</summary>
public sealed class XmlDocNamespaceNode : XmlDocNode
{
	internal XmlDocNamespaceNode(XmlDocAssemblyNode assembly, string name, IReadOnlyList<TypeInfo> types)
		: base(assembly, null)
	{
		Name = name;
		Ref = XmlDocRef.ForNamespace(name);
		foreach (var type in types)
			AddChild(new XmlDocTypeNode(this, type));
		Types = [.. Children.OfType<XmlDocTypeNode>()];
	}

	/// <inheritdoc />
	public override string Name { get; }

	/// <inheritdoc />
	public override XmlDocRef Ref { get; }

	/// <inheritdoc />
	public override XmlDocVisibility Visibility => XmlDocVisibility.Public;

	/// <summary>Gets top-level types in this namespace.</summary>
	public IReadOnlyList<XmlDocTypeNode> Types { get; }
}
