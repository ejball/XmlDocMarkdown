using System.Collections.ObjectModel;

namespace XmlDocMarkdown.Core
{
	public sealed class XmlDocMarkdownResult
	{
		public Collection<string> Added { get; } = new Collection<string>();

		public Collection<string> Changed { get; } = new Collection<string>();

		public Collection<string> Removed { get; } = new Collection<string>();

		public Collection<string> Messages { get; } = new Collection<string>();
	}
}
