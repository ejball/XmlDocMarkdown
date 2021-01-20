namespace ExampleAssembly
{
	/// <summary>
	/// A generic delegate.
	/// </summary>
	/// <typeparam name="T1">The first generic type.</typeparam>
	/// <typeparam name="T2">The second generic type.</typeparam>
	/// <typeparam name="TResult">The result type.</typeparam>
	/// <param name="arg1">The first argument.</param>
	/// <param name="arg2">The second argument.</param>
	/// <returns>The result.</returns>
	public delegate TResult ExampleGenericDelegate<in T1, in T2, out TResult>(T1 arg1, T2 arg2)
		where T1 : class, new()
		where T2 : struct;
}
