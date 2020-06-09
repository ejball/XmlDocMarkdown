using System;

namespace XmlDocMarkdown.Core
{
	/// <summary>
	/// The approach used for documenting source code paths, if `--source` is given.
	/// </summary>
	[Flags]
	public enum XmlDocSourceCodeStyle
	{
		/// <summary>
		/// Path is based on type name, and C# is assumed.
		/// </summary>
		Default = 0,

		/// <summary>
		/// If no other path is found, report based on type name, and C# is assumed.
		/// </summary>
		TypeName = 1,

		/// <summary>
		/// If debug symbol information is found, report based on the document(s)
		/// in which the type has members.
		/// </summary>
		DebugSymbol = 2,

		/// <summary>
		/// If debug symbol information is found, and sourcelink information is
		/// present, use that to report based on the document(s)
		/// in which the type has members, in preference to all else.
		/// </summary>
		SourceLink = 4
	}
}
