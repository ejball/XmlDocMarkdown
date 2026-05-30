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

	private static string Escape(string value) => WebUtility.HtmlEncode(value).Replace("|", "&#x7C;", StringComparison.Ordinal);
}

/// <summary>Renders node and XML content as Markdown building blocks.</summary>
public class MarkdownRenderer
{
	/// <summary>Writes a C# signature block.</summary>
	public virtual void WriteSignature(MarkdownWriter writer, XmlDocNode node, XmlDocPageContext context)
	{
		writer.WriteLine("```csharp");
		writer.WriteLine(CSharpSignatureBuilder.Full.GetSignature(node).Text);
		writer.WriteLine("```");
	}

	/// <summary>Writes a summary section.</summary>
	public virtual void WriteSummary(MarkdownWriter writer, XmlDocNode node, XmlDocPageContext context) => WriteBlocks(writer, node.Xml?.Summary, context);

	/// <summary>Writes a remarks section.</summary>
	public virtual void WriteRemarks(MarkdownWriter writer, XmlDocNode node, XmlDocPageContext context)
	{
		if (node.Xml?.Remarks.Count > 0)
		{
			writer.WriteLine();
			writer.WriteHeading(2, "Remarks");
			WriteBlocks(writer, node.Xml.Remarks, context);
		}
	}

	/// <summary>Writes a parameter section.</summary>
	public virtual void WriteParameters(MarkdownWriter writer, XmlDocMemberNode member, XmlDocPageContext context)
	{
		var typeParameters = member.Xml?.TypeParameters ?? [];
		var parameters = member.Xml?.Parameters ?? [];
		if (typeParameters.Count + parameters.Count == 0)
			return;

		writer.WriteLine();
		writer.WriteTableRow("parameter", "description");
		writer.WriteLine("| --- | --- |");
		foreach (var parameter in typeParameters.Concat(parameters))
			writer.WriteTableRow(parameter.Name, RenderBlocksInline(parameter.Description, context));
	}

	/// <summary>Writes a see-also section.</summary>
	public virtual void WriteSeeAlso(MarkdownWriter writer, XmlDocNode node, XmlDocPageContext context)
	{
		if (node.Xml?.SeeAlso.Count > 0)
		{
			writer.WriteLine();
			writer.WriteHeading(2, "See Also");
			foreach (var seeAlso in node.Xml.SeeAlso)
			{
				var text = seeAlso.Text;
				var url = seeAlso.Href;
				if (seeAlso.Ref is { } reference)
				{
					text = string.IsNullOrWhiteSpace(text) ? XmlDocRefUtility.GetShortNameForXmlDocRef(reference) : text;
					url = context.GetLinkUrl(reference);
				}
				writer.WriteLine(url is null ? "* " + text : $"* [{Escape(text ?? url)}]({url})");
			}
		}
	}

	/// <summary>Writes inline XML documentation content.</summary>
	public virtual void WriteInlines(MarkdownWriter writer, IEnumerable<XmlDocXmlInline> inlines, XmlDocPageContext context) => writer.Write(RenderInlines(inlines, context));

	/// <summary>Writes blocks of XML documentation content.</summary>
	protected void WriteBlocks(MarkdownWriter writer, IEnumerable<XmlDocXmlBlock>? blocks, XmlDocPageContext context)
	{
		if (blocks is null)
			return;

		var isFirst = true;
		foreach (var block in blocks)
		{
			if (!isFirst)
				writer.WriteLine();
			isFirst = false;

			if (block.IsCode)
			{
				writer.WriteLine("```" + (block.Language ?? "csharp"));
				foreach (var inline in block.Inlines)
					writer.WriteLine(inline.Text ?? "");
				writer.WriteLine("```");
			}
			else if (block.ListKind is XmlDocXmlListKind.Bullet or XmlDocXmlListKind.Number)
			{
				var prefix = block.ListKind == XmlDocXmlListKind.Number ? "1. " : "* ";
				writer.WriteLine(new string(' ', block.ListDepth * 2) + prefix + RenderInlines(block.Inlines, context));
			}
			else
			{
				writer.WriteLine(RenderInlines(block.Inlines, context));
			}
		}
	}

	private string RenderBlocksInline(IEnumerable<XmlDocXmlBlock> blocks, XmlDocPageContext context) => string.Join(" ", blocks.Select(x => RenderInlines(x.Inlines, context)));

	private string RenderInlines(IEnumerable<XmlDocXmlInline> inlines, XmlDocPageContext context) => Regex.Replace(string.Concat(inlines.Select(x => RenderInline(x, context))), @"\s+", " ").Trim();

