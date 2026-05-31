namespace XmlDocGen.Core.CSharp;

/// <summary>Kinds of C# signature tokens.</summary>
public enum CSharpTokenKind
{
	/// <summary>A keyword.</summary>
	Keyword,

	/// <summary>An identifier.</summary>
	Identifier,

	/// <summary>A type name.</summary>
	TypeName,

	/// <summary>An operator.</summary>
	Operator,

	/// <summary>Punctuation.</summary>
	Punctuation,

	/// <summary>Whitespace.</summary>
	Whitespace,

	/// <summary>A literal value.</summary>
	Literal,

	/// <summary>Plain text.</summary>
	Text,
}
