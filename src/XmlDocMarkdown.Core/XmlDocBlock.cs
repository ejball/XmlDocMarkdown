using System.Collections.ObjectModel;

namespace XmlDocMarkdown.Core
{
	internal sealed class XmlDocBlock
	{
		public Collection<XmlDocInline> Inlines { get; } = new();

		public bool IsCode { get; set; }

		public string CodeLanguage { get; set; } = "csharp";

		public XmlDocListKind? ListKind { get; set; }

		public int ListDepth { get; set; }

		public bool IsListHeader { get; set; }

		public bool IsListTerm { get; set; }
	}
}
