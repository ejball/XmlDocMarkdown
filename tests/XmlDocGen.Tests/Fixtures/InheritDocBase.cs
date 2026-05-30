namespace XmlDocGen.Tests.Fixtures;

/// <summary>A documented base class.</summary>
internal class InheritDocBase
{
	/// <summary>Inherited base summary.</summary>
	/// <returns>The inherited value.</returns>
	public virtual string BaseMethod() => "base";

	/// <summary>Path-filtered summary.</summary>
	/// <remarks>Path-filtered remarks.</remarks>
	public virtual void PathMethod()
	{
	}
}
