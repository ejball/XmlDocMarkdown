namespace ExampleAssembly
{
	/// <summary>
	/// A C# 9 generic record.
	/// </summary>
	/// <param name="Name">A string.</param>
	/// <param name="Age">An integer.</param>
	/// <param name="Days">A hash set.</param>
	/// <param name="GenericType">A generic type parameter.</param>
	/// <param name="GenericLambda">An lambda parameter.</param>
	/// <typeparam name="T">Some generic type.</typeparam>
	public record ExampleRecord<T>(string Name, int Age, HashSet<DayOfWeek> Days, T GenericType, Action<T> GenericLambda);
}
