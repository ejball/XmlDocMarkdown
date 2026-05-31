using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace XmlDocGen.Core.Xml;

/// <summary>An XML documentation identifier, e.g. <c>T:My.Type</c> or <c>M:My.Type.Method(System.Int32)</c>.</summary>
public readonly partial struct XmlDocRef : IEquatable<XmlDocRef>
{
	/// <summary>Initializes a new instance of the <see cref="XmlDocRef"/> struct.</summary>
	public XmlDocRef(string value)
	{
		ArgumentException.ThrowIfNullOrEmpty(value);
		if (value.Length < 3 || value[1] != ':')
			throw new ArgumentException("XML documentation identifiers must start with a one-letter prefix and ':'.", nameof(value));

		Value = value;
	}

	/// <summary>Gets the raw identifier string.</summary>
	public string Value { get; }

	/// <summary>Gets the unqualified member or type name from the identifier.</summary>
	public string ShortName
	{
		get
		{
			var match = XmlDocNameRegex().Match(Value);
			return match.Success ? match.Groups["name"].Value : Value;
		}
	}

	/// <summary>Builds a reference from a reflection type.</summary>
	public static XmlDocRef ForType(Type type) => new(GetXmlDocRef(type.GetTypeInfo())!);

	/// <summary>Builds a reference from a reflected member.</summary>
	public static XmlDocRef ForMember(MemberInfo member) => new(GetXmlDocRef(member)!);

	/// <summary>Builds a reference for a namespace.</summary>
	public static XmlDocRef ForNamespace(string namespaceName) => new("N:" + namespaceName);

	/// <inheritdoc />
	public bool Equals(XmlDocRef other) => StringComparer.Ordinal.Equals(Value, other.Value);

	/// <inheritdoc />
	public override bool Equals(object? obj) => obj is XmlDocRef other && Equals(other);

	/// <inheritdoc />
	public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(Value);

	/// <inheritdoc />
	public override string ToString() => Value;

	/// <summary>Compares two references for equality.</summary>
	public static bool operator ==(XmlDocRef left, XmlDocRef right) => left.Equals(right);

	/// <summary>Compares two references for inequality.</summary>
	public static bool operator !=(XmlDocRef left, XmlDocRef right) => !left.Equals(right);

	private static string? GetXmlDocRef(MemberInfo? memberInfo)
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
