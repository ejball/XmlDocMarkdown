using NUnit.Framework;
using XmlDocGen.Core.Markdown;
using XmlDocGen.Core.Pages;
using XmlDocGen.Core.Sites;

namespace XmlDocGen.Tests.Markdown;

internal sealed class MarkdownLayerTests
{
	[Test]
	public void MarkdownRendersPropertyValuesAndOverviewTables()
	{
		var site = new MarkdownSiteBuilder(new XmlDocSiteBuilderSettings { PageMap = XmlDocPageMap.PerMember }).Build(TestSupport.CreateExampleTree());

		Assert.That(site.FindFile("ExampleAssembly/ExampleAssembly/ExampleClass.md")?.Text, Does.Contain("## Members"));
		Assert.That(site.FindFile("ExampleAssembly/ExampleAssembly/ExampleClass.md")?.Text, Does.Contain("[Id](./ExampleClass/Id.md)"));
		Assert.That(site.FindFile("ExampleAssembly/ExampleAssembly/ExampleClass/Id.md")?.Text, Does.Contain("## Property Value"));
		Assert.That(site.FindFile("ExampleAssembly/ExampleAssembly/ExampleClass/Id.md")?.Text, Does.Contain("The ID."));
	}

	[Test]
	public void MarkdownRendersExternalLinksParamRefsAndSeeAlso()
	{
		var site = new MarkdownSiteBuilder(new XmlDocSiteBuilderSettings { PageMap = XmlDocPageMap.PerMember }).Build(TestSupport.CreateExampleTree());

		Assert.That(site.FindFile("ExampleAssembly/ExampleAssembly/ExampleClass/HasHyperlinks.md")?.Text, Does.Contain("[more info](https://ejball.com/)"));
		Assert.That(site.FindFile("ExampleAssembly/ExampleAssembly/ExampleClass/ParameterReference.md")?.Text, Does.Contain("*value*"));
		Assert.That(site.FindFile("ExampleAssembly/ExampleAssembly/ExampleDerivedClass/SeeAlso.md")?.Text, Does.Contain("## See Also"));
	}

	[Test]
	public void PageRendererCanWriteFrontMatter()
	{
		var site = new MarkdownSiteBuilder(renderer: new FrontMatterRenderer()).Build(TestSupport.CreateExampleTree());

		Assert.That(site.FindFile("ExampleAssembly/ExampleAssembly/ExampleClass.md")?.Text.ReplaceLineEndings("\n"), Does.StartWith("---\ntitle: ExampleClass\n---\n"));
	}

	private sealed class FrontMatterRenderer : MarkdownPageRenderer
	{
		protected override void WriteFrontMatter(MarkdownWriter writer, XmlDocPage page)
		{
			writer.WriteLine("---");
			writer.WriteLine("title: " + page.Nodes[0].Name);
			writer.WriteLine("---");
		}
	}
}
