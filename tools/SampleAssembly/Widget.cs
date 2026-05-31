namespace SampleAssembly;

/// <summary>A sample widget.</summary>
/// <remarks>Use widgets to demonstrate generated API documentation.</remarks>
public class Widget
{
	/// <summary>Initializes a new instance of the <see cref="Widget" /> class.</summary>
	/// <param name="name">The widget name.</param>
	public Widget(string name)
	{
		Name = name;
	}

	/// <summary>Gets the widget name.</summary>
	/// <value>The widget name.</value>
	public string Name { get; }

	/// <summary>Creates a default widget.</summary>
	/// <returns>A default widget.</returns>
	public static Widget Create() => new("Default");

	/// <summary>Formats this widget.</summary>
	/// <param name="count">The number of times to format the name.</param>
	/// <returns>A formatted widget name.</returns>
	/// <seealso cref="WidgetKind" />
	/// <seealso cref="string" />
	public string Format(int count) => string.Join(", ", Enumerable.Repeat(Name, count));

	/// <summary>Gets the number of times this widget has been reset.</summary>
	internal int ResetCount => m_resetCount;

	/// <summary>Resets internal widget state.</summary>
	internal void Reset() => m_resetCount++;

	private int m_resetCount;
}
