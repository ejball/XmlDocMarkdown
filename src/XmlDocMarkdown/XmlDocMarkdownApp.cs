using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
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

				var generator = new MarkdownGenerator();

				string newLine = argsReader.ReadNewLineOption();
				if (newLine != null)
					generator.NewLine = newLine;

				generator.SourceCodePath = argsReader.ReadSourceOption();
				generator.RootNamespace = argsReader.ReadNamespaceOption();

				bool shouldClean = argsReader.ReadCleanFlag();
				bool isQuiet = argsReader.ReadQuietFlag();
				bool isVerify = argsReader.ReadVerifyFlag();
				bool isDryRun = argsReader.ReadDryRunFlag();

				string inputPath = argsReader.ReadArgument();
				if (inputPath == null)
					throw new ArgsReaderException("Missing input path.");

				string outputPath = argsReader.ReadArgument();
				if (outputPath == null)
					throw new ArgsReaderException("Missing output path.");

				argsReader.VerifyComplete();

				var assembly = Assembly.LoadFrom(inputPath);
				XmlDocAssembly xmlDocAssembly;

				var xmlDocPath = Path.ChangeExtension(inputPath, ".xml");
				if (!File.Exists(xmlDocPath))
					xmlDocPath = Path.ChangeExtension(inputPath, ".XML");

				if (File.Exists(xmlDocPath))
				{
					var xDocument = XDocument.Load(xmlDocPath);
					xmlDocAssembly = new XmlDocAssembly(xDocument);
				}
				else
				{
					xmlDocAssembly = new XmlDocAssembly();
				}

				var namedTexts = generator.GenerateOutput(assembly, xmlDocAssembly);

				var namedTextsToWrite = new List<NamedText>();
				foreach (var namedText in namedTexts)
				{
					string existingFilePath = Path.Combine(outputPath, namedText.Name);
					if (File.Exists(existingFilePath))
					{
						// ignore CR when comparing files
						if (namedText.Text.Replace("\r", "") != File.ReadAllText(existingFilePath).Replace("\r", ""))
						{
							namedTextsToWrite.Add(namedText);
							if (!isQuiet)
								Console.WriteLine("changed " + namedText.Name);
						}
					}
					else
					{
						namedTextsToWrite.Add(namedText);
						if (!isQuiet)
							Console.WriteLine("added " + namedText.Name);
					}
				}

				var namesToDelete = new List<string>();
				if (shouldClean)
				{
					var directoryInfo = new DirectoryInfo(outputPath);
					if (directoryInfo.Exists)
					{
						string assemblyName = assembly.GetName().Name;
						string assemblyFilePath = assembly.Modules.FirstOrDefault()?.FullyQualifiedName;
						string assemblyFileName = assemblyFilePath != null ? Path.GetFileName(assemblyFilePath) : assemblyName;
						string assemblyFolder = Path.GetFileNameWithoutExtension(assemblyFileName);
						var patterns = new[] { $"{assemblyFolder}/*.md", $"{assemblyFolder}/*/*.md" };
						string codeGenComment = MarkdownGenerator.GetCodeGenComment(assemblyFileName);

						foreach (string nameMatchingPattern in FindNamesMatchingPatterns(directoryInfo, patterns, codeGenComment))
						{
							if (namedTexts.All(x => x.Name != nameMatchingPattern))
							{
								namesToDelete.Add(nameMatchingPattern);
								if (!isQuiet)
									Console.WriteLine("removed " + nameMatchingPattern);
							}
						}
					}
				}

				if (isVerify)
					return namedTextsToWrite.Count != 0 || namesToDelete.Count != 0 ? 1 : 0;

				if (!isDryRun)
				{
					if (!Directory.Exists(outputPath))
						Directory.CreateDirectory(outputPath);

					foreach (var namedText in namedTextsToWrite)
					{
						string outputFilePath = Path.Combine(outputPath, namedText.Name);

						string outputFileDirectoryPath = Path.GetDirectoryName(outputFilePath);
						if (outputFileDirectoryPath != null && outputFileDirectoryPath != outputPath && !Directory.Exists(outputFileDirectoryPath))
							Directory.CreateDirectory(outputFileDirectoryPath);

						File.WriteAllText(outputFilePath, namedText.Text);
					}

					foreach (string nameToDelete in namesToDelete)
						File.Delete(Path.Combine(outputPath, nameToDelete));
				}

				return 0;
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

		private IEnumerable<string> FindNamesMatchingPatterns(DirectoryInfo directoryInfo, IReadOnlyList<string> namePatterns, string requiredSubstring)
		{
			foreach (var namePattern in namePatterns)
			{
				foreach (string name in FindNamesMatchingPattern(directoryInfo, namePattern, requiredSubstring))
					yield return name;
			}
		}

		private IEnumerable<string> FindNamesMatchingPattern(DirectoryInfo directoryInfo, string namePattern, string requiredSubstring)
		{
			var parts = namePattern.Split(new[] { '/' }, 2);
			if (parts[0].Length == 0)
				throw new InvalidOperationException("Invalid name pattern.");

			if (parts.Length == 1)
			{
				foreach (var fileInfo in directoryInfo.GetFiles(parts[0]))
				{
					if (File.ReadAllText(fileInfo.FullName).Contains(requiredSubstring))
						yield return fileInfo.Name;
				}
			}
			else
			{
				foreach (var subdirectoryInfo in directoryInfo.GetDirectories(parts[0]))
				{
					foreach (string name in FindNamesMatchingPattern(subdirectoryInfo, parts[1], requiredSubstring))
						yield return subdirectoryInfo.Name + '/' + name;
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
