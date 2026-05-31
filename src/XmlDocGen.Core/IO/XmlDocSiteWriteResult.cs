namespace XmlDocGen.Core.IO;

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
