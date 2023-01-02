using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace XmlDocMarkdown.Core
{
	internal static class XmlDocUtility
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
					return "M:" +
						GetXmlDocMemberPart(methodBase) +
						(methodBase.IsGenericMethodDefinition ? $"``{methodBase.GetGenericArguments().Length}" : "") +
						GetXmlDocParameters(methodBase.GetParameters()) +
						(methodInfo?.Name is "op_Implicit" or "op_Explicit" ? $"~{GetXmlDocTypePart(methodInfo.ReturnType.GetTypeInfo())}" : "");

				case PropertyInfo propertyInfo:
					return "P:" +
						GetXmlDocMemberPart(propertyInfo) +
						(propertyInfo.GetIndexParameters().Length == 0 ? "" : GetXmlDocParameters(propertyInfo.GetIndexParameters()));

				case EventInfo eventInfo:
					return "E:" + GetXmlDocMemberPart(eventInfo);

				case FieldInfo fieldInfo:
					return "F:" + GetXmlDocMemberPart(fieldInfo);

				default:
					throw new InvalidOperationException("Unexpected member: " + memberInfo);
			}
		}

		public static string GetShortNameForXmlDocRef(string xmlDocRef)
		{
			var match = Regex.Match(xmlDocRef, @"^[A-Z]:([^\.]+\.)*(?'name'[^\.\(\{`]+)", RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture);
			return match.Success ? match.Groups["name"].Value : xmlDocRef;
		}

		private static string GetXmlDocTypePart(TypeInfo typeInfo)
		{
			var stringBuilder = new StringBuilder();

			if (typeInfo.IsArray)
			{
				stringBuilder.Append(GetXmlDocTypePart(typeInfo.GetElementType().GetTypeInfo()));
				stringBuilder.Append("[]");
			}
			else if (typeInfo.IsByRef)
			{
				stringBuilder.Append(GetXmlDocTypePart(typeInfo.GetElementType().GetTypeInfo()));
				stringBuilder.Append('@');
			}
			else if (!typeInfo.IsGenericParameter)
			{
				if (typeInfo.DeclaringType is not null)
					stringBuilder.Append(GetXmlDocTypePart(typeInfo.DeclaringType.GetTypeInfo()) + ".");
				else if (!string.IsNullOrEmpty(typeInfo.Namespace))
					stringBuilder.Append(typeInfo.Namespace + ".");

				var tickIndex = typeInfo.Name.IndexOf('`');
				if (typeInfo is { IsGenericType: true, IsGenericTypeDefinition: false } && tickIndex != -1)
				{
					stringBuilder.Append(typeInfo.Name.Substring(0, tickIndex));
					stringBuilder.Append('{');
					stringBuilder.Append(string.Join(",", typeInfo.GenericTypeArguments.Select(x => GetXmlDocTypePart(x.GetTypeInfo()))));
					stringBuilder.Append('}');
				}
				else
				{
					stringBuilder.Append(typeInfo.Name);
				}
			}
			else if (typeInfo.DeclaringMethod is { } declaringMethod)
			{
				var genericTypeIndex = declaringMethod.GetGenericArguments().ToList().IndexOf(typeInfo.AsType());
				if (genericTypeIndex == -1)
					throw new InvalidOperationException("Unexpected type: " + typeInfo);

				stringBuilder.Append("``" + genericTypeIndex);
			}
			else
			{
				var genericTypeIndex = typeInfo.DeclaringType.GetTypeInfo().GenericTypeParameters.ToList().IndexOf(typeInfo.AsType());
				if (genericTypeIndex == -1)
					throw new InvalidOperationException("Unexpected type: " + typeInfo);

				stringBuilder.Append("`" + genericTypeIndex);
			}

			if (stringBuilder.Length == 0)
				throw new InvalidOperationException("Unexpected type: " + typeInfo);

			return stringBuilder.ToString();
		}

		private static string GetXmlDocMemberPart(MemberInfo memberInfo) =>
			GetXmlDocTypePart(memberInfo.DeclaringType.GetTypeInfo()) +
			"." +
			memberInfo.Name.Replace('.', '#');

		private static string GetXmlDocParameters(ParameterInfo[] parameters)
		{
			if (parameters.Length == 0)
				return "";

			return "(" +
				string.Join(",", parameters.Select(x => GetXmlDocTypePart(x.ParameterType.GetTypeInfo()))) +
				")";
		}
	}
}
