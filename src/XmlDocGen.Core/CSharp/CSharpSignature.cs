using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
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

	/// <summary>Gets the rendered signature text.</summary>
	public string Text { get; }

	/// <summary>Gets the signature tokens.</summary>
	public IReadOnlyList<CSharpToken> Tokens { get; }

	/// <inheritdoc />
	public override string ToString() => Text;
}
