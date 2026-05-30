using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using XmlDocGen.Core.Xml;

namespace XmlDocGen.Core.Nodes;

/// <summary>A documentation tree built from one or more assemblies.</summary>
public sealed class XmlDocTree
{
	private XmlDocTree(IEnumerable<XmlDocAssemblyNode> assemblies)
	{
		Assemblies = [.. assemblies.OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase)];
		m_nodesByRef = Assemblies.SelectMany(x => x.DescendantsAndSelf()).GroupBy(x => x.Ref).ToDictionary(x => x.Key, x => x.First());
		m_nodesByMember = Assemblies.SelectMany(x => x.DescendantsAndSelf()).Where(x => x.MemberInfo is not null).GroupBy(x => x.MemberInfo!).ToDictionary(x => x.Key, x => x.First());
	}

	/// <summary>Creates a tree from assembly nodes.</summary>
	public static XmlDocTree Create(IEnumerable<XmlDocAssemblyNode> assemblies) => new(assemblies);

	/// <summary>Creates a tree from reflected assemblies and XML documentation files.</summary>
	public static XmlDocTree Create(IEnumerable<(Assembly Assembly, XmlDocXmlFile Xml)> inputs) => new(inputs.Select(x => XmlDocAssemblyNode.Create(x.Assembly, x.Xml)));

	/// <summary>Gets the assembly roots.</summary>
	public IReadOnlyList<XmlDocAssemblyNode> Assemblies { get; }

	/// <summary>Finds any node in the tree by reference.</summary>
	public XmlDocNode? FindNode(XmlDocRef reference) => m_nodesByRef.GetValueOrDefault(reference);

	/// <summary>Finds the node documenting a reflection type or member.</summary>
	public XmlDocNode? FindNode(MemberInfo member) => m_nodesByMember.GetValueOrDefault(member);

	/// <summary>Enumerates every node in the tree.</summary>
	public IEnumerable<XmlDocNode> DescendantsAndSelf() => Assemblies.SelectMany(x => x.DescendantsAndSelf());

	private readonly IReadOnlyDictionary<XmlDocRef, XmlDocNode> m_nodesByRef;
	private readonly IReadOnlyDictionary<MemberInfo, XmlDocNode> m_nodesByMember;
}

/// <summary>Base class for an assembly, namespace, type, or member documentation node.</summary>
public abstract class XmlDocNode
{
	private protected XmlDocNode(XmlDocNode? parent, XmlDocXmlMember? xml)
	{
		Parent = parent;
		Xml = xml;
		if (parent is null && this is XmlDocAssemblyNode assembly)
			Assembly = assembly;
		else
			Assembly = parent?.Assembly ?? throw new InvalidOperationException("Non-assembly nodes need a parent.");
	}

	/// <summary>Gets the simple display name.</summary>
	public abstract string Name { get; }

	/// <summary>Gets this node's XML documentation reference.</summary>
	public abstract XmlDocRef Ref { get; }

	/// <summary>Gets the parent node, or null for an assembly root.</summary>
	public XmlDocNode? Parent { get; }

	/// <summary>Gets the owning assembly node.</summary>
	public XmlDocAssemblyNode Assembly { get; }

	/// <summary>Gets child nodes.</summary>
	public Collection<XmlDocNode> Children { get; } = [];

	/// <summary>Gets the associated XML documentation, if any.</summary>
	public XmlDocXmlMember? Xml { get; }

	/// <summary>Gets the exact visibility of this node.</summary>
	public abstract XmlDocVisibility Visibility { get; }

	/// <summary>Gets the reflected member associated with this node.</summary>
	public virtual MemberInfo? MemberInfo => null;

	/// <summary>Enumerates this node and every descendant.</summary>
	public IEnumerable<XmlDocNode> DescendantsAndSelf()
	{
		yield return this;
		foreach (var child in Children.SelectMany(x => x.DescendantsAndSelf()))
			yield return child;
	}
}

/// <summary>An assembly documentation node.</summary>
public sealed class XmlDocAssemblyNode : XmlDocNode
{
	private XmlDocAssemblyNode(Assembly assembly, XmlDocXmlFile xml)
		: base(null, null)
	{
		ReflectionAssembly = assembly;
		XmlFile = xml;
		Name = assembly.GetName().Name ?? assembly.FullName ?? "Assembly";
		Ref = new XmlDocRef("A:" + Name);

		var namespaces = assembly.DefinedTypes.Where(IsDocumentableType).GroupBy(x => x.Namespace ?? "global").OrderBy(x => x.Key, StringComparer.OrdinalIgnoreCase);
		foreach (var group in namespaces)
			Children.Add(new XmlDocNamespaceNode(this, group.Key, [.. group.OrderBy(x => x.FullName, StringComparer.OrdinalIgnoreCase)]));
	}

