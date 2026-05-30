namespace ExampleAssembly;

/// <summary>Static interface member syntax.</summary>
/// <typeparam name="TSelf">The implementing type.</typeparam>
public interface IExampleStaticAbstractInterface<TSelf>
	where TSelf : IExampleStaticAbstractInterface<TSelf>
{
	/// <summary>Creates a value.</summary>
	abstract static TSelf Create();

	/// <summary>Returns the value.</summary>
	/// <param name="value">The value.</param>
	virtual static TSelf Identity(TSelf value) => value;
}
