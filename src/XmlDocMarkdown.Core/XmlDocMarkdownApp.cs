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
		/// <param name="configure">Called to configure the settings.</param>
		/// <returns>The exit code.</returns>
		public static int Run(IReadOnlyList<string> args, Action<string, XmlDocMarkdownSettings>? configure = null)
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
					ShouldClean = argsReader.ReadCleanFlag(),
					IsQuiet = argsReader.ReadQuietFlag(),
					IsDryRun = isVerify || argsReader.ReadDryRunFlag(),
				};

				var assemblyName = argsReader.ReadArgument() ?? throw new ArgsReaderException("Missing assembly name.");
				var outputPath = argsReader.ReadArgument() ?? throw new ArgsReaderException("Missing output path.");
				argsReader.VerifyComplete();

				configure?.Invoke(assemblyName, settings);

				var input = new XmlDocInput { Assembly = Assembly.Load(assemblyName) };
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
				else if (exception is ApplicationException or IOException or UnauthorizedAccessException)
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
			textWriter.WriteLine("      The name of the input assembly.");
			textWriter.WriteLine("   output");
			textWriter.WriteLine("      The path of the output directory.");
			textWriter.WriteLine();
			textWriter.WriteLine("   --clean");
			textWriter.WriteLine("      Deletes previously generated files that are no longer used.");
			textWriter.WriteLine("   --dryrun");
			textWriter.WriteLine("      Executes the tool without making changes to the file system.");
			textWriter.WriteLine("   --quiet");
			textWriter.WriteLine("      Suppresses normal console output.");
			textWriter.WriteLine("   --verify");
			textWriter.WriteLine("      Exits with error code 1 if changes to the file system are needed.");
		}
	}
}
