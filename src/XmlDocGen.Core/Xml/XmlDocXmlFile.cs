using System.Collections.ObjectModel;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Xml.XPath;

namespace XmlDocGen.Core.Xml;

/// <summary>An in-memory representation of a compiler-generated XML documentation file.</summary>
public sealed class XmlDocXmlFile
{
	/// <summary>Initializes a new empty XML documentation file.</summary>
	public XmlDocXmlFile()
	{
		Members = [];
		m_membersByRef = new Dictionary<XmlDocRef, XmlDocXmlMember>();
	}

	/// <summary>Initializes a new instance of the <see cref="XmlDocXmlFile"/> class.</summary>
	public XmlDocXmlFile(XDocument document)
	{
		ArgumentNullException.ThrowIfNull(document);
		AssemblyName = document.Root?.Element("assembly")?.Element("name")?.Value;
		Members = [.. document.Root?.Elements("members").Elements("member").Where(x => x.Attribute("name") is not null).Select(x => new XmlDocXmlMember(x)) ?? []];
		m_membersByRef = Members.GroupBy(x => x.Ref).ToDictionary(x => x.Key, x => x.First());
	}

	/// <summary>Loads XML documentation from a file path.</summary>
	public static XmlDocXmlFile Load(string path) => new(LoadDocument(path));

	/// <summary>Loads XML documentation from a stream.</summary>
	public static XmlDocXmlFile Load(Stream stream) => new(XDocument.Load(stream));

	/// <summary>Parses XML documentation from a string.</summary>
	public static XmlDocXmlFile Parse(string xml) => new(XDocument.Parse(xml));

	/// <summary>Gets the assembly name from the XML documentation file, if present.</summary>
	public string? AssemblyName { get; }

	/// <summary>Gets the parsed members.</summary>
	public IReadOnlyList<XmlDocXmlMember> Members { get; }

	/// <summary>Finds documentation for the given reference.</summary>
	public XmlDocXmlMember? FindMember(XmlDocRef reference) => m_membersByRef.GetValueOrDefault(reference);

	private static XDocument LoadDocument(string path)
	{
		var document = XDocument.Load(path);
		ResolveIncludes(document, Path.GetDirectoryName(Path.GetFullPath(path)) ?? "");
		return document;
	}

	private static void ResolveIncludes(XDocument document, string basePath)
	{
		foreach (var include in document.Descendants("include").ToList())
		{
			var file = include.Attribute("file")?.Value;
			var path = include.Attribute("path")?.Value;
			if (string.IsNullOrWhiteSpace(file) || string.IsNullOrWhiteSpace(path))
				continue;

			var includePath = Path.GetFullPath(Path.Combine(basePath, file));
			var includeDocument = XDocument.Load(includePath);
			include.ReplaceWith(includeDocument.XPathSelectElements(path).Select(CloneElement));
		}
	}

	private static XElement CloneElement(XElement element) => new(element);

	private readonly IReadOnlyDictionary<XmlDocRef, XmlDocXmlMember> m_membersByRef;
}
