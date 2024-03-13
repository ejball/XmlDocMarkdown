using System.Reflection;
using ExampleAssembly;
using NUnit.Framework;
using XmlDocMarkdown.Core;

namespace XmlDocMarkdown.Tests
{
	public class MarkdownGeneratorTests
	{
		[Test]
		public void ExampleAssembly()
		{
			XmlDocMarkdownGenerator.Generate(
				typeof(ExampleClass).GetTypeInfo().Assembly.Location,
				Path.Combine(Path.GetTempPath(), "MarkdownGeneratorTests"),
				new XmlDocMarkdownSettings { IsDryRun = true });
		}
	}
}
