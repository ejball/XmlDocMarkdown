using System.Collections.Generic;

namespace ExampleAssembly
{
	/// <summary>
	/// A generic class with three generic type parameters.
	/// </summary>
	/// <typeparam name="TOne">The first generic type.</typeparam>
	/// <typeparam name="TTwo">The second generic type.</typeparam>
	/// <typeparam name="TThree">The third generic type.</typeparam>
	public class ExampleTriGenericClass<TOne, TTwo, TThree>
		where TTwo : struct, IEnumerable<string>
		where TThree : class, TOne, IEnumerable<TTwo?>
	{
	}
}
