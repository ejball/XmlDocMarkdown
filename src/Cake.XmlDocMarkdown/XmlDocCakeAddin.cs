using Cake.Core;
using Cake.Core.Annotations;
using Cake.Core.IO;
using XmlDocMarkdown.Core;
using LogLevel = Cake.Core.Diagnostics.LogLevel;
using Verbosity = Cake.Core.Diagnostics.Verbosity;

namespace Cake.XmlDocMarkdown
{
	/// <summary>
	/// The Cake addin.
	/// </summary>
	[CakeAliasCategory("XmlDocMarkdown")]
	[CakeNamespaceImport("XmlDocMarkdown.Core")]
	public static class XmlDocCakeAddin
	{
		/// <summary>
		/// Generates Markdown from .NET XML documentation comments.
		/// </summary>
		/// <param name="context">The Cake context.</param>
		/// <param name="inputPath">The input assembly.</param>
		/// <param name="outputPath">The output directory.</param>
		/// <param name="settings">The settings.</param>
		/// <returns>The names of files that were added, changed, or removed.</returns>
		[CakeMethodAlias]
		public static XmlDocMarkdownResult XmlDocMarkdownGenerate(this ICakeContext context, FilePath inputPath, DirectoryPath outputPath, XmlDocMarkdownSettings settings = null)
			=> context.XmlDocMarkdownGenerate(inputPath.FullPath, outputPath.FullPath, settings);

		/// <summary>
		/// Generates Markdown from .NET XML documentation comments.
		/// </summary>
		/// <param name="context">The Cake context.</param>
		/// <param name="inputPath">The input assembly.</param>
		/// <param name="outputPath">The output directory.</param>
		/// <param name="settings">The settings.</param>
		/// <returns>The names of files that were added, changed, or removed.</returns>
		[CakeMethodAlias]
		public static XmlDocMarkdownResult XmlDocMarkdownGenerate(this ICakeContext context, string inputPath, string outputPath, XmlDocMarkdownSettings settings = null)
		{
			var result = XmlDocMarkdownGenerator.Generate(inputPath, outputPath, settings);
			foreach (string message in result.Messages)
				context?.Log.Write(Verbosity.Normal, LogLevel.Information, message);
			return result;
		}
	}
}
