using System.IO;
using System.Xml.Linq;
using ExampleAssembly;
using NUnit.Framework;
using XmlDocMarkdown.Core;

namespace XmlDocMarkdown.Tests
{
	[TestFixture]
	public class MarkdownGeneratorTests
	{
		[Test]
		public void ExampleAssembly()
		{
			var assembly = typeof(ExampleClass).Assembly;
			var xDocument = XDocument.Load(Path.ChangeExtension(assembly.Location, ".xml"));
			var xmlDocAssembly = new XmlDocAssembly(xDocument);
			new MarkdownGenerator().GenerateOutput(assembly, xmlDocAssembly);
		}
	}
}
