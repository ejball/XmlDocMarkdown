using System.Collections.Generic;

namespace ExampleAssembly
{
	/// <summary>
	/// A derived interface.
	/// </summary>
	public interface IExampleDerivedInterface : IReadOnlyDictionary<string, IExampleInterface>, IDictionary<string, IExampleInterface>
	{
	}
}
