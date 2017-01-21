using System;

namespace XmlDocMarkdown
{
	/// <summary>
	/// Thrown when an error occurs while processing command-line arguments.
	/// </summary>
	public sealed class ArgsReaderException : Exception
	{
		/// <summary>
		/// Creates an exception.
		/// </summary>
		public ArgsReaderException(string message, Exception innerException = null)
			: base(message, innerException)
		{
		}
	}
}
