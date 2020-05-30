using System.IO;
using System.Reflection;
using ExampleAssembly;
using XmlDocMarkdown.Core;
using Xunit;

namespace XmlDocMarkdown.Tests
{
	public class MarkdownGeneratorTests
	{
		[Fact]
		public void ExampleAssembly()
		{
			XmlDocMarkdownGenerator.Generate(
				typeof(ExampleClass).GetTypeInfo().Assembly.Location,
				Path.Combine(Path.GetTempPath(), "MarkdownGeneratorTests"),
				new XmlDocMarkdownSettings { IsDryRun = true });
		}

		[Fact]
		public void FSharpWithNulls()
		{
			XmlDocMarkdownGenerator.Generate(
				typeof(ExtensionMethods.Augment).GetTypeInfo().Assembly.Location,
				Path.Combine(Path.GetTempPath(), "MarkdownGeneratorTests"),
				new XmlDocMarkdownSettings { IsDryRun = true });
		}
	}
}
