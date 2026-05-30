using System.Diagnostics.CodeAnalysis;

namespace XmlDocMarkdown.Core;

[SuppressMessage("Design", "CA1064:Exceptions should be public", Justification = "For internal use.")]
internal sealed class ArgsReaderException : Exception
{
	public ArgsReaderException(string message, Exception? innerException = null)
		: base(message, innerException)
	{
	}
}
