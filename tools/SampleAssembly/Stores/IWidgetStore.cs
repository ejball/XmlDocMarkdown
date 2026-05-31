namespace SampleAssembly.Stores;

/// <summary>Stores sample widgets.</summary>
public interface IWidgetStore
{
	/// <summary>Gets a widget by name.</summary>
	/// <param name="name">The widget name.</param>
	/// <returns>The matching widget.</returns>
	Widget Get(string name);
}
