using System.Collections.ObjectModel;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Xml.XPath;

namespace XmlDocGen.Core.Xml;

/// <summary>Parsed XML documentation for a see-also item.</summary>
public sealed class XmlDocXmlSeeAlso(XmlDocRef? reference, string? href, string? text)
{
	/// <summary>Gets the referenced XML documentation identifier.</summary>
	public XmlDocRef? Ref { get; } = reference;

	/// <summary>Gets the external URL.</summary>
	public string? Href { get; } = href;

	/// <summary>Gets the display text.</summary>
	public string? Text { get; } = text;
}
