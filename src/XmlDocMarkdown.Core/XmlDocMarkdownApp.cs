using System.Reflection;

namespace XmlDocMarkdown.Core
{
	/// <summary>
	/// Implements the command-line application.
	/// </summary>
	public sealed class XmlDocMarkdownApp
	{
		/// <summary>
		/// Run the command-line application.
		/// </summary>
		/// <param name="args">The command-line arguments.</param>
		/// <returns>The exit code.</returns>
		public static int Run(IReadOnlyList<string> args)
		{
			try
			{
				var argsReader = new ArgsReader(args);
				if (argsReader.ReadHelpFlag())
				{
					WriteUsage(Console.Out);
					return 0;
				}

				var isVerify = argsReader.ReadVerifyFlag();

				var settings = new XmlDocMarkdownSettings
				{
					NewLine = argsReader.ReadNewLineOption(),
					SourceCodePath = argsReader.ReadSourceOption(),
					RootNamespace = argsReader.ReadNamespaceOption(),
					IncludeObsolete = argsReader.ReadObsoleteFlag(),
					SkipUnbrowsable = argsReader.ReadSkipUnbrowsableFlag(),
					SkipCompilerGenerated = argsReader.ReadSkipCompilerGeneratedFlag(),
					VisibilityLevel = argsReader.ReadVisibilityOption(),
					ShouldClean = argsReader.ReadCleanFlag(),
					IsQuiet = argsReader.ReadQuietFlag(),
					IsDryRun = isVerify || argsReader.ReadDryRunFlag(),
					FrontMatter = argsReader.ReadFrontMatter(),
					PermalinkStyle = argsReader.ReadPermalinkStyle(),
					GenerateToc = argsReader.ReadTocFlag(),
					TocPrefix = argsReader.ReadTocPrefix(),
					NamespacePages = argsReader.ReadNamespacePagesFlag(),
				};

				var externalDocs = new List<ExternalDocumentation>();
				while (argsReader.ReadExternalOption() is { } externalOption)
					externalDocs.Add(new ExternalDocumentation { Namespace = externalOption });
				if (externalDocs.Count != 0)
					settings.ExternalDocs = externalDocs;

				var inputPath = argsReader.ReadArgument() ?? throw new ArgsReaderException("Missing input path.");
				var input = File.Exists(inputPath)
					? new XmlDocInput { AssemblyPath = inputPath }
					: new XmlDocInput { Assembly = Assembly.Load(inputPath) };

				var outputPath = argsReader.ReadArgument() ?? throw new ArgsReaderException("Missing output path.");
				argsReader.VerifyComplete();

				var result = XmlDocMarkdownGenerator.Generate(input, outputPath, settings);

				foreach (var message in result.Messages)
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

		private static void WriteUsage(TextWriter textWriter)
		{
			textWriter.WriteLine("Generates Markdown from .NET XML documentation comments.");
			textWriter.WriteLine();
			textWriter.WriteLine($"Usage: {Assembly.GetEntryAssembly()?.GetName().Name} input output [options]");
			textWriter.WriteLine();
			textWriter.WriteLine("   input");
			textWriter.WriteLine("      The path or name of the input assembly.");
			textWriter.WriteLine("   output");
			textWriter.WriteLine("      The path of the output directory.");
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
