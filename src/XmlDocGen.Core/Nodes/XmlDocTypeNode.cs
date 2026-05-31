using System.Reflection;
using XmlDocGen.Core.Xml;

namespace XmlDocGen.Core.Nodes;

/// <summary>A type documentation node.</summary>
public sealed class XmlDocTypeNode : XmlDocNode
{
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

	internal XmlDocTypeNode(XmlDocNode parent, TypeInfo type)
		: base(parent, ResolveXmlMember(parent.Assembly.Xml, type))
	{
		Type = type;
		Name = ReflectionFacts.GetShortName(type);
		Ref = XmlDocRef.ForType(type);
		Kind = GetTypeKind(type);
		Visibility = GetVisibility(type);

		foreach (var nestedType in type.DeclaredNestedTypes.Where(x => x.Name.Length != 0 && x.Name[0] != '<').OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase))
			AddChild(new XmlDocTypeNode(this, nestedType));

		foreach (var member in GetDocumentableMembers(type))
			AddChild(new XmlDocMemberNode(this, member));
		NestedTypes = [.. Children.OfType<XmlDocTypeNode>()];
		Members = [.. Children.OfType<XmlDocMemberNode>()];
	}

	private static IEnumerable<MemberInfo> GetDocumentableMembers(TypeInfo type) => type.DeclaredMembers.Where(x => x is not System.Reflection.TypeInfo && IsDocumentableMember(x)).OrderBy(GetMemberOrder).ThenBy(ReflectionFacts.GetShortName, StringComparer.OrdinalIgnoreCase);

	private static XmlDocTypeKind GetTypeKind(TypeInfo type)
	{
		if (typeof(Delegate).GetTypeInfo().IsAssignableFrom(type))
			return XmlDocTypeKind.Delegate;
		if (IsRecord(type) && type.IsValueType)
			return XmlDocTypeKind.RecordStruct;
		if (IsRecord(type))
			return XmlDocTypeKind.Record;
		if (type.IsInterface)
			return XmlDocTypeKind.Interface;
		if (type.IsEnum)
			return XmlDocTypeKind.Enum;
		if (type.IsValueType)
			return XmlDocTypeKind.Struct;
		return XmlDocTypeKind.Class;
	}

	private static bool IsDocumentableMember(MemberInfo member)
	{
		if (member.DeclaringType?.IsEnum == true && member.Name == "value__")
			return false;
		if (IsBuiltInRecordMember(member))
			return false;
		if (member.Name.Length == 0 || member.Name[0] == '<')
			return false;
		if (member is MethodBase { IsSpecialName: true } method && member is not ConstructorInfo && !method.Name.StartsWith("op_", StringComparison.Ordinal))
			return false;
		return true;
	}

	private static bool IsRecord(Type type) => type.GetMethod("<Clone>$") is not null || type.GetCustomAttributes().Any(x => x.GetType().FullName == "System.Runtime.CompilerServices.CompilerFeatureRequiredAttribute" && (string?) x.GetType().GetProperty("FeatureName")?.GetValue(x) == "Records");

	private static bool IsBuiltInRecordMember(MemberInfo member)
	{
		if (member.DeclaringType is not { } declaringType || !IsRecord(declaringType))
			return false;
		return member switch
		{
			MethodInfo method => method.Name is "<Clone>$" or "Deconstruct" or "Equals" or "GetHashCode" or "op_Equality" or "op_Inequality" or "PrintMembers" or "ToString",
			PropertyInfo property => property.Name is "EqualityContract",
			ConstructorInfo constructor => !constructor.IsPublic && constructor.GetParameters().Select(x => x.ParameterType).SequenceEqual([declaringType]),
			_ => false,
		};
	}

	private static int GetMemberOrder(MemberInfo member) => member switch
	{
		ConstructorInfo => 0,
		PropertyInfo => 1,
		FieldInfo => 2,
		EventInfo => 3,
		MethodInfo method when method.Name.StartsWith("op_", StringComparison.Ordinal) => 5,
		MethodInfo => 4,
		_ => 6,
	};
}
