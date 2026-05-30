using System.Collections.ObjectModel;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Xml.XPath;

namespace XmlDocGen.Core.Xml;

/// <summary>A block of parsed XML documentation content.</summary>
public sealed class XmlDocXmlBlock
{
	/// <summary>Gets inline content in this block.</summary>
	public Collection<XmlDocXmlInline> Inlines { get; } = [];

	/// <summary>Gets or sets a value indicating whether this block is fenced code.</summary>
	public bool IsCode { get; set; }

	/// <summary>Gets or sets the code language hint.</summary>
	public string? Language { get; set; }

	/// <summary>Gets or sets the list kind, when this block is part of a list.</summary>
	public XmlDocXmlListKind? ListKind { get; set; }

	/// <summary>Gets or sets the list depth.</summary>
	public int ListDepth { get; set; }

	/// <summary>Gets or sets a value indicating whether this block is a list header.</summary>
	public bool IsListHeader { get; set; }

	/// <summary>Gets or sets a value indicating whether this block is a definition-list term.</summary>
	public bool IsListTerm { get; set; }
}