	/// <summary>Creates an assembly node.</summary>
	public static XmlDocAssemblyNode Create(Assembly assembly, XmlDocXmlFile xml) => new(assembly, xml);

	/// <inheritdoc />
	public override string Name { get; }

	/// <inheritdoc />
	public override XmlDocRef Ref { get; }

	/// <summary>Gets the reflected assembly.</summary>
	public Assembly ReflectionAssembly { get; }

	/// <summary>Gets the XML documentation file associated with the assembly.</summary>
	public XmlDocXmlFile XmlFile { get; }

	/// <inheritdoc />
	public override XmlDocVisibility Visibility => XmlDocVisibility.Public;

	private static bool IsDocumentableType(TypeInfo type) => type.Name.Length != 0 && type.Name[0] != '<' && !type.GetCustomAttributes<CompilerGeneratedAttribute>().Any() && type.DeclaringType is null;
}

/// <summary>A namespace documentation node.</summary>
public sealed class XmlDocNamespaceNode : XmlDocNode
{
	internal XmlDocNamespaceNode(XmlDocAssemblyNode assembly, string name, IReadOnlyList<TypeInfo> types)
		: base(assembly, null)
	{
		Name = name;
		Ref = XmlDocRef.ForNamespace(name);
		foreach (var type in types)
			Children.Add(new XmlDocTypeNode(this, type));
	}

	/// <inheritdoc />
	public override string Name { get; }

	/// <inheritdoc />
	public override XmlDocRef Ref { get; }

	/// <inheritdoc />
	public override XmlDocVisibility Visibility => XmlDocVisibility.Public;
}

/// <summary>A type documentation node.</summary>
public sealed class XmlDocTypeNode : XmlDocNode
{
	internal XmlDocTypeNode(XmlDocNode parent, TypeInfo type)
		: base(parent, parent.Assembly.XmlFile.FindMember(XmlDocRef.ForType(type)))
	{
		Type = type;
		Name = ReflectionFacts.GetShortName(type);
		Ref = XmlDocRef.ForType(type);
		Kind = ReflectionFacts.GetTypeKind(type);
		Visibility = ReflectionFacts.GetVisibility(type);

		foreach (var nestedType in type.DeclaredNestedTypes.Where(x => x.Name.Length != 0 && x.Name[0] != '<').OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase))
			Children.Add(new XmlDocTypeNode(this, nestedType));

		foreach (var member in ReflectionFacts.GetDocumentableMembers(type))
			Children.Add(new XmlDocMemberNode(this, member));
	}

	/// <inheritdoc />
	public override string Name { get; }

	/// <inheritdoc />
	public override XmlDocRef Ref { get; }

	/// <summary>Gets the reflected type.</summary>
	public TypeInfo Type { get; }

	/// <summary>Gets the type kind.</summary>
	public XmlDocTypeKind Kind { get; }

	/// <summary>Gets a value indicating whether the type is readonly.</summary>
	public bool IsReadOnly => Type.GetCustomAttributes().Any(x => x.GetType().FullName == "System.Runtime.CompilerServices.IsReadOnlyAttribute");

	/// <summary>Gets a value indicating whether the type is a ref struct.</summary>
	public bool IsRefStruct => Type.GetCustomAttributes().Any(x => x.GetType().FullName == "System.Runtime.CompilerServices.IsByRefLikeAttribute");

	/// <inheritdoc />
	public override XmlDocVisibility Visibility { get; }

	/// <inheritdoc />
	public override MemberInfo MemberInfo => Type;
}

/// <summary>A member documentation node.</summary>
public sealed class XmlDocMemberNode : XmlDocNode
{
	internal XmlDocMemberNode(XmlDocTypeNode parent, MemberInfo member)
		: base(parent, parent.Assembly.XmlFile.FindMember(XmlDocRef.ForMember(member)))
	{
		Member = member;
		Name = ReflectionFacts.GetShortName(member);
		Ref = XmlDocRef.ForMember(member);
		Visibility = ReflectionFacts.GetVisibility(member);
	}

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
}

/// <summary>The exact visibility of a documentation node.</summary>
public enum XmlDocVisibility
{
	/// <summary>Private.</summary>
	Private,
	/// <summary>Internal.</summary>
	Internal,
	/// <summary>Protected internal.</summary>
	ProtectedInternal,
	/// <summary>Protected.</summary>
	Protected,
	/// <summary>Public.</summary>
	Public,
}

/// <summary>Kinds of documented types.</summary>
public enum XmlDocTypeKind
{
	/// <summary>A class.</summary>
	Class,
	/// <summary>An interface.</summary>
	Interface,
	/// <summary>A struct.</summary>
	Struct,
	/// <summary>An enum.</summary>
	Enum,
	/// <summary>A delegate.</summary>
	Delegate,
	/// <summary>A record class.</summary>
	Record,
	/// <summary>A record struct.</summary>
	RecordStruct,
}

