using System;
using System.Collections.Generic;
using System.IO;
using ArgsReading;
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

				var settings = new XmlDocMarkdownSettings
				{
					NewLine = argsReader.ReadNewLineOption(),
					SourceCodePath = argsReader.ReadSourceOption(),
					RootNamespace = argsReader.ReadNamespaceOption(),
					IncludeObsolete = argsReader.ReadObsoleteFlag(),
					VisibilityLevel = argsReader.ReadVisibilityOption(),
					ShouldClean = argsReader.ReadCleanFlag(),
					IsQuiet = argsReader.ReadQuietFlag(),
					IsDryRun = isVerify || argsReader.ReadDryRunFlag(),
				};

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
			textWriter.WriteLine("   --clean");
			textWriter.WriteLine("      Deletes previously generated files that are no longer used.");
			textWriter.WriteLine("   --verify");
			textWriter.WriteLine("      Exits with error code 1 if changes to the file system are needed.");
			textWriter.WriteLine("   --dryrun");
			textWriter.WriteLine("      Executes the tool without making changes to the file system.");
			textWriter.WriteLine("   --quiet");
			textWriter.WriteLine("      Suppresses normal console output.");
			textWriter.WriteLine("   --newline (auto|lf|crlf)");
			textWriter.WriteLine("      The newline used in the output (default auto).");
		}
	}
}
