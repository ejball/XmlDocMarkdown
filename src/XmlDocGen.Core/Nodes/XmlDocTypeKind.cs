namespace XmlDocGen.Core.Nodes;

/// <summary>Kinds of documented types.</summary>
public enum XmlDocTypeKind
{
	/// <summary>A class.</summary>
	Class,

	/// <summary>An interface.</summary>
	Interface,

	/// <summary>A struct.</summary>
	Struct,

	/// <summary>An enum.</summary>
	Enum,

	/// <summary>A delegate.</summary>
	Delegate,

	/// <summary>A record class.</summary>
	Record,

	/// <summary>A record struct.</summary>
	RecordStruct,
}
