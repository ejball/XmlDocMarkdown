namespace SampleExtraAssembly;

/// <summary>Creates widgets from an extra assembly.</summary>
public static class ExtraWidget
{
	/// <summary>Creates an advanced widget.</summary>
	/// <returns>An advanced widget.</returns>
	public static SampleAssembly.Widget CreateAdvanced() => new("Advanced");
}
