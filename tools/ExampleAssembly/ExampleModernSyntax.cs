namespace ExampleAssembly;

/// <summary>Modern C# signature syntax.</summary>
/// <param name="name">The name.</param>
public unsafe class ExampleModernSyntax(string name)
{
	private readonly int m_value = 42;

	/// <summary>Gets the primary-constructor name.</summary>
	public string Name { get; } = name;

	/// <summary>Gets or sets nullable text.</summary>
	public string? NullableText { get; set; }

	/// <summary>Gets a ref readonly value.</summary>
	public ref readonly int RefReadonlyValue => ref m_value;

	/// <summary>Gets a function pointer.</summary>
	/// <param name="callback">The callback.</param>
	public delegate*<int, int> GetFunctionPointer(delegate*<int, int> callback) => callback;

	/// <summary>Updates a scoped reference.</summary>
	/// <param name="value">The value.</param>
	public void UpdateScoped(scoped ref int value) => value++;

	/// <summary>Reads a ref readonly parameter.</summary>
	/// <param name="value">The value.</param>
	public void ReadRefReadonly(ref readonly int value)
	{
	}

	/// <summary>Gets tuple names and native integers.</summary>
	/// <param name="input">The input.</param>
	public (int Count, string? Name) TupleNames((nint Index, nuint Length) input) => ((int) input.Length, Name);

	/// <summary>Shows modern constraints.</summary>
	/// <typeparam name="TNotNull">The not-null type.</typeparam>
	/// <typeparam name="TUnmanaged">The unmanaged type.</typeparam>
	/// <typeparam name="TClass">The nullable reference type.</typeparam>
	public void Constrained<TNotNull, TUnmanaged, TClass>()
		where TNotNull : notnull
		where TUnmanaged : unmanaged
		where TClass : class?
	{
	}

	/// <summary>Checked addition.</summary>
	/// <param name="left">The left value.</param>
	/// <param name="right">The right value.</param>
	public static ExampleModernSyntax operator +(ExampleModernSyntax left, ExampleModernSyntax right) => left;

	/// <summary>Checked addition.</summary>
	/// <param name="left">The left value.</param>
	/// <param name="right">The right value.</param>
	public static ExampleModernSyntax operator checked +(ExampleModernSyntax left, ExampleModernSyntax right) => left;

	/// <summary>Unsigned right shift.</summary>
	/// <param name="value">The value.</param>
	/// <param name="shift">The shift.</param>
	public static ExampleModernSyntax operator >>>(ExampleModernSyntax value, int shift) => value;
}
