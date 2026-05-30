using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using XmlDocGen.Core.Xml;

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
