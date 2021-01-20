namespace ExampleAssembly
{
	/// <summary>
	/// A 64-bit enumeration.
	/// </summary>
	public enum ExampleLongEnum : ulong
	{
		/// <summary>
		/// Backward.
		/// </summary>
		Backward = ulong.MinValue,

		/// <summary>
		/// Forward.
		/// </summary>
		Forward = ulong.MaxValue,
	}
}
