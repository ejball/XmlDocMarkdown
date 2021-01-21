using System.Collections.ObjectModel;

namespace XmlDocMarkdown.Core
{
	internal sealed class XmlDocParameter
	{
		public string? Name { get; set; }

		public Collection<XmlDocBlock> Description { get; } = new();
	}
}
