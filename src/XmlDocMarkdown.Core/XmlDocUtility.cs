using System;
using System.Linq;
using System.Reflection;
using System.Text;

namespace XmlDocMarkdown.Core
{
	public static class XmlDocUtility
	{
		public static string GetXmlDocName(MemberInfo memberInfo)
		{
			if (memberInfo == null)
				return null;

			var typeInfo = memberInfo as TypeInfo;
			if (typeInfo != null)
				return "T:" + GetXmlDocTypePart(typeInfo);

			var methodBase = memberInfo as MethodBase;
			if (methodBase != null)
			{
				return "M:" +
					GetXmlDocMemberPart(methodBase) +
					(methodBase.IsGenericMethodDefinition ? $"``{methodBase.GetGenericArguments().Length}" : "") +
					GetXmlDocParameters(methodBase.GetParameters());
			}

			var propertyInfo = memberInfo as PropertyInfo;
			if (propertyInfo != null)
				return "P:" + GetXmlDocMemberPart(propertyInfo);

			var eventInfo = memberInfo as EventInfo;
			if (eventInfo != null)
				return "E:" + GetXmlDocMemberPart(eventInfo);

			var fieldInfo = memberInfo as FieldInfo;
			if (fieldInfo != null)
				return "F:" + GetXmlDocMemberPart(fieldInfo);

			throw new InvalidOperationException("Unexpected member: " + memberInfo);
		}

		private static string GetXmlDocTypePart(TypeInfo typeInfo)
		{
			var stringBuilder = new StringBuilder();

			if (!typeInfo.IsGenericParameter)
			{
				if (typeInfo.DeclaringType != null)
					stringBuilder.Append(GetXmlDocTypePart(typeInfo.DeclaringType.GetTypeInfo()) + ".");
				else if (!string.IsNullOrEmpty(typeInfo.Namespace))
					stringBuilder.Append(typeInfo.Namespace + ".");

				if (typeInfo.IsGenericType && !typeInfo.IsGenericTypeDefinition)
				{
					stringBuilder.Append(typeInfo.Name.Substring(0, typeInfo.Name.IndexOf('`')));
					stringBuilder.Append("{");
					stringBuilder.Append(string.Join(",", typeInfo.GenericTypeArguments.Select(x => GetXmlDocTypePart(x.GetTypeInfo()))));
					stringBuilder.Append("}");
				}
				else
				{
					stringBuilder.Append(typeInfo.Name);
				}
			}
			else if (typeInfo.DeclaringMethod != null)
			{
				stringBuilder.Append("``" + typeInfo.DeclaringMethod.GetGenericArguments().ToList().IndexOf(typeInfo.AsType()));
			}
			else
			{
				stringBuilder.Append("`" + typeInfo.DeclaringType.GetTypeInfo().GenericTypeArguments.ToList().IndexOf(typeInfo.AsType()));
			}

			if (stringBuilder.Length == 0)
				throw new InvalidOperationException("Unexpected type: " + typeInfo);

			return stringBuilder.ToString();
		}

		private static string GetXmlDocMemberPart(MemberInfo memberInfo)
		{
			return GetXmlDocTypePart(memberInfo.DeclaringType.GetTypeInfo()) +
				"." +
				memberInfo.Name.Replace('.', '#');
		}

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
