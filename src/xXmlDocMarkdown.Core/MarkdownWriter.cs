using System.Collections.Generic;
using System.IO;

namespace XmlDocMarkdown.Core
{
	public sealed class MarkdownWriter
	{
		public MarkdownWriter(TextWriter textWriter)
		{
			TextWriter = textWriter;
		}

		public TextWriter TextWriter { get; }

		public void Write(string text)
		{
			TextWriter.Write(text);
		}

		public void WriteLine()
		{
			TextWriter.WriteLine();
		}

		public void WriteLine(string text)
		{
			Write(text);
			WriteLine();
		}

		public void WriteLines(IEnumerable<string> lines)
		{
			foreach (var line in lines)
				WriteLine(line);
		}
	}
}
