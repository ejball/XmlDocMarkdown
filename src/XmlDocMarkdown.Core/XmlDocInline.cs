using System.Reflection;

namespace XmlDocMarkdown.Core
{
	public sealed class XmlDocInline
	{
		public string Text { get; set; }

		public MemberInfo See { get; set; }

		public bool IsCode { get; set; }

		public bool IsParamRef { get; set; }

		public bool IsTypeParamRef { get; set; }
	}
}
