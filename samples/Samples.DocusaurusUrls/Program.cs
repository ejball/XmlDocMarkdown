using XmlDocGen.Core;
using XmlDocGen.Core.Pages;

return XmlDocGenApp.Run(args, ctx => ctx.UrlMapper = XmlDocUrlMapper.Docusaurus);
