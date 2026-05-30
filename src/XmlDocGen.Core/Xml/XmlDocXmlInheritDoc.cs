using System.Collections.ObjectModel;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Xml.XPath;

namespace XmlDocGen.Core.Xml;

/// <summary>The raw XML inheritdoc directive.</summary>
public sealed class XmlDocXmlInheritDoc(XmlDocRef? cref, string? path)
{
	/// <summary>Gets the optional inherited member reference.</summary>
	public XmlDocRef? Cref { get; } = cref;

	/// <summary>Gets the optional XML path.</summary>
	public string? Path { get; } = path;
}
