using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace XmlDocGen.Core.Xml;

internal static partial class XmlDocRefUtility
{
	public static string? GetXmlDocRef(MemberInfo? memberInfo)
	{
		switch (memberInfo)
		{
			case null:
				return null;
			case TypeInfo typeInfo:
				return "T:" + GetXmlDocTypePart(typeInfo);
			case MethodBase methodBase:
				var methodInfo = methodBase as MethodInfo;
				return "M:" + GetXmlDocMemberPart(methodBase) + (methodBase.IsGenericMethodDefinition ? $"``{methodBase.GetGenericArguments().Length}" : "") + GetXmlDocParameters(methodBase.GetParameters()) + (methodInfo?.Name is "op_Implicit" or "op_Explicit" ? $"~{GetXmlDocTypePart(methodInfo.ReturnType.GetTypeInfo())}" : "");
			case PropertyInfo propertyInfo:
				return "P:" + GetXmlDocMemberPart(propertyInfo) + (propertyInfo.GetIndexParameters().Length == 0 ? "" : GetXmlDocParameters(propertyInfo.GetIndexParameters()));
			case EventInfo eventInfo:
				return "E:" + GetXmlDocMemberPart(eventInfo);
			case FieldInfo fieldInfo:
				return "F:" + GetXmlDocMemberPart(fieldInfo);
			default:
				throw new InvalidOperationException("Unexpected member: " + memberInfo);
		}
	}

	public static string GetShortNameForXmlDocRef(XmlDocRef reference)
	{
		var match = XmlDocNameRegex().Match(reference.Value);
		return match.Success ? match.Groups["name"].Value : reference.Value;
	}

	private static string GetXmlDocTypePart(TypeInfo typeInfo)
	{
		var builder = new StringBuilder();
		if (typeInfo.IsArray)
		{
			builder.Append(GetXmlDocTypePart(typeInfo.GetElementType()!.GetTypeInfo()));
			builder.Append("[]");
		}
		else if (typeInfo.IsByRef)
		{
			builder.Append(GetXmlDocTypePart(typeInfo.GetElementType()!.GetTypeInfo()));
			builder.Append('@');
		}
		else if (!typeInfo.IsGenericParameter)
		{
			if (typeInfo.DeclaringType is not null)
				builder.Append(GetXmlDocTypePart(typeInfo.DeclaringType.GetTypeInfo()) + ".");
			else if (!string.IsNullOrEmpty(typeInfo.Namespace))
				builder.Append(typeInfo.Namespace + ".");

			var tickIndex = typeInfo.Name.IndexOf('`', StringComparison.Ordinal);
			if (typeInfo is { IsGenericType: true, IsGenericTypeDefinition: false } && tickIndex != -1)
			{
				builder.Append(typeInfo.Name.AsSpan(0, tickIndex));
				builder.Append('{');
				builder.Append(string.Join(",", typeInfo.GenericTypeArguments.Select(x => GetXmlDocTypePart(x.GetTypeInfo()))));
				builder.Append('}');
			}
			else
			{
				builder.Append(typeInfo.Name);
			}
		}
		else if (typeInfo.DeclaringMethod is { } declaringMethod)
		{
			builder.Append("``" + declaringMethod.GetGenericArguments().ToList().IndexOf(typeInfo.AsType()));
		}
		else
		{
			builder.Append("`" + typeInfo.DeclaringType!.GetTypeInfo().GenericTypeParameters.ToList().IndexOf(typeInfo.AsType()));
		}

		return builder.ToString();
	}

	private static string GetXmlDocMemberPart(MemberInfo memberInfo) => GetXmlDocTypePart(memberInfo.DeclaringType!.GetTypeInfo()) + "." + memberInfo.Name.Replace('.', '#');

	private static string GetXmlDocParameters(ParameterInfo[] parameters) => parameters.Length == 0 ? "" : "(" + string.Join(",", parameters.Select(x => GetXmlDocTypePart(x.ParameterType.GetTypeInfo()))) + ")";

	[GeneratedRegex(@"^[A-Z]:([^\.]+\.)*(?'name'[^\.\(\{`]+)", RegexOptions.ExplicitCapture | RegexOptions.CultureInvariant)]
	private static partial Regex XmlDocNameRegex();
}
