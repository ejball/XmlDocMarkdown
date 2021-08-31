using System;
using System.Collections.Generic;

namespace ExampleAssembly
{
	/// <summary>
	/// A C# 9 generic record&lt;T&gt;.
	/// </summary>
	/// <param name="Name">A readonly string</param>
	/// <param name="Age">A readonly int</param>
	/// <param name="Days">A Hashset&lt;DayOfWeek&gt;</param>
	/// <param name="GenericType">A generic type parameter.</param>
	/// <param name="GenericLambda">An Action&lt;T&gt; lambda parameter.</param>
	/// <typeparam name="T">Some generic type</typeparam>
	public record ExampleRecord<T>(string Name, int Age, HashSet<DayOfWeek> Days, T GenericType, Action<T> GenericLambda);
}
