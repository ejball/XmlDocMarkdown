using System.Reflection;
using ExampleAssembly;
using NUnit.Framework;
using XmlDocGen.Core.Nodes;
using XmlDocGen.Core.Pages;
using XmlDocGen.Core.Xml;

namespace XmlDocGen.Tests.Pages;

internal sealed class PagesLayerTests
{
	[Test]
	public void PerTypePageMapFoldsMembersOntoTypePage()
	{
		var tree = TestSupport.CreateExampleTree();
		var pages = XmlDocPageBuilder.CreatePages(tree, XmlDocNodeVisibility.Public, XmlDocPageMap.PerType);
		var typeNode = (XmlDocTypeNode) tree.FindNode(XmlDocRef.ForType(typeof(ExampleClass)))!;
		var propertyNode = (XmlDocMemberNode) tree.FindNode(XmlDocRef.ForMember(typeof(ExampleClass).GetProperty(nameof(ExampleClass.Id))!))!;

		Assert.That(pages.Single(x => x.Nodes.Contains(typeNode)).Nodes, Does.Contain(propertyNode));
	}

	[Test]
	public void PerMemberPageMapDisambiguatesOverloads()
	{
		var tree = TestSupport.CreateExampleTree();
		var pages = XmlDocPageBuilder.CreatePages(tree, XmlDocNodeVisibility.Public, XmlDocPageMap.PerMember);

		Assert.That(pages.Select(x => x.Path), Does.Not.Contain("ExampleAssembly/ExampleAssembly"));
		Assert.That(pages.Select(x => x.Path), Does.Contain("ExampleAssembly/ExampleClass/Create"));
		Assert.That(pages.Single(x => x.Path == "ExampleAssembly/ExampleClass/Create").Nodes, Has.Count.EqualTo(2));
	}

	[Test]
	public void UrlMappersAppendFragmentsOnlyForFoldedNodes()
	{
		var tree = TestSupport.CreateExampleTree();
		var pages = XmlDocPageBuilder.CreatePages(tree, XmlDocNodeVisibility.Public, XmlDocPageMap.PerType);
		var currentPage = pages.Single(x => x.Nodes[0].Name == "ExampleClass");
		var createMethod = typeof(ExampleClass).GetMethod(nameof(ExampleClass.Create), [typeof(string)])!;
		var context = new XmlDocPageContext(tree, pages, currentPage, XmlDocUrlMapper.GitHub, XmlDocExternalLinkResolver.DotNetApi, null);

		Assert.That(context.GetLinkUrl(createMethod), Is.EqualTo("./ExampleClass.md#create-string"));
	}

	[Test]
	public void ExternalResolversCanBePatternedAndCombined()
	{
		var resolver = XmlDocExternalLinkResolver.Combine(XmlDocExternalLinkResolver.UrlPattern("https://example.test/{name}"));

		Assert.That(resolver.TryGetUrl(new XmlDocRef("T:Example.Widget"), null), Is.EqualTo("https://example.test/Example.Widget"));
	}

	[Test]
	public void SourceLinksCanProbeAssemblyWithoutThrowing()
	{
		Assert.DoesNotThrow(() => XmlDocSourceLinks.TryCreate(typeof(ExampleClass).GetTypeInfo().Assembly));
	}
}
