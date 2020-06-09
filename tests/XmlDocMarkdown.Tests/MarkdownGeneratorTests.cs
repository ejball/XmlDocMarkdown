using System.Collections.Concurrent;
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

		[Fact]
		public void MatchNugetDependencies()
		{
			var generatorAssembly = typeof(XmlDocMarkdownGenerator).Assembly;
			var generatorType = generatorAssembly.GetType("XmlDocMarkdown.Core.MarkdownGenerator");
			var table = generatorType.GetField("resolutionTable",
				BindingFlags.Static | BindingFlags.NonPublic).GetValue(null)
				as ConcurrentDictionary<string, Assembly>;
			table.Clear();

			XmlDocMarkdownGenerator.Generate(
				typeof(Cake.XmlDocMarkdown.XmlDocCakeAddin).GetTypeInfo().Assembly.Location,
				Path.Combine(Path.GetTempPath(), "MarkdownGeneratorTests"),
				new XmlDocMarkdownSettings { IsDryRun = true });

			Assert.True(table.Count > 0, "Was empty");
		}
	}
}
