namespace ExampleAssembly
{
	/// <summary>
	/// A generic class.
	/// </summary>
	/// <typeparam name="T">The generic type.</typeparam>
	public class ExampleGenericClass<T>
	{
		/// <summary>
		/// Creates an instance.
		/// </summary>
		public ExampleGenericClass(T value)
		{
		}

		/// <summary>
		/// The value.
		/// </summary>
		public IEnumerable<T>? Value { get; set; }

		/// <summary>
		/// Gets an example tuple.
		/// </summary>
		public ExampleTuple<T> GetTuple()
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Adds values.
		/// </summary>
		public void AddValues(IEnumerable<T> values)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Adds values.
		/// </summary>
		public T AddTuples(IEnumerable<(T Key, object? Value)> values)
		{
			throw new NotImplementedException();
		}
	}
}
