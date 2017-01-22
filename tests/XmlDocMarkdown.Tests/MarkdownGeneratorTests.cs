using ExampleAssembly;
using NUnit.Framework;
using XmlDocMarkdown.Core;
using XmlDocMarkdown.NuDoqCore;

namespace XmlDocMarkdown.Tests
{
	[TestFixture]
	public class MarkdownGeneratorTests
	{
		[Test]
		public void ExampleAssembly()
		{
			var assembly = typeof(ExampleClass).Assembly;
			var xmlDocAssembly = NuDoqXmlDocUtility.CreateXmlDocAssembly(assembly);
			new MarkdownGenerator().GenerateOutput(xmlDocAssembly);
		}
	}
}
