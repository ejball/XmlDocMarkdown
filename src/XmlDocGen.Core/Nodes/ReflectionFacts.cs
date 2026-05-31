using System.Reflection;
using XmlDocGen.Core.Xml;

namespace XmlDocGen.Core.Nodes;

/// <summary>Shared reflection helpers for node construction and rendering.</summary>
public static class ReflectionFacts
{
	/// <summary>Gets documentable members declared by the type.</summary>
	public static IEnumerable<MemberInfo> GetDocumentableMembers(TypeInfo type) => type.DeclaredMembers.Where(x => x is not TypeInfo && IsDocumentableMember(x)).OrderBy(GetMemberOrder).ThenBy(GetShortName, StringComparer.OrdinalIgnoreCase);

	/// <summary>Gets the member kind.</summary>
	public static XmlDocMemberKind GetMemberKind(MemberInfo member) => member switch
	{
		ConstructorInfo => XmlDocMemberKind.Constructor,
		MethodInfo method when method.Name.StartsWith("op_", StringComparison.Ordinal) => XmlDocMemberKind.Operator,
		MethodInfo => XmlDocMemberKind.Method,
		PropertyInfo => XmlDocMemberKind.Property,
		FieldInfo => XmlDocMemberKind.Field,
		EventInfo => XmlDocMemberKind.Event,
		_ => XmlDocMemberKind.Method,
	};

	/// <summary>Gets XML documentation for a member, resolving simple inheritdoc directives.</summary>
	public static XmlDocXmlMember? ResolveXmlMember(XmlDocXmlFile xml, MemberInfo member)
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
