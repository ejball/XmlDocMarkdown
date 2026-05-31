using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;

namespace XmlDocGen.Core.Xml;

/// <summary>Parsed XML documentation for an exception.</summary>
[SuppressMessage("Naming", "CA1711:Identifiers should not have incorrect suffix", Justification = "Consistency.")]
public sealed class XmlDocXmlException(XmlDocRef? exceptionTypeRef)
{
	/// <summary>Gets the exception type reference.</summary>
	public XmlDocRef? ExceptionTypeRef { get; } = exceptionTypeRef;

	/// <summary>Gets the documented condition.</summary>
	public Collection<XmlDocXmlBlock> Condition { get; } = [];
}
