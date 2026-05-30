namespace XmlDocGen.Tests.Fixtures;

/// <inheritdoc />
internal sealed class InheritDocDerived : InheritDocBase, IInheritDocFixture
{
	/// <inheritdoc cref="InheritDocBase.BaseMethod" />
	public override string BaseMethod() => "derived";

	/// <inheritdoc />
	public void InterfaceMethod(string value)
	{
	}
}
