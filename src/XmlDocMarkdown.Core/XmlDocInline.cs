namespace XmlDocMarkdown.Core
{
	internal sealed class XmlDocInline
	{
		public string? Text { get; set; }

		public string? SeeRef { get; set; }

		public string? LangWord { get; set; }

		public string? LinkUrl { get; set; }

		public bool IsCode { get; set; }

		public bool IsParamRef { get; set; }

		public bool IsTypeParamRef { get; set; }
	}
}
