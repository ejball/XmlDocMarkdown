namespace XmlDocGen.Core.Sites;

/// <summary>A generated documentation site.</summary>
public sealed class XmlDocSite
{
	/// <summary>Initializes a new instance of the <see cref="XmlDocSite"/> class.</summary>
	public XmlDocSite(IEnumerable<XmlDocSiteFile> files)
	{
		Files = [.. files.OrderBy(x => x.Path, StringComparer.OrdinalIgnoreCase)];
		m_filesByPath = Files.ToDictionary(x => x.Path, StringComparer.OrdinalIgnoreCase);
	}

	/// <summary>Gets generated files in deterministic order.</summary>
	public IReadOnlyList<XmlDocSiteFile> Files { get; }

	/// <summary>Finds a generated file by path.</summary>
	public XmlDocSiteFile? FindFile(string path) => m_filesByPath.GetValueOrDefault(path);

	private readonly IReadOnlyDictionary<string, XmlDocSiteFile> m_filesByPath;
}
