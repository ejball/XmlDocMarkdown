using System.Collections.Generic;

namespace XmlDocMarkdown.Core
{
	/// <summary>
	/// Settings for markdown generation.
	/// </summary>
	public class XmlDocMarkdownSettings
	{
		/// <summary>
		/// The URL of the folder containing the source code of the assembly, e.g. at GitHub.
		/// </summary>
		/// <remarks>The URL may be absolute or relative. Required to generate source code
		/// links in the See Also sections for types.</remarks>
		public string SourceCodePath { get; set; }

		/// <summary>
		/// The root namespace of the input assembly.
		/// </summary>
		/// <remarks>Used to generate source code links in the See Also sections for types.
		/// If omitted, the tool guesses the root namespace from the exported types.</remarks>
		public string RootNamespace { get; set; }

		/// <summary>
		/// If true, generates documentation for obsolete types and members. (Default false.)
		/// </summary>
		public bool IncludeObsolete { get; set; }

		/// <summary>
		/// If true, skips documentation for types that have System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never).
		/// </summary>
		public bool SkipUnbrowsable { get; set; }

		/// <summary>
		/// The minimum visibility for documented types and members.
		/// </summary>
		/// <remarks>Defaults to <c>Protected</c>.</remarks>
		public XmlDocVisibilityLevel? VisibilityLevel { get; set; }

		/// <summary>
		/// Indicates the newline used in the output.
		/// </summary>
		/// <remarks>Defaults to <c>"\r\n"</c> or <c>"\n"</c>, depending on the platform.</remarks>
		public string NewLine { get; set; }

		/// <summary>
		/// If true, deletes previously generated files that are no longer used.
		/// </summary>
		public bool ShouldClean { get; set; }

		/// <summary>
		/// If true, generates a .yml file that can be used in a Jekyll based site.
		/// </summary>
		public bool GenerateToc { get; set; }

		/// <summary>
		/// Generate separate pages for each namespace containing list of types in each.
		/// </summary>
		public bool NamespacePages { get; set; }

		/// <summary>
		/// A path prefix to add to all links in the table of contents .yml file.
		/// </summary>
		public string TocPrefix { get; set; }

		/// <summary>
		/// If true, suppresses normal console output.
		/// </summary>
		public bool IsQuiet { get; set; }

		/// <summary>
		/// If true, executes without making changes to the file system.
		/// </summary>
		public bool IsDryRun { get; set; }

		/// <summary>
		/// If non-null, contains the path to a file that contains the Jekyll front matter template.
		/// </summary>
		public string FrontMatter { get; set; }

		/// <summary>
		/// Specify permalink style, 'none' or 'pretty' (default 'none').
		/// 'pretty' permalinks do not contain file extensions, and when you select this option
		/// periods have to be removed from file names, for example, 'System.Console' would have to be 'SystemConsole'.
		/// since the removal of the '.md' extension would make Jekyll think '.Console' is a file extension which doesn't work.
		/// </summary>
		public string PermalinkStyle { get; set; }

		/// <summary>
		/// Configures external documentation.
		/// </summary>
		public IReadOnlyList<ExternalDocumentation> ExternalDocs { get; set; }
	}
}
