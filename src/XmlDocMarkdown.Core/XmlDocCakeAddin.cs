using Cake.Core;
using Cake.Core.Annotations;
using Cake.Core.IO;
using LogLevel = Cake.Core.Diagnostics.LogLevel;
using Verbosity = Cake.Core.Diagnostics.Verbosity;

namespace XmlDocMarkdown.Core
{
	public static class XmlDocCakeAddin
	{
		[CakeMethodAlias]
		public static XmlDocGeneratorResult XmlDocMarkdownGenerate(this ICakeContext context, FilePath inputPath, DirectoryPath outputPath, XmlDocMarkdownSettings settings = null)
			=> context.XmlDocMarkdownGenerate(inputPath.FullPath, outputPath.FullPath, settings);

		[CakeMethodAlias]
		public static XmlDocGeneratorResult XmlDocMarkdownGenerate(this ICakeContext context, string inputPath, string outputPath, XmlDocMarkdownSettings settings = null)
		{
			var result = XmlDocGenerator.Generate(inputPath, outputPath, settings);
			foreach (string message in result.Messages)
				context?.Log.Write(Verbosity.Normal, LogLevel.Information, message);
			return result;
		}
	}
}
