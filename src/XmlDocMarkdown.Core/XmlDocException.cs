using System.Collections.ObjectModel;

namespace XmlDocMarkdown.Core
{
	public sealed class XmlDocException
	{
		public string ExceptionTypeRef { get; set; }

		public Collection<XmlDocBlock> Condition { get; } = new Collection<XmlDocBlock>();
	}
}
