using System;

namespace ExampleAssembly
{
	/// <summary>
	/// A flags enumeration.
	/// </summary>
	[Flags]
	public enum ExampleFlagsEnum
	{
		/// <summary>
		/// No bits.
		/// </summary>
		None = 0,

		/// <summary>
		/// First bit.
		/// </summary>
		First = 1,

		/// <summary>
		/// Second bit.
		/// </summary>
		Second = 2,

		/// <summary>
		/// Third bit.
		/// </summary>
		Third = 4,

		/// <summary>
		/// Fourth bit.
		/// </summary>
		Fourth = 8,

		/// <summary>
		/// All bits.
		/// </summary>
		All = First | Second | Third | Fourth,

		/// <summary>
		/// Insanity.
		/// </summary>
		Insanity = -1,
	}
}
