using System.Reflection;
using XmlDocGen.Core;
using XmlDocGen.Core.Pages;

return XmlDocGenApp.Run(args, ctx => ctx.SourceLinks = ctx.AssemblyNames.Count == 0 ? null : XmlDocSourceLinks.TryCreate(Assembly.Load(ctx.AssemblyNames[0])));
