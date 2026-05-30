using System.Reflection;
using NUnit.Framework;
using XmlDocGen.Core;
using XmlDocGen.Core.CSharp;
using XmlDocGen.Core.IO;
using XmlDocGen.Core.Markdown;
using XmlDocGen.Core.Nodes;
using XmlDocGen.Core.Pages;
using XmlDocGen.Core.Sites;
using XmlDocGen.Core.Xml;
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

	[Test]
	public void CustomVisibilityCanSubclassBaseClass()
	{
		var tree = CreateExampleTree();
		var visibility = new NoMemberVisibility();

		Assert.That(tree.DescendantsAndSelf(visibility), Has.Some.InstanceOf<XmlDocTypeNode>());
		Assert.That(tree.DescendantsAndSelf(visibility), Has.None.InstanceOf<XmlDocMemberNode>());
	}

	[Test]
	public void PerTypePageMapFoldsMembersOntoTypePage()
	{
		var tree = CreateExampleTree();
		var pages = XmlDocPageBuilder.CreatePages(tree, XmlDocNodeVisibility.Public, XmlDocPageMap.PerType);
		var typeNode = (XmlDocTypeNode) tree.FindNode(XmlDocRef.ForType(typeof(ExampleClass)))!;
		var propertyNode = (XmlDocMemberNode) tree.FindNode(XmlDocRef.ForMember(typeof(ExampleClass).GetProperty(nameof(ExampleClass.Id))!))!;

		var page = pages.Single(x => x.Nodes.Contains(typeNode));

		Assert.That(page.Nodes, Does.Contain(propertyNode));
	}

	[Test]
	public void ExternalResolversCanBePatternedAndCombined()
	{
		var resolver = XmlDocExternalLinkResolver.Combine(
			XmlDocExternalLinkResolver.UrlPattern("https://example.test/{name}"));

		Assert.That(resolver.TryGetUrl(new XmlDocRef("T:Example.Widget"), null), Is.EqualTo("https://example.test/Example.Widget"));
	}

	[Test]
	public void HelpIncludesCustomHostLines()
	{
		using var output = new StringWriter();
		var oldOutput = Console.Out;
		try
		{
			Console.SetOut(output);

			var exitCode = XmlDocGenApp.Run(["--help"], ctx => ctx.HelpLines.Add("  --sample   Custom sample option."));

			Assert.That(exitCode, Is.Zero);
			Assert.That(output.ToString(), Does.Contain("--sample"));
		}
		finally
		{
			Console.SetOut(oldOutput);
		}
	}

	[Test]
	public void CleanUsesManifestWithoutDeletingHandAuthoredFiles()
	{
		var outputPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
		try
		{
			var writer = new XmlDocSiteWriter(new XmlDocSiteWriterSettings { ShouldClean = true });
			writer.Write(new XmlDocSite([new XmlDocSiteFile("generated.md", "one")]), outputPath);
			File.WriteAllText(Path.Combine(outputPath, "hand.md"), "hand");

			writer.Write(new XmlDocSite([]), outputPath);

			Assert.That(File.Exists(Path.Combine(outputPath, "generated.md")), Is.False);
			Assert.That(File.Exists(Path.Combine(outputPath, "hand.md")), Is.True);
		}
		finally
		{
			if (Directory.Exists(outputPath))
				Directory.Delete(outputPath, recursive: true);
		}
	}

	[Test]
	public void FullSignaturesRenderConstraintsDefaultsAndBases()
	{
		var tree = CreateExampleTree();
		var typeNode = tree.FindNode(XmlDocRef.ForType(typeof(ExampleClass)))!;
		var typeSignature = CSharpSignatureBuilder.Full.GetSignature(typeNode).Text;

		Assert.That(typeSignature, Does.Contain("public class ExampleClass : IExampleContravariantInterface<ExampleClass>"));
		Assert.That(typeSignature, Does.Not.Contain("public  class"));

		var overloaded = typeof(ExampleClass).GetMethods().Single(x => x.Name == nameof(ExampleClass.Overloaded) && x.GetGenericArguments().Length == 2);
		var overloadedSignature = CSharpSignatureBuilder.Full.GetSignature(tree.FindNode(XmlDocRef.ForMember(overloaded))!).Text;

		Assert.That(overloadedSignature, Does.Contain("where T : class"));
		Assert.That(overloadedSignature, Does.Contain("where U : struct"));

		var defaultParameters = typeof(ExampleClass).GetMethod(nameof(ExampleClass.DefaultParameters))!;
		var defaultSignature = CSharpSignatureBuilder.Full.GetSignature(tree.FindNode(XmlDocRef.ForMember(defaultParameters))!).Text;

		Assert.That(defaultSignature, Does.Contain("bool @bool = true"));
		Assert.That(defaultSignature, Does.Contain("double @double = double.NaN"));
		Assert.That(defaultSignature, Does.Contain("ExampleFlagsEnum flags = ExampleFlagsEnum.Second | ExampleFlagsEnum.Third"));
	}

	[Test]
	public void MarkdownRendersPropertyValuesAndOverviewTables()
	{
		var site = new MarkdownSiteBuilder(new XmlDocSiteBuilderSettings { PageMap = XmlDocPageMap.PerMember }).Build(CreateExampleTree());

		var typeFile = site.Files.Single(x => x.Path == "ExampleAssembly/ExampleAssembly/ExampleClass.md");
		var propertyFile = site.Files.Single(x => x.Path == "ExampleAssembly/ExampleAssembly/ExampleClass/Id.md");

		Assert.That(typeFile.Text, Does.Contain("## Members"));
		Assert.That(typeFile.Text, Does.Contain("[Id](./ExampleClass/Id.md)"));
		Assert.That(propertyFile.Text, Does.Contain("## Property Value"));
		Assert.That(propertyFile.Text, Does.Contain("The ID."));
	}

	[Test]
	public void XmlFileLoadExpandsIncludes()
	{
		var directory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
		Directory.CreateDirectory(directory);
		try
		{
			File.WriteAllText(Path.Combine(directory, "include.xml"), "<docs><summary>Included summary.</summary></docs>");
			File.WriteAllText(Path.Combine(directory, "main.xml"), "<doc><members><member name=\"T:Example.Widget\"><include file=\"include.xml\" path=\"/docs/summary\" /></member></members></doc>");

			var file = XmlDocXmlFile.Load(Path.Combine(directory, "main.xml"));

			Assert.That(file.FindMember(new XmlDocRef("T:Example.Widget"))?.Summary.Single().Inlines.Single().Text, Is.EqualTo("Included summary."));
		}
		finally
		{
			Directory.Delete(directory, recursive: true);
		}
	}

	[Test]
	public void SourceLinksCanProbeAssemblyWithoutThrowing()
	{
		Assert.DoesNotThrow(() => XmlDocSourceLinks.TryCreate(typeof(ExampleClass).GetTypeInfo().Assembly));
	}

	private static XmlDocTree CreateExampleTree()
	{
		var assembly = typeof(ExampleClass).GetTypeInfo().Assembly;
		return XmlDocTree.Create([(assembly, XmlDocXmlFile.Load(Path.ChangeExtension(assembly.Location, ".xml")))]);
	}

	private sealed class NoMemberVisibility : XmlDocNodeVisibility
	{
		public override bool IsVisible(XmlDocNode node) => XmlDocNodeVisibility.Public.IsVisible(node) && node is not XmlDocMemberNode;
	}
}
