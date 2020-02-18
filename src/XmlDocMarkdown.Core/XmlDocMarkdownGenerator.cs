using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;

namespace XmlDocMarkdown.Core
{
	/// <summary>
	/// Generates Markdown from .NET XML documentation comments.
	/// </summary>
	public static class XmlDocMarkdownGenerator
	{
		/// <summary>
		/// Generates Markdown from .NET XML documentation comments.
		/// </summary>
		/// <param name="inputPath">The input assembly.</param>
		/// <param name="outputPath">The output directory.</param>
		/// <param name="settings">The settings.</param>
		/// <returns>The names of files that were added, changed, or removed.</returns>
		public static XmlDocMarkdownResult Generate(string inputPath, string outputPath, XmlDocMarkdownSettings settings)
		{
			if (inputPath == null)
				throw new ArgumentNullException(nameof(inputPath));
			if (outputPath == null)
				throw new ArgumentNullException(nameof(outputPath));

			var result = new XmlDocMarkdownResult();

			settings = settings ?? new XmlDocMarkdownSettings();

			var generator = new MarkdownGenerator
			{
				SourceCodePath = settings.SourceCodePath,
				RootNamespace = settings.RootNamespace,
				IncludeObsolete = settings.IncludeObsolete,
				SkipUnbrowsable = settings.SkipUnbrowsable,
				Visibility = settings.VisibilityLevel ?? XmlDocVisibilityLevel.Protected,
				ExternalDocs = settings.ExternalDocs,
				NamespacePages = settings.NamespacePages,
				FrontMatter = settings.FrontMatter,
			};
			if (settings.NewLine != null)
				generator.NewLine = settings.NewLine;

			if (string.Compare(settings.PermalinkStyle, "pretty", StringComparison.OrdinalIgnoreCase) == 0)
				generator.PermalinkPretty = true;

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
						result.Changed.Add(namedText.Name);
						if (!settings.IsQuiet)
							result.Messages.Add("changed " + namedText.Name);
					}
				}
				else
				{
					namedTextsToWrite.Add(namedText);
					result.Added.Add(namedText.Name);
					if (!settings.IsQuiet)
						result.Messages.Add("added " + namedText.Name);
				}
			}

			if (settings.GenerateToc)
			{
				string tocPath = Path.Combine(outputPath, "toc.yml");

				NamedText root = namedTexts.FirstOrDefault();
				if (root != null)
				{
					XmlDocToc toc = new XmlDocToc() { Path = root.Name, Title = root.Title, Prefix = settings.TocPrefix };

					foreach (var namedText in namedTexts.Skip(1))
					{
						toc.AddChild(namedText.Name, namedText.Parent, namedText.Title);
					}

					toc.Save(tocPath);
				}
			}

			var namesToDelete = new List<string>();
			if (settings.ShouldClean)
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
							result.Removed.Add(nameMatchingPattern);
							if (!settings.IsQuiet)
								result.Messages.Add("removed " + nameMatchingPattern);
						}
					}
				}
			}

			if (!settings.IsDryRun)
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

			return result;
		}

		private static IEnumerable<string> FindNamesMatchingPatterns(DirectoryInfo directoryInfo, IReadOnlyList<string> namePatterns, string requiredSubstring)
		{
			foreach (var namePattern in namePatterns)
			{
				foreach (string name in FindNamesMatchingPattern(directoryInfo, namePattern, requiredSubstring))
					yield return name;
			}
		}

		private static IEnumerable<string> FindNamesMatchingPattern(DirectoryInfo directoryInfo, string namePattern, string requiredSubstring)
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
	}
}
