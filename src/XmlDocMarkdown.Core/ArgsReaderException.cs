using System;
using System.Diagnostics.CodeAnalysis;

namespace XmlDocMarkdown.Core
{
	/// <summary>
	/// Thrown when an error occurs while processing command-line arguments.
	/// </summary>
	[SuppressMessage("Design", "CA1064:Exceptions should be public", Justification = "hack to avoid dependency")]
	internal sealed class ArgsReaderException : Exception
	{
		/// <summary>
		/// Creates an exception.
		/// </summary>
		/// <param name="message">The exception message.</param>
		/// <param name="innerException">The inner exception.</param>
		public ArgsReaderException(string message, Exception innerException = null)
			: base(message, innerException)
		{
		}
	}
}
