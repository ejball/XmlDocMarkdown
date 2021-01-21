using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

#pragma warning disable 67

namespace ExampleAssembly
{
	/// <summary>
	/// A class.
	/// </summary>
	public class ExampleClass : IExampleContravariantInterface<ExampleClass>, IExampleCovariantInterface<string>
	{
		/// <summary>
		/// A no-arg constructor.
		/// </summary>
		public ExampleClass()
		{
		}

		/// <summary>
		/// A one-arg constructor.
		/// </summary>
		/// <param name="id">The ID.</param>
		/// <remarks><para>These remarks reference parameter <paramref name="id"/>
		/// at the end of a line.</para><para>These remarks reference parameter
		/// <paramref name="id"/> at the start of a line.</para></remarks>
		public ExampleClass(string id)
		{
		}

		/// <summary>
		/// A static lifetime field.
		/// </summary>
		public static readonly ExampleClass Default = new ExampleClass();

		/// <summary>
		/// A static lifetime property.
		/// </summary>
		public static ExampleClass Instance { get; } = new ExampleClass();

		/// <summary>
		/// A static lifetime method.
		/// </summary>
		public static ExampleClass Create()
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Another static lifetime method.
		/// </summary>
		public static ExampleClass Create(string id)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// A read-only property.
		/// </summary>
		/// <value>The ID.</value>
		public string Id { get; } = BlankId;

		/// <summary>
		/// A read-write property with a much-longer than expected summary to see
		/// if there is any word wrapping in the member name.
		/// </summary>
		public double Weight { get; set; }

		/// <summary>
		/// A static read-only field.
		/// </summary>
		public static readonly double DefaultWeight = 1.0;

		/// <summary>
		/// A static field.
		/// </summary>
		public static int GlobalVariable;

		/// <summary>
		/// A constant field.
		/// </summary>
		public const string BlankId = "";

		/// <summary>
		/// A static read-only property.
		/// </summary>
		public static double MinWeight { get; } = 0.0;

		/// <summary>
		/// A static read-write property.
		/// </summary>
		public static double MaxWeight { get; set; }

		/// <summary>
		/// An event.
		/// </summary>
		public event EventHandler? WeightChanged;

		/// <summary>
		/// A static event.
		/// </summary>
		public static event EventHandler? MaxWeightChanged;

		/// <summary>
		/// A virtual method.
		/// </summary>
		public virtual void Jump()
		{
		}

		/// <summary>
		/// A boring static method. It has a really long summary because things get interesting when
		/// table cells have to wrap. Also, we should probably cut the summary off at some point, since
		/// some documenters tend to put way more in the summary than would generally be expected.
		/// </summary>
		public static void JumpAll()
		{
		}

		/// <summary>
		/// A public field.
		/// </summary>
		public bool IsBadIdea;

		/// <summary>
		/// An overloaded method.
		/// </summary>
		/// <typeparam name="T">The type parameter.</typeparam>
		/// <param name="x">The parameter.</param>
		/// <remarks>We can put special characters in <c>&lt;c&gt;</c> elements,
		/// <c>like | and ` and &amp;#x21;</c>.</remarks>
		public T Overloaded<T>(string x)
		{
			return default!;
		}

		/// <summary>
		/// An overloaded method.
		/// </summary>
		/// <typeparam name="T">The type parameter.</typeparam>
		/// <typeparam name="U">The second type parameter.</typeparam>
		/// <param name="x">The parameter.</param>
		/// <param name="y">The second parameter.</param>
		public void Overloaded<T, U>(T x, U y)
			where T : class
			where U : struct
		{
		}

		/// <summary>
		/// An overloaded method.
		/// </summary>
		/// <typeparam name="T">The type parameter.</typeparam>
		/// <param name="x">The parameter.</param>
		public void Overloaded<T>(T x)
		{
		}

