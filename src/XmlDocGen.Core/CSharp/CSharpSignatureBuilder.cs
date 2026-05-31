using XmlDocGen.Core.Nodes;

namespace XmlDocGen.Core.CSharp;

/// <summary>Builds structured C# signatures from documentation nodes.</summary>
public abstract class CSharpSignatureBuilder
{
	/// <summary>Gets the default full-signature builder.</summary>
	public static CSharpSignatureBuilder Full { get; } = new FullCSharpSignatureBuilder();

	/// <summary>Gets the default short-signature builder.</summary>
	public static CSharpSignatureBuilder Short { get; } = new ShortCSharpSignatureBuilder();

	/// <summary>Builds a C# signature for a node.</summary>
	public abstract CSharpSignature GetSignature(XmlDocNode node);
}
