using NUnit.Framework;
using XmlDocGen.Core.Markdown;
using XmlDocGen.Core.Pages;
using XmlDocGen.Core.Sites;

namespace XmlDocGen.Tests.Sites;

internal sealed class SitesLayerTests
{
	[Test]
	public void SiteBuilderUsesSettingsAndFindsFiles()
	{
		var site = new XmlDocSiteBuilder(new MarkdownPageRenderer(), new XmlDocSiteBuilderSettings { PageMap = XmlDocPageMap.SinglePage, NewLine = "\n" }).Build(TestSupport.CreateExampleTree());

		Assert.That(site.Files, Has.Count.EqualTo(1));
		Assert.That(site.FindFile("index.md"), Is.Not.Null);
		Assert.That(site.FindFile("index.md")!.Text, Does.Not.Contain("\r\n"));
	}

	[Test]
	public void SiteFilesAreOrderedCaseInsensitively()
	{
		var site = new XmlDocSite([new XmlDocSiteFile("b.md", ""), new XmlDocSiteFile("A.md", "")]);

		Assert.That(site.Files.Select(x => x.Path), Is.EqualTo(new[] { "A.md", "b.md" }));
	}
}
