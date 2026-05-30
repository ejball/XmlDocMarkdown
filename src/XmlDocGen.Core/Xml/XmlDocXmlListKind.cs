using System.Collections.ObjectModel;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Xml.XPath;

namespace XmlDocGen.Core.Xml;

/// <summary>Kinds of XML documentation lists.</summary>
public enum XmlDocXmlListKind
{
	/// <summary>A bullet list.</summary>
	Bullet,
	/// <summary>A numbered list.</summary>
	Number,
	/// <summary>A table.</summary>
	Table,
	/// <summary>A definition list.</summary>
	Definition,
}
