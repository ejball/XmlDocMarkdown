using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using XmlDocGen.Core.Xml;

namespace XmlDocGen.Core.Nodes;

/// <summary>The exact visibility of a documentation node.</summary>
public enum XmlDocVisibility
{
	/// <summary>Private.</summary>
	Private,
	/// <summary>Internal.</summary>
	Internal,
	/// <summary>Protected internal.</summary>
	ProtectedInternal,
	/// <summary>Protected.</summary>
	Protected,
	/// <summary>Public.</summary>
	Public,
}
