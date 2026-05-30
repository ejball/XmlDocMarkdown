using XmlDocGen.Core;
using XmlDocGen.Core.Markdown;
using XmlDocGen.Core.Nodes;
using XmlDocGen.Core.Pages;

return XmlDocGenApp.Run(args, ctx => ctx.Renderer = new MarkdownPageRenderer(new CustomMarkdownRenderer()));

internal sealed class CustomMarkdownRenderer : MarkdownRenderer
{
	public override void WriteSummary(MarkdownWriter writer, XmlDocNode node, XmlDocPageContext context)
	{
		base.WriteSummary(writer, node, context);
		writer.WriteLine();
		writer.WriteLine("_Generated with a custom Markdown renderer._");
	}
}
