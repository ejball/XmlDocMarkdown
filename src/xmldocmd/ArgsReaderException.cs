using System;

namespace ArgsReading
{
	/// <summary>
	/// Thrown when an error occurs while processing command-line arguments.
	/// </summary>
	public sealed class ArgsReaderException : Exception
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
