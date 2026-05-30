using System.Net;
using System.Text;
using XmlDocGen.Core;
using XmlDocGen.Core.Pages;

return XmlDocGenApp.Run(args, ctx => ctx.Renderer = new HtmlPageRenderer());

internal sealed class HtmlPageRenderer : XmlDocPageRenderer
{
	public override XmlDocRenderedFile RenderPage(XmlDocPage page, XmlDocPageContext context)
	{
		var builder = new StringBuilder();
		builder.AppendLine("<!doctype html>");
		builder.AppendLine("<meta charset=\"utf-8\">");
		builder.Append("<h1>").Append(WebUtility.HtmlEncode(page.Nodes[0].Name)).AppendLine("</h1>");
		foreach (var node in page.Nodes)
			builder.Append("<h2>").Append(WebUtility.HtmlEncode(node.Name)).AppendLine("</h2>");
		return new XmlDocRenderedFile(page.Path + ".html", builder.ToString());
	}
}
