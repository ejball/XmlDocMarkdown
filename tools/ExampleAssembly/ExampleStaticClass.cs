namespace ExampleAssembly
{
	/// <summary>
	/// A static class.
	/// </summary>
	public static class ExampleStaticClass
	{
		/// <summary>
		/// Gets the next enumerated value.
		/// </summary>
		public static ExampleEnum GetNext(this ExampleEnum value)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Clones the specified array.
		/// </summary>
		/// <param name="array">The array to clone.</param>
		/// <returns>A clone of the specified array.</returns>
		/// <remarks>This method is merely useful in avoiding the cast that is otherwise necessary
		/// when calling <see cref="Array.Clone" />.</remarks>
		public static T[] Clone<T>(T[] array) => (T[]) array.Clone();
	}
}
