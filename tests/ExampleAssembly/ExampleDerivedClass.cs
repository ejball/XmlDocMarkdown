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
		/// An implicitly implemented interface method.
		/// </summary>
		public int ExampleMethod(string value)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// The enumerator.
		/// </summary>
		/// <returns>The strings.</returns>
		/// <exception cref="ExampleException">An example error occurred.</exception>
		/// <exception cref="NotImplementedException">It isn't implemented.</exception>
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