	private string RenderInline(XmlDocXmlInline inline, XmlDocPageContext context)
	{
		var text = inline.Text ?? "";
		if (inline.Kind == XmlDocXmlInlineKind.SeeCref && inline.Ref is { } reference)
		{
			text = string.IsNullOrWhiteSpace(text) ? XmlDocRefUtility.GetShortNameForXmlDocRef(reference) : text;
			var url = context.GetLinkUrl(reference);
			return url is null ? Code(text) : $"[{Code(text)}]({url})";
		}
		if (inline.Kind == XmlDocXmlInlineKind.SeeHref && inline.Href is { } href)
			return $"[{Escape(string.IsNullOrWhiteSpace(text) ? href : text)}]({href})";
		if (inline.Kind == XmlDocXmlInlineKind.SeeLangword)
			return Code(inline.Langword ?? text);
		if (inline.Kind == XmlDocXmlInlineKind.Code)
			return Code(text);
		if (inline.Kind is XmlDocXmlInlineKind.ParamRef or XmlDocXmlInlineKind.TypeParamRef)
			return "*" + Escape(text) + "*";
		return Escape(text);
	}

	private static string Code(string value)
	{
		var ticks = new string('`', Regex.Matches(value, "`+").Select(x => x.Length).Concat([0]).Max() + 1);
		return ticks + value + ticks;
	}

	private static string Escape(string value) => WebUtility.HtmlEncode(value).Replace("|", "&#x7C;", StringComparison.Ordinal);
}

/// <summary>A page renderer that emits Markdown.</summary>
public class MarkdownPageRenderer : XmlDocPageRenderer
{
	/// <summary>Initializes a new instance of the <see cref="MarkdownPageRenderer"/> class.</summary>
	public MarkdownPageRenderer(MarkdownRenderer? renderer = null)
	{
		Renderer = renderer ?? new MarkdownRenderer();
	}

	/// <summary>Gets the section renderer.</summary>
	protected MarkdownRenderer Renderer { get; }

	/// <inheritdoc />
	public override XmlDocRenderedFile RenderPage(XmlDocPage page, XmlDocPageContext context)
	{
		using var stringWriter = new StringWriter();
		var writer = new MarkdownWriter(stringWriter);
		WriteFrontMatter(writer, page);
		WriteHeader(writer, page, context);
		WriteBody(writer, page, context);
		return new XmlDocRenderedFile(page.Path + ".md", stringWriter.ToString());
	}

	/// <summary>Emits front matter for a page.</summary>
	protected virtual void WriteFrontMatter(MarkdownWriter writer, XmlDocPage page)
	{
	}

	/// <summary>Writes the page header.</summary>
	protected virtual void WriteHeader(MarkdownWriter writer, XmlDocPage page, XmlDocPageContext context)
	{
		writer.WriteHeading(1, page.Nodes[0].Name);
	}

	/// <summary>Writes the page body.</summary>
	protected virtual void WriteBody(MarkdownWriter writer, XmlDocPage page, XmlDocPageContext context)
	{
		foreach (var (node, index) in page.Nodes.Select((node, index) => (node, index)))
		{
			if (index != 0)
			{
				writer.WriteLine();
				writer.WriteHeading(2, node.Name);
			}

			writer.WriteLine();
			Renderer.WriteSummary(writer, node, context);
			writer.WriteLine();
			Renderer.WriteSignature(writer, node, context);
			if (node is XmlDocMemberNode member)
				Renderer.WriteParameters(writer, member, context);
			Renderer.WriteRemarks(writer, node, context);
			Renderer.WriteSeeAlso(writer, node, context);
		}
	}
}

/// <summary>Convenience builder for Markdown sites.</summary>
public sealed class MarkdownSiteBuilder
{
	/// <summary>Initializes a new instance of the <see cref="MarkdownSiteBuilder"/> class.</summary>
	public MarkdownSiteBuilder(XmlDocSiteBuilderSettings? settings = null, MarkdownPageRenderer? renderer = null)
	{
		Settings = settings ?? new XmlDocSiteBuilderSettings();
		Renderer = renderer ?? new MarkdownPageRenderer();
	}

	/// <summary>Gets the settings.</summary>
	public XmlDocSiteBuilderSettings Settings { get; }

	/// <summary>Gets the renderer.</summary>
	public MarkdownPageRenderer Renderer { get; }

	/// <summary>Builds a Markdown site.</summary>
	public XmlDocSite Build(XmlDocTree tree) => new XmlDocSiteBuilder(Renderer, Settings).Build(tree);
}
