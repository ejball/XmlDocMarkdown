using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using XmlDocGen.Core.Nodes;

namespace XmlDocGen.Core.CSharp;

internal sealed class ShortCSharpSignatureBuilder : CSharpSignatureBuilder
{
	public override CSharpSignature GetSignature(XmlDocNode node) => new(CSharpSignatureRendering.Render(node, full: false));
}
