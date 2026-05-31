using System.Reflection;
using XmlDocGen.Core.Nodes;

namespace XmlDocGen.Core.Pages;

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
