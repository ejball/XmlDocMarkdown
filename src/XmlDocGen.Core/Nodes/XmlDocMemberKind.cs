namespace XmlDocGen.Core.Nodes;

/// <summary>Kinds of documented members.</summary>
public enum XmlDocMemberKind
{
	/// <summary>A constructor.</summary>
	Constructor,

	/// <summary>A method.</summary>
	Method,

	/// <summary>A property.</summary>
	Property,

	/// <summary>A field.</summary>
	Field,

	/// <summary>An event.</summary>
	Event,

	/// <summary>An operator.</summary>
	Operator,
}
