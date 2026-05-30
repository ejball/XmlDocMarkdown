using System.Collections.ObjectModel;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Xml.XPath;

namespace XmlDocGen.Core.Xml;

/// <summary>Parsed inline XML documentation content.</summary>
public sealed class XmlDocXmlInline
{
	/// <summary>Initializes a new instance of the <see cref="XmlDocXmlInline"/> class.</summary>
	public XmlDocXmlInline(XmlDocXmlInlineKind kind, string? text)
	{
		Kind = kind;
		Text = text;
	}

	/// <summary>Gets the inline kind.</summary>
	public XmlDocXmlInlineKind Kind { get; }

	/// <summary>Gets the display text.</summary>
	public string? Text { get; }

	/// <summary>Gets the referenced XML documentation identifier.</summary>
	public XmlDocRef? Ref { get; init; }

	/// <summary>Gets the linked URL.</summary>
	public string? Href { get; init; }

	/// <summary>Gets the language keyword.</summary>
	public string? Langword { get; init; }

	/// <summary>Gets the referenced parameter or type parameter name.</summary>
	public string? Name => Text;
}
