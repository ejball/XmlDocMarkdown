using XmlDocGen.Core.Nodes;

namespace XmlDocGen.Core.CSharp;

/// <summary>A C# signature and its token stream.</summary>
public sealed class CSharpSignature
{
	/// <summary>Initializes a new instance of the <see cref="CSharpSignature"/> class.</summary>
	public CSharpSignature(IEnumerable<CSharpToken> tokens)
	{
		Tokens = [.. tokens];
		Text = string.Concat(Tokens.Select(x => x.Text));
	}

	/// <summary>Creates a full C# signature for a node.</summary>
	public static CSharpSignature CreateFull(XmlDocNode node) => new(CSharpSignatureRendering.Render(node, full: true));

	/// <summary>Creates a short C# signature for a node.</summary>
	public static CSharpSignature CreateShort(XmlDocNode node) => new(CSharpSignatureRendering.Render(node, full: false));

	/// <summary>Gets the rendered signature text.</summary>
	public string Text { get; }

	/// <summary>Gets the signature tokens.</summary>
	public IReadOnlyList<CSharpToken> Tokens { get; }

	/// <inheritdoc />
	public override string ToString() => Text;
}
