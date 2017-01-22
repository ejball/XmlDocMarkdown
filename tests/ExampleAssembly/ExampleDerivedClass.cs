using System;
using System.Collections;
using System.Collections.Generic;

namespace ExampleAssembly
{
	/// <summary>
	/// A class that derives from <see cref="ExampleClass"/>.
	/// </summary>
	public class ExampleDerivedClass : ExampleClass, IExampleInterface, IEnumerable<string>, IExampleInternalInterface
	{
		/// <summary>
		/// The enumerator.
		/// </summary>
		/// <returns>The strings.</returns>
		public IEnumerator<string> GetEnumerator()
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// The old-school enumerator.
		/// </summary>
		/// <returns>The strings.</returns>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}
