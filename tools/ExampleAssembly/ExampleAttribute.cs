using System;

namespace ExampleAssembly
{
	/// <summary>
	/// An attribute.
	/// </summary>
	[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
	public sealed class ExampleAttribute : Attribute
	{
	}
}