/// <summary>A composable node-visibility filter.</summary>
public sealed class XmlDocNodeVisibility
{
	private XmlDocNodeVisibility(Func<XmlDocNode, bool> predicate)
	{
		m_predicate = predicate;
	}

	/// <summary>Gets a filter that includes public nodes.</summary>
	public static XmlDocNodeVisibility Public { get; } = Create(XmlDocVisibility.Public);

	/// <summary>Gets a filter that includes public and protected nodes.</summary>
	public static XmlDocNodeVisibility Protected { get; } = Create(XmlDocVisibility.Protected);

	/// <summary>Gets a filter that includes public, protected, and internal nodes.</summary>
	public static XmlDocNodeVisibility Internal { get; } = Create(XmlDocVisibility.Internal);

	/// <summary>Gets a filter that includes all nodes.</summary>
	public static XmlDocNodeVisibility Private { get; } = Create(XmlDocVisibility.Private);

	/// <summary>Creates a minimum-visibility filter.</summary>
	public static XmlDocNodeVisibility Create(XmlDocVisibility minimum) => new(node => node is XmlDocAssemblyNode or XmlDocNamespaceNode || (int) node.Visibility >= (int) minimum);

	/// <summary>Creates a custom predicate filter.</summary>
	public static XmlDocNodeVisibility Create(Func<XmlDocNode, bool> predicate) => new(predicate);

	/// <summary>Combines this filter with another filter.</summary>
	public XmlDocNodeVisibility And(XmlDocNodeVisibility other) => new(node => Includes(node) && other.Includes(node));

	/// <summary>Excludes obsolete nodes.</summary>
	public XmlDocNodeVisibility ExcludeObsolete() => new(node => Includes(node) && node.MemberInfo?.GetCustomAttributes<ObsoleteAttribute>().Any() != true);

	/// <summary>Excludes nodes marked with <see cref="EditorBrowsableState.Never"/>.</summary>
	public XmlDocNodeVisibility ExcludeUnbrowsable() => new(node => Includes(node) && node.MemberInfo?.GetCustomAttributes<EditorBrowsableAttribute>().Any(x => x.State == EditorBrowsableState.Never) != true);

	/// <summary>Excludes compiler-generated nodes.</summary>
	public XmlDocNodeVisibility ExcludeCompilerGenerated() => new(node => Includes(node) && node.MemberInfo?.GetCustomAttributes<CompilerGeneratedAttribute>().Any() != true);

	/// <summary>Returns true if the node is included.</summary>
	public bool Includes(XmlDocNode node) => m_predicate(node);

	private readonly Func<XmlDocNode, bool> m_predicate;
}

/// <summary>Shared reflection helpers for node construction and rendering.</summary>
public static class ReflectionFacts
{
	/// <summary>Gets documentable members declared by the type.</summary>
	public static IEnumerable<MemberInfo> GetDocumentableMembers(TypeInfo type) => type.DeclaredMembers.Where(x => x is not TypeInfo && IsDocumentableMember(x)).OrderBy(GetMemberOrder).ThenBy(GetShortName, StringComparer.OrdinalIgnoreCase);

	/// <summary>Gets a C#-style short name.</summary>
	public static string GetShortName(MemberInfo member)
	{
		var name = member.Name;
		var tickIndex = name.IndexOf('`', StringComparison.Ordinal);
		if (tickIndex != -1)
			name = name[..tickIndex];
		if (name is ".ctor" or ".cctor")
			name = GetShortName(member.DeclaringType!.GetTypeInfo());
		return name;
	}

	/// <summary>Gets the type kind.</summary>
	public static XmlDocTypeKind GetTypeKind(TypeInfo type)
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

	/// <summary>Gets the exact visibility of a reflected member.</summary>
	public static XmlDocVisibility GetVisibility(MemberInfo member) => GetVisibility(member, XmlDocVisibility.ProtectedInternal);

	/// <summary>Returns true when the member is static.</summary>
	public static bool IsStatic(MemberInfo member) => member switch
	{
		TypeInfo type => type is { IsClass: true, IsAbstract: true, IsSealed: true },
		EventInfo @event => @event.AddMethod?.IsStatic is true,
		PropertyInfo property => (property.GetMethod ?? property.SetMethod)?.IsStatic ?? false,
		FieldInfo field => field is { IsStatic: true, IsLiteral: false },
		MethodBase method => method.IsStatic,
		_ => false,
	};

