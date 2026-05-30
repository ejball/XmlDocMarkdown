using System.Reflection;
using NUnit.Framework;
using XmlDocGen.Core;
using ExampleClass = ExampleAssembly.ExampleClass;

namespace XmlDocMarkdown.Tests;

internal sealed class MarkdownGeneratorTests
{
	[Test]
	public void ExampleAssembly()
	{
		var exitCode = XmlDocGenApp.Run(
			[
				typeof(ExampleClass).GetTypeInfo().Assembly.GetName().Name!,
				Path.Combine(Path.GetTempPath(), "MarkdownGeneratorTests"),
				"--dryrun",
			]);

		Assert.That(exitCode, Is.Zero);
	}
}
