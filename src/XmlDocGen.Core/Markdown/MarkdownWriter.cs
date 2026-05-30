using System.Net;
using System.Text.RegularExpressions;
using XmlDocGen.Core.CSharp;
using XmlDocGen.Core.Nodes;
using XmlDocGen.Core.Pages;
using XmlDocGen.Core.Sites;
using XmlDocGen.Core.Xml;

namespace XmlDocGen.Core.Markdown;

/// <summary>Low-level Markdown emit helpers.</summary>
public sealed class MarkdownWriter
{
	/// <summary>Initializes a new instance of the <see cref="MarkdownWriter"/> class.</summary>
	public MarkdownWriter(TextWriter writer)
	{
		Writer = writer;
	}

	/// <summary>Gets the underlying text writer.</summary>
	public TextWriter Writer { get; }

	/// <summary>Writes text.</summary>
	public void Write(string text) => Writer.Write(text);

	/// <summary>Writes a blank line.</summary>
	public void WriteLine() => Writer.WriteLine();

	/// <summary>Writes a line of text.</summary>
	public void WriteLine(string text) => Writer.WriteLine(text);

	/// <summary>Writes a Markdown link.</summary>
	public void WriteLink(string text, string url) => Write($"[{text}]({url})");

	/// <summary>Writes a Markdown heading.</summary>
	public void WriteHeading(int level, string text) => WriteLine(new string('#', level) + " " + Escape(text));

	/// <summary>Writes a Markdown table row.</summary>
	public void WriteTableRow(params string[] cells) => WriteLine("| " + string.Join(" | ", cells.Select(Escape)) + " |");

	/// <summary>Writes a Markdown table row whose cells may contain Markdown.</summary>
	public void WriteMarkdownTableRow(params string[] cells) => WriteLine("| " + string.Join(" | ", cells.Select(EscapeTableCell)) + " |");

	private static string Escape(string value) => WebUtility.HtmlEncode(value).Replace("|", "&#x7C;", StringComparison.Ordinal);

	private static string EscapeTableCell(string value) => value.ReplaceLineEndings(" ").Replace("|", "&#x7C;", StringComparison.Ordinal);
}
