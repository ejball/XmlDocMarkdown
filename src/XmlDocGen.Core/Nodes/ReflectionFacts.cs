using System.Reflection;
namespace XmlDocGen.Core.Nodes;

/// <summary>Shared reflection helpers for node construction and rendering.</summary>
public static class ReflectionFacts
{
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
}
