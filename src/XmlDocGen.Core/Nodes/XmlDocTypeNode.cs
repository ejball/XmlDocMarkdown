using System.Reflection;
using XmlDocGen.Core.Xml;

namespace XmlDocGen.Core.Nodes;

/// <summary>A type documentation node.</summary>
public sealed class XmlDocTypeNode : XmlDocNode
{
	internal XmlDocTypeNode(XmlDocNode parent, TypeInfo type)
		: base(parent, ReflectionFacts.ResolveXmlMember(parent.Assembly.Xml, type))
	{
		Type = type;
		Name = ReflectionFacts.GetShortName(type);
		Ref = XmlDocRef.ForType(type);
		Kind = ReflectionFacts.GetTypeKind(type);
		Visibility = ReflectionFacts.GetVisibility(type);

		foreach (var nestedType in type.DeclaredNestedTypes.Where(x => x.Name.Length != 0 && x.Name[0] != '<').OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase))
			AddChild(new XmlDocTypeNode(this, nestedType));

		foreach (var member in ReflectionFacts.GetDocumentableMembers(type))
			AddChild(new XmlDocMemberNode(this, member));
		NestedTypes = [.. Children.OfType<XmlDocTypeNode>()];
		Members = [.. Children.OfType<XmlDocMemberNode>()];
	}

	/// <inheritdoc />
	public override string Name { get; }

	/// <inheritdoc />
	public override XmlDocRef Ref { get; }

	/// <summary>Gets the reflected type.</summary>
	public Type Type { get; }

	/// <summary>Gets the reflected type info.</summary>
	public TypeInfo TypeInfo => (TypeInfo) Type;

	/// <summary>Gets the type kind.</summary>
	public XmlDocTypeKind Kind { get; }

	/// <summary>Gets a value indicating whether the type is readonly.</summary>
	public bool IsReadOnly => TypeInfo.GetCustomAttributes().Any(x => x.GetType().FullName == "System.Runtime.CompilerServices.IsReadOnlyAttribute");

	/// <summary>Gets a value indicating whether the type is a ref struct.</summary>
	public bool IsRefStruct => TypeInfo.GetCustomAttributes().Any(x => x.GetType().FullName == "System.Runtime.CompilerServices.IsByRefLikeAttribute");

	/// <summary>Gets nested types.</summary>
	public IReadOnlyList<XmlDocTypeNode> NestedTypes { get; }

	/// <summary>Gets members.</summary>
	public IReadOnlyList<XmlDocMemberNode> Members { get; }

	/// <inheritdoc />
	public override XmlDocVisibility Visibility { get; }

	/// <inheritdoc />
	public override MemberInfo MemberInfo => TypeInfo;
}
