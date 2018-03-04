namespace XmlDocMarkdown.Core
{
	public class XmlDocMarkdownSettings
	{
		public string SourceCodePath { get; set; }

		public string RootNamespace { get; set; }

		public bool IncludeObsolete { get; set; }

		public VisibilityLevel? VisibilityLevel { get; set; }

		public string NewLine { get; set; }

		public bool ShouldClean { get; set; }

		public bool IsQuiet { get; set; }

		public bool IsDryRun { get; set; }
	}
}
