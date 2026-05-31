namespace XmlDocGen.Tests.Fixtures;

/// <inheritdoc cref="InheritDocBase" />
internal sealed class InheritDocDerived : InheritDocBase, IInheritDocFixture
{
	/// <inheritdoc cref="InheritDocBase.BaseMethod" />
	public override string BaseMethod() => "derived";

	/// <inheritdoc />
	public void InterfaceMethod(string value)
	{
	}

	/// <inheritdoc cref="InheritDocBase.PathMethod" path="/remarks" />
	public override void PathMethod()
	{
	}
}
