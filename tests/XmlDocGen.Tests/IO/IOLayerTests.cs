using NUnit.Framework;
using XmlDocGen.Core.IO;
using XmlDocGen.Core.Sites;

namespace XmlDocGen.Tests.IO;

internal sealed class IOLayerTests
{
	[Test]
	public void WriterCleansOnlyManifestFiles()
	{
		var outputPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
		try
		{
			var writer = new XmlDocSiteWriter(new XmlDocSiteWriterSettings { ShouldClean = true });
			writer.Write(new XmlDocSite([new XmlDocSiteFile("generated.md", "one")]), outputPath);
			File.WriteAllText(Path.Combine(outputPath, "hand.md"), "hand");

			var result = writer.Write(new XmlDocSite([]), outputPath);

			Assert.That(result.Removed, Is.EqualTo(new[] { "generated.md" }));
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
	public void DryRunReportsChangesWithoutWriting()
	{
		var outputPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
		var writer = new XmlDocSiteWriter(new XmlDocSiteWriterSettings { IsDryRun = true });

		var result = writer.Write(new XmlDocSite([new XmlDocSiteFile("file.md", "text")]), outputPath);

		Assert.That(result.Added, Is.EqualTo(new[] { "file.md" }));
		Assert.That(Directory.Exists(outputPath), Is.False);
	}

	[Test]
	public void QuietSuppressesMessagesButKeepsResultLists()
	{
		var outputPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
		var writer = new XmlDocSiteWriter(new XmlDocSiteWriterSettings { IsDryRun = true, IsQuiet = true });

		var result = writer.Write(new XmlDocSite([new XmlDocSiteFile("file.md", "text")]), outputPath);

		Assert.That(result.Added, Is.Not.Empty);
		Assert.That(result.Messages, Is.Empty);
	}
}
