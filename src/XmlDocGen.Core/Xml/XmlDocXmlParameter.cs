using System.Collections.ObjectModel;

namespace XmlDocGen.Core.Xml;

/// <summary>Parsed XML documentation for a parameter.</summary>
public sealed class XmlDocXmlParameter(string name)
{
	/// <summary>Gets the parameter name.</summary>
	public string Name { get; } = name;

	/// <summary>Gets the parameter description.</summary>
	public Collection<XmlDocXmlBlock> Description { get; } = [];
}
