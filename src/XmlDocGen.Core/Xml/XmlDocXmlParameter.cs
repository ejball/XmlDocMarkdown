using System.Collections.ObjectModel;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Xml.XPath;

namespace XmlDocGen.Core.Xml;

/// <summary>Parsed XML documentation for a parameter.</summary>
public sealed class XmlDocXmlParameter(string name)
{
	/// <summary>Gets the parameter name.</summary>
	public string Name { get; } = name;

	/// <summary>Gets the parameter description.</summary>
	public Collection<XmlDocXmlBlock> Description { get; } = [];
}
