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
