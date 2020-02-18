using System;
using System.Collections.Generic;
using System.IO;
using XmlDocMarkdown.Core;

namespace XmlDocMarkdown
{
	public sealed class XmlDocMarkdownApp
	{
		public static int Main(string[] args)
		{
			return new XmlDocMarkdownApp().Run(args);
		}

		public int Run(IReadOnlyList<string> args)
		{
			try
			{
				var argsReader = new ArgsReader(args);
				if (argsReader.ReadHelpFlag())
				{
					WriteUsage(Console.Out);
					return 0;
				}

				bool isVerify = argsReader.ReadVerifyFlag();

				var settings = new XmlDocMarkdownSettings();
				settings.NewLine = argsReader.ReadNewLineOption();
				settings.SourceCodePath = argsReader.ReadSourceOption();
				settings.RootNamespace = argsReader.ReadNamespaceOption();
				settings.IncludeObsolete = argsReader.ReadObsoleteFlag();
				settings.SkipUnbrowsable = argsReader.ReadSkipUnbrowsableFlag();
				settings.VisibilityLevel = argsReader.ReadVisibilityOption();
				settings.ShouldClean = argsReader.ReadCleanFlag();
				settings.IsQuiet = argsReader.ReadQuietFlag();
				settings.IsDryRun = isVerify || argsReader.ReadDryRunFlag();
				settings.FrontMatter = argsReader.ReadFrontMatter();
				settings.PermalinkStyle = argsReader.ReadPermalinkStyle();
				settings.GenerateToc = argsReader.ReadGeneratyeTocFlag();
				settings.TocPrefix = argsReader.ReadTocPrefix();
				settings.NamespacePages = argsReader.ReadNamespacePageFlag();

				var externalDocs = new List<ExternalDocumentation>();
				string externalOption;
				while ((externalOption = argsReader.ReadExternalOption()) != null)
					externalDocs.Add(new ExternalDocumentation { Namespace = externalOption });
				if (externalDocs.Count != 0)
					settings.ExternalDocs = externalDocs;

				string inputPath = argsReader.ReadArgument();
				if (inputPath == null)
					throw new ArgsReaderException("Missing input path.");

				string outputPath = argsReader.ReadArgument();
				if (outputPath == null)
					throw new ArgsReaderException("Missing output path.");

				argsReader.VerifyComplete();

				var result = XmlDocMarkdownGenerator.Generate(inputPath, outputPath, settings);

				foreach (string message in result.Messages)
					Console.WriteLine(message);

				return isVerify && result.Added.Count + result.Changed.Count + result.Removed.Count != 0 ? 1 : 0;
			}
			catch (Exception exception)
			{
				if (exception is ArgsReaderException)
				{
					Console.Error.WriteLine(exception.Message);
					Console.Error.WriteLine();
					WriteUsage(Console.Error);
					return 2;
				}
				else if (exception is ApplicationException || exception is IOException || exception is UnauthorizedAccessException)
				{
					Console.Error.WriteLine(exception.Message);
					return 3;
				}
				else
				{
					Console.Error.WriteLine(exception.ToString());
					return 3;
				}
			}
		}

		private void WriteUsage(TextWriter textWriter)
		{
			textWriter.WriteLine("Generates Markdown from .NET XML documentation comments.");
			textWriter.WriteLine();
			textWriter.WriteLine("Usage: XmlDocMarkdown input output [options]");
			textWriter.WriteLine();
			textWriter.WriteLine("   input");
			textWriter.WriteLine("      The path to the input assembly.");
			textWriter.WriteLine("   output");
			textWriter.WriteLine("      The path to the output directory.");
			textWriter.WriteLine();
			textWriter.WriteLine("   --source <url>");
			textWriter.WriteLine("      The URL (absolute or relative) of the folder containing the source");
			textWriter.WriteLine("      code of the assembly, e.g. at GitHub. (optional)");
			textWriter.WriteLine("   --namespace <ns>");
			textWriter.WriteLine("      The root namespace of the input assembly. (optional)");
			textWriter.WriteLine("   --visibility (public|protected|internal|private)");
			textWriter.WriteLine("      The minimum visibility of documented members. (default 'protected')");
			textWriter.WriteLine("   --obsolete");
			textWriter.WriteLine("      Generates documentation for obsolete types and members.");
			textWriter.WriteLine("   --skip-unbrowsable");
			textWriter.WriteLine("      Skips documentation for types that are marked with System.ComponentModel.EditorBrowsable Never.");
			textWriter.WriteLine("   --skip-compiler-generated");
			textWriter.WriteLine("      Skips documentation for types that are marked with System.Runtime.CompilerServices.CompilerGenerated.");
			textWriter.WriteLine("   --clean");
			textWriter.WriteLine("      Deletes previously generated files that are no longer used.");
			textWriter.WriteLine("   --verify");
			textWriter.WriteLine("      Exits with error code 1 if changes to the file system are needed.");
			textWriter.WriteLine("   --dryrun");
			textWriter.WriteLine("      Executes the tool without making changes to the file system.");
			textWriter.WriteLine("   --quiet");
			textWriter.WriteLine("      Suppresses normal console output.");
			textWriter.WriteLine("   --front-matter");
			textWriter.WriteLine("      File containing the Jekyll front matter template you want in each generated page.");
			textWriter.WriteLine("      The front matter can use $title argument and $rel for permalinks.");
			textWriter.WriteLine("      When front matter is defined the .md extension is dropped in all generated links.");
			textWriter.WriteLine("   --permalink");
			textWriter.WriteLine("      Specify permalink style, 'none' or 'pretty' (default 'none').");
			textWriter.WriteLine("      'pretty' permalinks do not contain file extensions, and when you select this option.");
			textWriter.WriteLine("      periods have to be removed from file names, for example, 'System.Console' would have to be 'SystemConsole'.");
			textWriter.WriteLine("      since the removal of the '.md' extension would make Jekyll think .Console is a file extension which doesn't work.");
			textWriter.WriteLine("   --namespace-pages");
			textWriter.WriteLine("      Generate separate pages for each namespace, listing types in each.");
			textWriter.WriteLine("   --toc");
			textWriter.WriteLine("      File containing table of contents in .yml format.");
			textWriter.WriteLine("   --newline (auto|lf|crlf)");
			textWriter.WriteLine("      The newline used in the output (default auto).");
		}
	}
}