		/// <summary>
		/// An overloaded method.
		/// </summary>
		/// <typeparam name="T">The type parameter.</typeparam>
		public T Overloaded<T>()
		{
			return default!;
		}

		/// <summary>
		/// An overloaded method.
		/// </summary>
		/// <param name="x">The parameter.</param>
		public void Overloaded(string x)
		{
		}

		/// <summary>
		/// An overloaded method.
		/// </summary>
		public void Overloaded()
		{
		}

		/// <summary>
		/// An overloaded method.
		/// </summary>
		/// <param name="x">The parameter.</param>
		public void Overloaded(int x)
		{
		}

		/// <summary>
		/// A method with default parameters.
		/// </summary>
		public void DefaultParameters<T>(bool @bool = true, bool? no = false, bool? maybe = null,
			byte @byte = byte.MaxValue, sbyte @sbyte = sbyte.MaxValue,
			char @char = '\u1234', decimal @decimal = 3.14m, double @double = double.NaN, float @float = float.NegativeInfinity,
			int @int = -42, uint @uint = 42, long @long = long.MinValue, ulong @ulong = long.MaxValue,
			object? @object = null, short @short = short.MinValue, ushort @ushort = ushort.MaxValue,
			string @string = "hi\0'\"\\\a\b\f\n\r\t\v\u0001\uABCD", T t = default(T),
			DateTime @virtual = default(DateTime),
			ExampleEnum @enum = ExampleEnum.One,
			ExampleFlagsEnum flags = ExampleFlagsEnum.Second | ExampleFlagsEnum.Third)
		{
		}

		/// <summary>
		/// A method with a really long name.
		/// </summary>
		public void LongMethodNameWithTemplateParametersAndMethodParameters<T>(T t)
		{
		}

		/// <summary>
		/// A method whose summary references <paramref name="value"/>.
		/// </summary>
		/// <param name="value">The value</param>
		public void ParameterReference(int value)
		{
		}

		/// <summary>
		/// A method whose summary references <typeparamref name="T"/>.
		/// </summary>
		/// <param name="value">The value of type <typeparamref name="T"/>.</param>
		public void TypeParameterReference<T>(T value)
		{
		}

		/// <summary>
		/// A method that tries to get a value.
		/// </summary>
		/// <param name="value">The value.</param>
		public bool TryGetValue(out object? value)
		{
			value = default;
			return false;
		}

		/// <summary>
		/// A method that tries to get a value.
		/// </summary>
		/// <param name="value">The value of type <typeparamref name="T"/>.</param>
		public bool TryGetValue<T>(out T value)
		{
			value = default!;
			return false;
		}

		/// <summary>
		/// A method that edits a value.
		/// </summary>
		/// <param name="value">The value to edit.</param>
		public void EditValue(ref object value)
		{
		}

		/// <summary>
		/// A method that edits a value.
		/// </summary>
		/// <param name="value">The value to edit of type <typeparamref name="T"/>.</param>
		public void EditValue<T>(ref T value)
		{
		}

		/// <summary>
		/// A method with parameters.
		/// </summary>
		/// <param name="parameters">The parameters.</param>
		public void HasParams(params string[] parameters)
		{
		}

		/// <summary>
		/// A method that uses caller info.
		/// </summary>
		public void UsesCallerInfo([CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
		{
		}

		/// <summary>
		/// A method whose docs have <a href="https://developer.mozilla.org/en-US/docs/Web/HTML/Element/a">hyperlinks</a>.
		/// </summary>
		/// <remarks>Visit <see href="https://ejball.com/" /> for <see href="https://ejball.com/">more info</see>.</remarks>
		public void HasHyperlinks()
		{
		}

		/// <summary>
		/// An obsolete method.
		/// </summary>
		[Obsolete("This method is old and busted.")]
		public void OldAndBusted()
		{
		}

		/// <summary>
		/// An unbrowsable method.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void UnbrowsableMethod()
		{
		}
	}
}
