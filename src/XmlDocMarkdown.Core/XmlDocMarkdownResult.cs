using System.Collections.ObjectModel;

namespace XmlDocMarkdown.Core;

/// <summary>
/// The names of files that were added, changed, or removed.
/// </summary>
public sealed class XmlDocMarkdownResult
{
	/// <summary>
	/// The names of files that were added.
	/// </summary>
	public Collection<string> Added { get; } = [];

	/// <summary>
	/// The names of files that were changed.
	/// </summary>
	public Collection<string> Changed { get; } = [];

	/// <summary>
	/// The names of files that were removed.
	/// </summary>
	public Collection<string> Removed { get; } = [];

	/// <summary>
	/// Messages that should be displayed (unless quiet).
	/// </summary>
	public Collection<string> Messages { get; } = [];
}
