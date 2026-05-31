using System.Collections.ObjectModel;
using System.Xml.Linq;

namespace XmlDocGen.Core.Xml;

/// <summary>An in-memory representation of a compiler-generated XML documentation file.</summary>
public sealed class XmlDocXmlFile
{
	/// <summary>Initializes a new empty XML documentation file.</summary>
	public XmlDocXmlFile()
	{
		Members = [];
	}

	/// <summary>Initializes a new instance of the <see cref="XmlDocXmlFile"/> class.</summary>
	public XmlDocXmlFile(XDocument document)
	{
		ArgumentNullException.ThrowIfNull(document);
		AssemblyName = document.Root?.Element("assembly")?.Element("name")?.Value;
		Members = [.. document.Root?.Elements("members").Elements("member").Where(x => x.Attribute("name") is not null).Select(x => new XmlDocXmlMember(x)) ?? []];
	}

	/// <summary>Gets the assembly name from the XML documentation file, if present.</summary>
	public string? AssemblyName { get; }

	/// <summary>Gets the parsed members.</summary>
	public Collection<XmlDocXmlMember> Members { get; }

	/// <summary>Finds documentation for the given reference.</summary>
	public XmlDocXmlMember? FindMember(XmlDocRef reference) => Members.FirstOrDefault(x => x.Ref == reference);
}
