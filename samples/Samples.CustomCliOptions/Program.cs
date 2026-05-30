using XmlDocGen.Core;
using XmlDocGen.Core.Nodes;

return XmlDocGenApp.Run(args, ctx =>
{
	ctx.HelpLines.Add("  --public-only   Only include public API members.");
	if (ctx.Args.ReadFlag("public-only"))
		ctx.Visibility = XmlDocNodeVisibility.Public;
});
