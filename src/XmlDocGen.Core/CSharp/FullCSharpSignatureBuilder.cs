using XmlDocGen.Core.Nodes;

namespace XmlDocGen.Core.CSharp;

internal sealed class FullCSharpSignatureBuilder : CSharpSignatureBuilder
{
	public override CSharpSignature GetSignature(XmlDocNode node) => new(CSharpSignatureRendering.Render(node, full: true));
}
