using XmlDocGen.Core;
using XmlDocGen.Core.Markdown;
using XmlDocGen.Core.Pages;

return XmlDocGenApp.Run(args, ctx => ctx.Renderer = new FrontMatterRenderer());

internal sealed class FrontMatterRenderer : MarkdownPageRenderer
{
	protected override void WriteFrontMatter(MarkdownWriter writer, XmlDocPage page)
	{
		writer.WriteLine("---");
		writer.WriteLine("title: " + page.Nodes[0].Name);
		writer.WriteLine("---");
	}
}
