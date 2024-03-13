using System.Reflection;
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
		public static XmlDocMarkdownResult Generate(string inputPath, string outputPath, XmlDocMarkdownSettings? settings)
		{
			if (inputPath == null)
				throw new ArgumentNullException(nameof(inputPath));

			return Generate(new XmlDocInput { AssemblyPath = inputPath }, outputPath, settings);
		}

		/// <summary>
		/// Generates Markdown from .NET XML documentation comments.
		/// </summary>
		/// <param name="input">The input.</param>
		/// <param name="outputPath">The output directory.</param>
		/// <param name="settings">The settings.</param>
		/// <returns>The names of files that were added, changed, or removed.</returns>
		public static XmlDocMarkdownResult Generate(XmlDocInput input, string outputPath, XmlDocMarkdownSettings? settings)
		{
			if (input == null)
				throw new ArgumentNullException(nameof(input));
			if (outputPath == null)
				throw new ArgumentNullException(nameof(outputPath));

			var result = new XmlDocMarkdownResult();

			settings ??= new XmlDocMarkdownSettings();

			var generator = new MarkdownGenerator
			{
				SourceCodePath = settings.SourceCodePath,
				RootNamespace = settings.RootNamespace,
				IncludeObsolete = settings.IncludeObsolete,
				SkipUnbrowsable = settings.SkipUnbrowsable,
				SkipCompilerGenerated = settings.SkipCompilerGenerated,
				Visibility = settings.VisibilityLevel ?? XmlDocVisibilityLevel.Protected,
				ExternalDocs = settings.ExternalDocs,
				NamespacePages = settings.NamespacePages,
				FrontMatter = settings.FrontMatter,
			};
			if (settings.NewLine != null)
				generator.NewLine = settings.NewLine;

			if (string.Equals(settings.PermalinkStyle, "pretty", StringComparison.OrdinalIgnoreCase))
				generator.PermalinkPretty = true;

			XmlDocAssembly xmlDocAssembly;

			var assembly = input.Assembly ?? Assembly.LoadFrom(input.AssemblyPath);
			var xmlDocPath = input.XmlDocPath;
			if (xmlDocPath == null)
			{
				var assemblyPath = input.AssemblyPath ?? assembly.Location;

				xmlDocPath = Path.ChangeExtension(assemblyPath, ".xml");
				if (!File.Exists(xmlDocPath))
					xmlDocPath = Path.ChangeExtension(assemblyPath, ".XML");
			}

			if (xmlDocPath != null && File.Exists(xmlDocPath))
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
				var existingFilePath = Path.Combine(outputPath, namedText.Name);
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
				var tocPath = Path.Combine(outputPath, "toc.yml");

				var root = namedTexts.FirstOrDefault();
				if (root != null)
				{
					var toc = new XmlDocToc { Path = root.Name, Title = root.Title, Prefix = settings.TocPrefix };

					foreach (var namedText in namedTexts.Skip(1))
						toc.AddChild(namedText.Name, namedText.Parent, namedText.Title);

					toc.Save(tocPath);
				}
			}

			var namesToDelete = new List<string>();
			if (settings.ShouldClean)
			{
				var directoryInfo = new DirectoryInfo(outputPath);
				if (directoryInfo.Exists)
				{
					var assemblyName = assembly.GetName().Name;
					var assemblyFilePath = assembly.Modules.FirstOrDefault()?.FullyQualifiedName;
					var assemblyFileName = assemblyFilePath != null ? Path.GetFileName(assemblyFilePath) : assemblyName;
					var assemblyFolder = Path.GetFileNameWithoutExtension(assemblyFileName);
					var patterns = new[] { $"{assemblyFolder}/*.md", $"{assemblyFolder}/*/*.md" };
					var codeGenComment = MarkdownGenerator.GetCodeGenComment(assemblyFileName);

					foreach (var nameMatchingPattern in FindNamesMatchingPatterns(directoryInfo, patterns, codeGenComment))
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
					var outputFilePath = Path.Combine(outputPath, namedText.Name);

					var outputFileDirectoryPath = Path.GetDirectoryName(outputFilePath);
					if (outputFileDirectoryPath != null && outputFileDirectoryPath != outputPath && !Directory.Exists(outputFileDirectoryPath))
						Directory.CreateDirectory(outputFileDirectoryPath);

					File.WriteAllText(outputFilePath, namedText.Text);
				}

				foreach (var nameToDelete in namesToDelete)
					File.Delete(Path.Combine(outputPath, nameToDelete));
			}

			return result;
		}

		private static IEnumerable<string> FindNamesMatchingPatterns(DirectoryInfo directoryInfo, IReadOnlyList<string> namePatterns, string requiredSubstring)
		{
			foreach (var namePattern in namePatterns)
			{
				foreach (var name in FindNamesMatchingPattern(directoryInfo, namePattern, requiredSubstring))
					yield return name;
			}
		}

		private static IEnumerable<string> FindNamesMatchingPattern(DirectoryInfo directoryInfo, string namePattern, string requiredSubstring)
		{
			var parts = namePattern.Split(['/'], 2);
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
					foreach (var name in FindNamesMatchingPattern(subdirectoryInfo, parts[1], requiredSubstring))
						yield return subdirectoryInfo.Name + '/' + name;
				}
			}
		}
	}
}
