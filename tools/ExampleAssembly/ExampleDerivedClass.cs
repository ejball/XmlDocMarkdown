using System;
using System.Collections;
using System.Collections.Generic;
using ExampleAssembly.InnerNamespace;

namespace ExampleAssembly
{
	/// <summary>
	/// A class that derives from <see cref="ExampleClass"/>.
	/// </summary>
	public class ExampleDerivedClass : ExampleClass,
		IExampleInterface,
		IEnumerable<object>,
		IEnumerable<string>,
		IExampleInternalInterface,
		IExampleContravariantInterface<ExampleDerivedClass>,
		IExampleCovariantInterface<object>
	{
		/// <summary>
		/// A method with lots of see alsos.
		/// </summary>
		/// <seealso cref="ExampleTuple{T1}"/>
		/// <seealso cref="ExampleTuple&lt;T1, T2&gt;"/>
		/// <seealso cref="ExampleClass(string)"/>
		/// <seealso cref="ExampleClass.Default"/>
		/// <seealso cref="ExampleClass.Instance"/>
		/// <seealso cref="ExampleClass.Create(string)"/>
		/// <seealso cref="ExampleClass.Overloaded{T}(T)"/>
		/// <seealso cref="ExampleClass.WeightChanged"/>
		/// <seealso cref="ExampleInnerClass"/>
		/// <seealso cref="ExampleDeepClass.NestedDelegate"/>
		/// <seealso cref="ExampleDeepClass.NestedClass.VeryNestedStruct"/>
		public void SeeAlso()
		{
		}

		/// <summary>
		/// An implicitly implemented interface method.
		/// </summary>
		public int ExampleMethod(string value)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// An overridden method.
		/// </summary>
		/// <exception>Some exception is thrown.</exception>
		public override void Jump()
		{
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
		/// The enumerator for a less derived type.
		/// </summary>
		/// <returns>The strings.</returns>
		/// <exception cref="ExampleException">An example error occurred.</exception>
		/// <exception cref="NotImplementedException">It isn't implemented.</exception>
		IEnumerator<object> IEnumerable<object>.GetEnumerator()
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