	/// <summary>Returns true when the member is abstract.</summary>
	public static bool IsAbstract(MemberInfo member)
	{
		if (member is TypeInfo { IsInterface: false } type)
			return type.IsAbstract;
		if (member.DeclaringType?.GetTypeInfo().IsInterface == true)
			return false;
		return member switch
		{
			EventInfo @event => @event.AddMethod is not null && IsAbstract(@event.AddMethod),
			PropertyInfo property => (property.GetMethod is not null && IsAbstract(property.GetMethod)) || (property.SetMethod is not null && IsAbstract(property.SetMethod)),
			MethodBase method => method.IsAbstract,
			_ => false,
		};
	}

	/// <summary>Returns true when the member is virtual.</summary>
	public static bool IsVirtual(MemberInfo member)
	{
		if (member.DeclaringType?.GetTypeInfo().IsInterface == true)
			return false;
		return member switch
		{
			EventInfo @event => @event.AddMethod is not null && IsVirtual(@event.AddMethod),
			PropertyInfo property => (property.GetMethod is not null && IsVirtual(property.GetMethod)) || (property.SetMethod is not null && IsVirtual(property.SetMethod)),
			MethodInfo method => method is { IsVirtual: true, IsFinal: false } && method.GetRuntimeBaseDefinition()!.DeclaringType == method.DeclaringType,
			_ => false,
		};
	}

	private static bool IsDocumentableMember(MemberInfo member)
	{
		if (IsBuiltInRecordMember(member))
			return false;
		if (member.Name.Length == 0 || member.Name[0] == '<')
			return false;
		if (member is MethodBase { IsSpecialName: true } method && member is not ConstructorInfo && !method.Name.StartsWith("op_", StringComparison.Ordinal))
			return false;
		return true;
	}

	private static XmlDocVisibility GetVisibility(MemberInfo member, XmlDocVisibility protectedInternal)
	{
		if (member is TypeInfo type)
		{
			var visibility = GetTypeVisibility(type, protectedInternal);
			return type.IsNested ? MostPrivate(visibility, GetTypeVisibility(type.DeclaringType!.GetTypeInfo(), protectedInternal)) : visibility;
		}
		return member switch
		{
			EventInfo @event => GetMethodVisibility(@event.AddMethod!, protectedInternal),
			PropertyInfo property => GetPropertyVisibility(property, protectedInternal),
			FieldInfo field => GetFieldVisibility(field, protectedInternal),
			MethodBase method => GetMethodVisibility(method, protectedInternal),
			_ => XmlDocVisibility.Private,
		};
	}

	private static XmlDocVisibility GetTypeVisibility(TypeInfo type, XmlDocVisibility protectedInternal)
	{
		if (type.IsPublic || type.IsNestedPublic)
			return XmlDocVisibility.Public;
		if (type.IsNestedFamORAssem)
			return protectedInternal;
		if (type.IsNestedFamily)
			return XmlDocVisibility.Protected;
		if (type.IsNestedAssembly || type.IsNestedFamANDAssem)
			return XmlDocVisibility.Internal;
		return XmlDocVisibility.Private;
	}

	private static XmlDocVisibility GetMethodVisibility(MethodBase method, XmlDocVisibility protectedInternal)
	{
		if (method.IsPublic)
			return XmlDocVisibility.Public;
		if (method.IsFamilyOrAssembly)
			return protectedInternal;
		if (method.IsFamily)
			return XmlDocVisibility.Protected;
		if (method.IsAssembly || method.IsFamilyAndAssembly)
			return XmlDocVisibility.Internal;
		return XmlDocVisibility.Private;
	}

	private static XmlDocVisibility GetPropertyVisibility(PropertyInfo property, XmlDocVisibility protectedInternal)
	{
		var getMethod = property.GetMethod;
		var setMethod = property.SetMethod;
		if (getMethod is not null && setMethod is not null)
			return MostPublic(GetMethodVisibility(getMethod, protectedInternal), GetMethodVisibility(setMethod, protectedInternal));
		return GetMethodVisibility((getMethod ?? setMethod)!, protectedInternal);
	}

	private static XmlDocVisibility GetFieldVisibility(FieldInfo field, XmlDocVisibility protectedInternal)
	{
		if (field.IsPublic)
			return XmlDocVisibility.Public;
		if (field.IsFamilyOrAssembly)
			return protectedInternal;
		if (field.IsFamily)
			return XmlDocVisibility.Protected;
		if (field.IsAssembly || field.IsFamilyAndAssembly)
			return XmlDocVisibility.Internal;
		return XmlDocVisibility.Private;
	}

	private static XmlDocVisibility MostPublic(XmlDocVisibility left, XmlDocVisibility right) => (XmlDocVisibility) Math.Max((int) left, (int) right);

	private static XmlDocVisibility MostPrivate(XmlDocVisibility left, XmlDocVisibility right) => (XmlDocVisibility) Math.Min((int) left, (int) right);

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