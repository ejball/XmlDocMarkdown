using XmlDocGen.Core;
using XmlDocGen.Core.Pages;

return XmlDocGenApp.Run(args, ctx => ctx.ExternalLinks = XmlDocExternalLinkResolver.Combine(XmlDocExternalLinkResolver.UrlPattern("https://docs.example.test/{name}"), XmlDocExternalLinkResolver.DotNetApi));
