namespace XmlDocGen.Core.Xml;

/// <summary>Kinds of inline XML documentation content.</summary>
public enum XmlDocXmlInlineKind
{
	/// <summary>Plain text.</summary>
	Text,

	/// <summary>Inline code.</summary>
	Code,

	/// <summary>A <c>see cref</c> link.</summary>
	SeeCref,

	/// <summary>A <c>see href</c> link.</summary>
	SeeHref,

	/// <summary>A <c>see langword</c> value.</summary>
	SeeLangword,

	/// <summary>A parameter reference.</summary>
	ParamRef,

	/// <summary>A type parameter reference.</summary>
	TypeParamRef,
}
