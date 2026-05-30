using System.Collections.ObjectModel;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Xml.XPath;

namespace XmlDocGen.Core.Xml;

/// <summary>Parsed XML documentation for an exception.</summary>
public sealed class XmlDocXmlException(XmlDocRef? exceptionTypeRef)
{
	/// <summary>Gets the exception type reference.</summary>
	public XmlDocRef? ExceptionTypeRef { get; } = exceptionTypeRef;

	/// <summary>Gets the documented condition.</summary>
	public Collection<XmlDocXmlBlock> Condition { get; } = [];
}
