using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Text.Json;
using XmlDocGen.Core.Nodes;
using XmlDocGen.Core.Xml;

namespace XmlDocGen.Core.Pages;

/// <summary>A generated documentation page before it is rendered.</summary>
public sealed class XmlDocPage
{
	/// <summary>Initializes a new instance of the <see cref="XmlDocPage"/> class.</summary>
	public XmlDocPage(string path, IEnumerable<XmlDocNode> nodes)
	{
		Path = path;
		Nodes = [.. nodes];
	}

	/// <summary>Gets the extensionless site-relative page path.</summary>
	public string Path { get; }

	/// <summary>Gets the nodes documented on this page.</summary>
	public IReadOnlyList<XmlDocNode> Nodes { get; }
}
