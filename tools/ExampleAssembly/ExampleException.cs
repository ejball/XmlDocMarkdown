namespace ExampleAssembly
{
	/// <summary>
	/// An example exception.
	/// </summary>
	public class ExampleException : Exception
	{
		/// <summary>
		/// Creates an instance.
		/// </summary>
		/// <param name="message">The message.</param>
		/// <param name="innerException">The inner exception.</param>
		public ExampleException(string message, Exception? innerException = null)
			: base(message, innerException)
		{
		}
	}
}
