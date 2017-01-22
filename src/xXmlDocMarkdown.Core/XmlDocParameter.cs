using System.Collections.ObjectModel;

namespace XmlDocMarkdown.Core
{
	public sealed class XmlDocParameter
	{
		public string Name { get; set; }

		public Collection<XmlDocBlock> Description { get; } = new Collection<XmlDocBlock>();
	}
}
