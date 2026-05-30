using System.Collections.ObjectModel;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Xml.XPath;

namespace XmlDocGen.Core.Xml;

/// <summary>An XML documentation identifier, e.g. <c>T:My.Type</c> or <c>M:My.Type.Method(System.Int32)</c>.</summary>
public readonly struct XmlDocRef : IEquatable<XmlDocRef>
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

	/// <summary>Builds a reference from a reflection type.</summary>
	public static XmlDocRef ForType(Type type) => new(XmlDocRefUtility.GetXmlDocRef(type.GetTypeInfo())!);

	/// <summary>Builds a reference from a reflected member.</summary>
	public static XmlDocRef ForMember(MemberInfo member) => new(XmlDocRefUtility.GetXmlDocRef(member)!);

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
}
