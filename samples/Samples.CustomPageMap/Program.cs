using XmlDocGen.Core;
using XmlDocGen.Core.Nodes;
using XmlDocGen.Core.Pages;

return XmlDocGenApp.Run(args, ctx => ctx.PageMap = new ApiPageMap());

internal sealed class ApiPageMap : XmlDocPageMap
{
	public override string GetPagePath(XmlDocNode node)
	{
		return node switch
		{
			XmlDocAssemblyNode => "api/index",
			XmlDocNamespaceNode namespaceNode => "api/namespaces/" + GetSafeName(namespaceNode.Name),
			XmlDocTypeNode typeNode => "api/types/" + GetSafeName(typeNode.Name),
			XmlDocMemberNode memberNode => "api/types/" + GetSafeName(memberNode.Parent!.Name),
			_ => "api/index",
		};
	}
}
