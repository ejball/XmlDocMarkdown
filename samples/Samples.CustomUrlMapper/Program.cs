using XmlDocGen.Core;
using XmlDocGen.Core.Nodes;
using XmlDocGen.Core.Pages;

return XmlDocGenApp.Run(args, ctx => ctx.UrlMapper = new AbsoluteApiUrlMapper());

internal sealed class AbsoluteApiUrlMapper : XmlDocUrlMapper
{
	public override string GetUrl(XmlDocPage fromPage, XmlDocPage targetPage, XmlDocNode targetNode) => "/api/" + targetPage.Path + ".html#" + targetNode.Name.ToLowerInvariant();
}
