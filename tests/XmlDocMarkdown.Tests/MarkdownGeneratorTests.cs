using System.IO;
using System.Reflection;
using System.Xml.Linq;
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
			var assembly = typeof(ExampleClass).GetTypeInfo().Assembly;
			var xDocument = XDocument.Load(Path.ChangeExtension(assembly.Location, ".xml"));
			var xmlDocAssembly = new XmlDocAssembly(xDocument);
			new MarkdownGenerator().GenerateOutput(assembly, xmlDocAssembly);
		}
	}
}
