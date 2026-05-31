namespace XmlDocGen.Core.IO;

/// <summary>Settings for writing a generated site.</summary>
public sealed class XmlDocSiteWriterSettings
{
	/// <summary>Gets or sets a value indicating whether stale generated files are deleted.</summary>
	public bool ShouldClean { get; set; }

	/// <summary>Gets or sets a value indicating whether no file-system changes should be made.</summary>
	public bool IsDryRun { get; set; }

	/// <summary>Gets or sets a value indicating whether normal messages are suppressed.</summary>
	public bool IsQuiet { get; set; }

	/// <summary>Gets or sets the file system abstraction.</summary>
	public IXmlDocFileSystem? FileSystem { get; set; }

	/// <summary>Gets or sets newline comparison behavior.</summary>
	public XmlDocNewLineComparison NewLineComparison { get; set; }
}
