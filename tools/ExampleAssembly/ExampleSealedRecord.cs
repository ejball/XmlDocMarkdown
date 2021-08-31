using System;

namespace ExampleAssembly
{
	/// <summary>
	/// A sealed C# 9 record.
	/// </summary>
	public sealed record ExampleSealedRecord
	{
		/// <summary>
		/// Constructs an instance.
		/// </summary>
		/// <param name="value">The value.</param>
		public ExampleSealedRecord(string value) => Value = value ?? throw new ArgumentNullException(nameof(value));

		/// <summary>
		/// The value.
		/// </summary>
		public string Value { get; }
	}
}
