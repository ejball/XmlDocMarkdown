using ExampleAssembly.InnerNamespace;

namespace ExampleAssembly
{
	/// <summary>
	/// A class with nested types.
	/// </summary>
	/// <remarks>The <see cref="ExampleDeepClass"/> class has a <see cref="NestedDelegate"/> and a <see cref="NestedClass"/>
	/// with a <see cref="NestedClass.VeryNestedStruct"/> and a <see cref="NestedClass.VeryNestedStruct.VeryVeryNestedInterface"/>.
	/// Another type in this namespace is <see cref="ExampleClass"/> with method <see cref="ExampleClass.Create(string)"/>. A type
	/// in an inner namespace is <see cref="ExampleInnerClass"/>, which has a constructor <see cref="ExampleInnerClass()"/>.</remarks>
	public class ExampleDeepClass
	{
		/// <summary>
		/// A nested delegate.
		/// </summary>
		/// <remarks>The <see cref="ExampleDeepClass"/> class has a <see cref="NestedDelegate"/> and a <see cref="NestedClass"/>
		/// with a <see cref="NestedClass.VeryNestedStruct"/> and a <see cref="NestedClass.VeryNestedStruct.VeryVeryNestedInterface"/>.
		/// Another type in this namespace is <see cref="ExampleClass"/> with method <see cref="ExampleClass.Create(string)"/>. A type
		/// in an inner namespace is <see cref="ExampleInnerClass"/>, which has a constructor <see cref="ExampleInnerClass()"/>.</remarks>
		public delegate void NestedDelegate();

		/// <summary>
		/// A nested class.
		/// </summary>
		/// <remarks>The <see cref="ExampleDeepClass"/> class has a <see cref="NestedDelegate"/> and a <see cref="NestedClass"/>
		/// with a <see cref="VeryNestedStruct"/> and a <see cref="VeryNestedStruct.VeryVeryNestedInterface"/>.
		/// Another type in this namespace is <see cref="ExampleClass"/> with method <see cref="ExampleClass.Create(string)"/>. A type
		/// in an inner namespace is <see cref="ExampleInnerClass"/>, which has a constructor <see cref="ExampleInnerClass()"/>.</remarks>
		public class NestedClass
		{
			/// <summary>
			/// A very nested structure.
			/// </summary>
			/// <remarks>The <see cref="ExampleDeepClass"/> class has a <see cref="NestedDelegate"/> and a <see cref="NestedClass"/>
			/// with a <see cref="VeryNestedStruct"/> and a <see cref="VeryVeryNestedInterface"/>.
			/// Another type in this namespace is <see cref="ExampleClass"/> with method <see cref="ExampleClass.Create(string)"/>. A type
			/// in an inner namespace is <see cref="ExampleInnerClass"/>, which has a constructor <see cref="ExampleInnerClass()"/>.</remarks>
			public struct VeryNestedStruct
			{
				/// <summary>
				/// A very nested property.
				/// </summary>
				/// <remarks>The <see cref="ExampleDeepClass"/> class has a <see cref="NestedDelegate"/> and a <see cref="NestedClass"/>
				/// with a <see cref="VeryNestedStruct"/> and a <see cref="VeryVeryNestedInterface"/>.
				/// Another type in this namespace is <see cref="ExampleClass"/> with method <see cref="ExampleClass.Create(string)"/>. A type
				/// in an inner namespace is <see cref="ExampleInnerClass"/>, which has a constructor <see cref="ExampleInnerClass()"/>.</remarks>
				public bool IsDeep { get; set; }

				/// <summary>
				/// A very, very nested interface.
				/// </summary>
				/// <remarks>The <see cref="ExampleDeepClass"/> class has a <see cref="NestedDelegate"/> and a <see cref="NestedClass"/>
				/// with a <see cref="VeryNestedStruct"/> and a <see cref="VeryVeryNestedInterface"/>.
				/// Another type in this namespace is <see cref="ExampleClass"/> with method <see cref="ExampleClass.Create(string)"/>. A type
				/// in an inner namespace is <see cref="ExampleInnerClass"/>, which has a constructor <see cref="ExampleInnerClass()"/>.</remarks>
				public interface VeryVeryNestedInterface
				{
				}
			}
		}

		/// <summary>
		/// A protected nested class.
		/// </summary>
		protected class ProtectedNestedClass
		{
		}

		/// <summary>
		/// An internal nested class.
		/// </summary>
		internal class InternalNestedClass
		{
		}

		/// <summary>
		/// A protected internal nested class.
		/// </summary>
		protected internal class ProtectedInternalNestedClass
		{
		}

		/// <summary>
		/// A private nested class.
		/// </summary>
		private class PrivateNestedClass
		{
		}
	}
}
