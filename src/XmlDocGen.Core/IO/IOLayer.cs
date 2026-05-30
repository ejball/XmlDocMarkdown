using XmlDocGen.Core.Sites;

namespace XmlDocGen.Core.IO;

/// <summary>Writes generated documentation sites to disk.</summary>
public sealed class XmlDocSiteWriter
{
	/// <summary>Initializes a new instance of the <see cref="XmlDocSiteWriter"/> class.</summary>
	public XmlDocSiteWriter(XmlDocSiteWriterSettings? settings = null)
	{
		Settings = settings ?? new XmlDocSiteWriterSettings();
	}

	/// <summary>Gets the writer settings.</summary>
	public XmlDocSiteWriterSettings Settings { get; }

	/// <summary>Writes the site to the output directory.</summary>
	public XmlDocSiteWriteResult Write(XmlDocSite site, string outputPath)
	{
		ArgumentNullException.ThrowIfNull(site);
		ArgumentException.ThrowIfNullOrEmpty(outputPath);

		var fileSystem = Settings.FileSystem ?? new RealXmlDocFileSystem();
		var result = new XmlDocSiteWriteResult();
		var manifestPath = Path.Combine(outputPath, ".xmldocgen-manifest");
		var previous = Settings.ShouldClean && fileSystem.FileExists(manifestPath) ? fileSystem.ReadAllText(manifestPath).Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries).ToHashSet(StringComparer.OrdinalIgnoreCase) : [];
		var current = site.Files.Select(x => x.Path).ToHashSet(StringComparer.OrdinalIgnoreCase);

		foreach (var file in site.Files)
		{
			var path = Path.Combine(outputPath, file.Path.Replace('/', Path.DirectorySeparatorChar));
			if (fileSystem.FileExists(path))
			{
				var oldText = fileSystem.ReadAllText(path);
				if (!SameText(oldText, file.Text))
				{
					result.AddChanged(file.Path);
					result.AddMessage("changed " + file.Path);
					if (!Settings.IsDryRun)
						fileSystem.WriteAllText(path, file.Text);
				}
			}
			else
			{
				result.AddAdded(file.Path);
				result.AddMessage("added " + file.Path);
				if (!Settings.IsDryRun)
					fileSystem.WriteAllText(path, file.Text);
			}
		}

		if (Settings.ShouldClean)
		{
			foreach (var removed in previous.Except(current, StringComparer.OrdinalIgnoreCase))
			{
				result.AddRemoved(removed);
				result.AddMessage("removed " + removed);
				if (!Settings.IsDryRun)
					fileSystem.DeleteFile(Path.Combine(outputPath, removed.Replace('/', Path.DirectorySeparatorChar)));
			}
			if (!Settings.IsDryRun)
				fileSystem.WriteAllText(manifestPath, string.Join(Environment.NewLine, current.Order(StringComparer.OrdinalIgnoreCase)));
		}

		if (Settings.IsQuiet)
			result.ClearMessages();

		return result;
	}

	private bool SameText(string oldText, string newText) => Settings.NewLineComparison == XmlDocNewLineComparison.Exact ? oldText == newText : oldText.ReplaceLineEndings("\n") == newText.ReplaceLineEndings("\n");
}

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

/// <summary>Controls newline comparison when diffing files.</summary>
public enum XmlDocNewLineComparison
{
	/// <summary>Normalize line endings before comparing.</summary>
	Ignore,
	/// <summary>Compare line endings exactly.</summary>
	Exact,
}

/// <summary>Abstracts file-system operations.</summary>
public interface IXmlDocFileSystem
{
	/// <summary>Returns true if the file exists.</summary>
	bool FileExists(string path);

	/// <summary>Reads all text from a file.</summary>
	string ReadAllText(string path);

	/// <summary>Writes all text to a file.</summary>
	void WriteAllText(string path, string text);

	/// <summary>Deletes a file.</summary>
	void DeleteFile(string path);

	/// <summary>Enumerates files under a directory.</summary>
	IEnumerable<string> EnumerateFiles(string directory);
}

/// <summary>The result of writing a generated site.</summary>
public sealed class XmlDocSiteWriteResult
{
	/// <summary>Gets added files.</summary>
	public IReadOnlyList<string> Added => m_added;

	/// <summary>Gets changed files.</summary>
	public IReadOnlyList<string> Changed => m_changed;

	/// <summary>Gets removed files.</summary>
	public IReadOnlyList<string> Removed => m_removed;

	/// <summary>Gets informational messages.</summary>
	public IReadOnlyList<string> Messages => m_messages;

	/// <summary>Gets a value indicating whether the write would change files.</summary>
	public bool HasChanges => Added.Count + Changed.Count + Removed.Count != 0;

	internal void AddAdded(string path) => m_added.Add(path);

	internal void AddChanged(string path) => m_changed.Add(path);

	internal void AddRemoved(string path) => m_removed.Add(path);

	internal void AddMessage(string message) => m_messages.Add(message);

	internal void ClearMessages() => m_messages.Clear();

	private readonly List<string> m_added = [];
	private readonly List<string> m_changed = [];
	private readonly List<string> m_removed = [];
	private readonly List<string> m_messages = [];
}

internal sealed class RealXmlDocFileSystem : IXmlDocFileSystem
{
	public bool FileExists(string path) => File.Exists(path);

	public string ReadAllText(string path) => File.ReadAllText(path);

	public void WriteAllText(string path, string text)
	{
		var directory = Path.GetDirectoryName(path);
		if (!string.IsNullOrEmpty(directory))
			Directory.CreateDirectory(directory);
		File.WriteAllText(path, text);
	}

	public void DeleteFile(string path)
	{
		if (File.Exists(path))
			File.Delete(path);
	}

	public IEnumerable<string> EnumerateFiles(string directory) => Directory.Exists(directory) ? Directory.EnumerateFiles(directory, "*", SearchOption.AllDirectories) : [];
}
