namespace ExampleAssembly;

/// <summary>Readonly ref struct syntax.</summary>
public readonly ref struct ExampleReadOnlyRefStruct
{
	/// <summary>Initializes a new instance of the <see cref="ExampleReadOnlyRefStruct"/> struct.</summary>
	/// <param name="value">The value.</param>
	public ExampleReadOnlyRefStruct(int value)
	{
		Value = value;
	}

	/// <summary>Gets the value.</summary>
	public int Value { get; }
}
