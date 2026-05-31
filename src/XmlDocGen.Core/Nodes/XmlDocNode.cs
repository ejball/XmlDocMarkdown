using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using XmlDocGen.Core.Xml;

namespace XmlDocGen.Core.Nodes;

/// <summary>Base class for an assembly, namespace, type, or member documentation node.</summary>
public abstract class XmlDocNode
{
	/// <summary>Gets the simple display name.</summary>
	public abstract string Name { get; }

	/// <summary>Gets this node's XML documentation reference.</summary>
	public abstract XmlDocRef Ref { get; }

	/// <summary>Gets the parent node, or null for an assembly root.</summary>
	public XmlDocNode? Parent { get; }

	/// <summary>Gets the owning assembly node.</summary>
	public XmlDocAssemblyNode Assembly { get; }

	/// <summary>Gets child nodes.</summary>
	public IReadOnlyList<XmlDocNode> Children => m_children;

	/// <summary>Gets the associated XML documentation, if any.</summary>
	public XmlDocXmlMember? XmlMember { get; }

	/// <summary>Gets a value indicating whether this node is obsolete.</summary>
	public bool IsObsolete => MemberInfo?.GetCustomAttributes<ObsoleteAttribute>().Any() == true;

	/// <summary>Gets a value indicating whether this node is browsable.</summary>
	public bool IsBrowsable => MemberInfo?.GetCustomAttributes<EditorBrowsableAttribute>().Any(x => x.State == EditorBrowsableState.Never) != true;

	/// <summary>Gets a value indicating whether this node is compiler-generated.</summary>
	public bool IsCompilerGenerated => MemberInfo?.GetCustomAttributes<CompilerGeneratedAttribute>().Any() == true;

	/// <summary>Gets the exact visibility of this node.</summary>
	public abstract XmlDocVisibility Visibility { get; }

	/// <summary>Gets the reflected member associated with this node.</summary>
	public virtual MemberInfo? MemberInfo => null;

	internal IEnumerable<XmlDocNode> EnumerateNodes()
	{
		yield return this;
		foreach (var child in Children.SelectMany(x => x.EnumerateNodes()))
			yield return child;
	}

	internal IEnumerable<XmlDocNode> EnumerateNodes(XmlDocNodeVisibility visibility)
	{
		if (visibility.IsVisible(this))
			yield return this;
		foreach (var child in Children.SelectMany(x => x.EnumerateNodes(visibility)))
			yield return child;
	}

	private protected XmlDocNode(XmlDocNode? parent, XmlDocXmlMember? xml)
	{
		Parent = parent;
		XmlMember = xml;
		if (parent is null && this is XmlDocAssemblyNode assembly)
			Assembly = assembly;
		else
			Assembly = parent?.Assembly ?? throw new InvalidOperationException("Non-assembly nodes need a parent.");
	}

	private protected void AddChild(XmlDocNode child) => m_children.Add(child);

	private protected static XmlDocXmlMember? ResolveXmlMember(XmlDocXmlFile xml, MemberInfo member)
	{
		var own = xml.FindMember(member is TypeInfo type ? XmlDocRef.ForType(type) : XmlDocRef.ForMember(member));
		if (own?.InheritDoc is not { } inheritDoc)
			return own;

		if (inheritDoc.Cref is { } inheritedReference)
			return xml.FindMember(inheritedReference)?.ApplyInheritDocPath(inheritDoc.Path) ?? own;

		var inheritedMember = FindInheritedMember(member);
		if (inheritedMember is null)
			return own;

		return xml.FindMember(inheritedMember is TypeInfo inheritedType ? XmlDocRef.ForType(inheritedType) : XmlDocRef.ForMember(inheritedMember))?.ApplyInheritDocPath(inheritDoc.Path) ?? own;
	}

	private protected static XmlDocVisibility GetVisibility(MemberInfo member) => GetVisibility(member, XmlDocVisibility.ProtectedInternal);

	private static MemberInfo? FindInheritedMember(MemberInfo member)
	{
		if (member is TypeInfo type)
			return type.BaseType?.GetTypeInfo();

		if (member is MethodInfo method)
		{
			var baseDefinition = method.GetBaseDefinition();
			if (baseDefinition != method)
				return baseDefinition;
			return FindInterfaceMethod(method);
		}

		if (member is PropertyInfo property)
		{
			var accessor = property.GetMethod ?? property.SetMethod;
			return accessor is null ? null : FindInheritedAccessorOwner(accessor, x => x.GetProperties(), (propertyInfo, inheritedAccessor) => propertyInfo.GetMethod == inheritedAccessor || propertyInfo.SetMethod == inheritedAccessor);
		}

		if (member is EventInfo @event)
		{
			var accessor = @event.AddMethod ?? @event.RemoveMethod;
			return accessor is null ? null : FindInheritedAccessorOwner(accessor, x => x.GetEvents(), (eventInfo, inheritedAccessor) => eventInfo.AddMethod == inheritedAccessor || eventInfo.RemoveMethod == inheritedAccessor);
		}

		return null;
	}

	private static MethodInfo? FindInterfaceMethod(MethodInfo method)
	{
		var declaringType = method.DeclaringType;
		if (declaringType is null)
			return null;

		foreach (var interfaceType in declaringType.GetInterfaces())
		{
			var map = declaringType.GetInterfaceMap(interfaceType);
			for (var index = 0; index < map.TargetMethods.Length; index++)
			{
				if (map.TargetMethods[index] == method)
					return map.InterfaceMethods[index];
			}
		}
		return null;
	}

	private static MemberInfo? FindInheritedAccessorOwner<T>(MethodInfo accessor, Func<Type, IEnumerable<T>> getMembers, Func<T, MethodInfo, bool> isOwner)
		where T : MemberInfo
	{
		if (FindInheritedMember(accessor) is not MethodInfo inheritedAccessor)
			return null;
		return getMembers(inheritedAccessor.DeclaringType!).FirstOrDefault(member => isOwner(member, inheritedAccessor));
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

	private readonly Collection<XmlDocNode> m_children = [];
}
