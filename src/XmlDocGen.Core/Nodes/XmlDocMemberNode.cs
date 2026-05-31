using System.Reflection;
using XmlDocGen.Core.Xml;

namespace XmlDocGen.Core.Nodes;

/// <summary>A member documentation node.</summary>
public sealed class XmlDocMemberNode : XmlDocNode
{
	/// <inheritdoc />
	public override string Name { get; }

	/// <inheritdoc />
	public override XmlDocRef Ref { get; }

	/// <summary>Gets the reflected member.</summary>
	public MemberInfo Member { get; }

	/// <inheritdoc />
	public override XmlDocVisibility Visibility { get; }

	/// <inheritdoc />
	public override MemberInfo MemberInfo => Member;

	/// <summary>Gets the member kind.</summary>
	public XmlDocMemberKind MemberKind { get; }

	/// <summary>Gets a value indicating whether the member is static.</summary>
	public bool IsStatic => ReflectionFacts.IsStatic(Member);

	/// <summary>Gets a value indicating whether the member is abstract.</summary>
	public bool IsAbstract => ReflectionFacts.IsAbstract(Member);

	/// <summary>Gets a value indicating whether the member is virtual.</summary>
	public bool IsVirtual => ReflectionFacts.IsVirtual(Member);

	/// <summary>Gets a value indicating whether the member is sealed.</summary>
	public bool IsSealed => Member is MethodInfo { IsFinal: true };

	/// <summary>Gets a value indicating whether the member is required.</summary>
	public bool IsRequired => Member.GetCustomAttributes().Any(x => x.GetType().FullName == "System.Runtime.CompilerServices.RequiredMemberAttribute");

	internal XmlDocMemberNode(XmlDocTypeNode parent, MemberInfo member)
		: base(parent, ResolveXmlMember(parent.Assembly.Xml, member))
	{
		Member = member;
		Name = ReflectionFacts.GetShortName(member);
		Ref = XmlDocRef.ForMember(member);
		Visibility = GetVisibility(member);
		MemberKind = GetMemberKind(member);
	}

	private static XmlDocMemberKind GetMemberKind(MemberInfo member) => member switch
	{
		ConstructorInfo => XmlDocMemberKind.Constructor,
		MethodInfo method when method.Name.StartsWith("op_", StringComparison.Ordinal) => XmlDocMemberKind.Operator,
		MethodInfo => XmlDocMemberKind.Method,
		PropertyInfo => XmlDocMemberKind.Property,
		FieldInfo => XmlDocMemberKind.Field,
		EventInfo => XmlDocMemberKind.Event,
		_ => XmlDocMemberKind.Method,
	};
}
