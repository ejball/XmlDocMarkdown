using XmlDocGen.Core.Sites;

namespace XmlDocGen.Core.IO;

/// <summary>Controls newline comparison when diffing files.</summary>
public enum XmlDocNewLineComparison
{
	/// <summary>Normalize line endings before comparing.</summary>
	Ignore,
	/// <summary>Compare line endings exactly.</summary>
	Exact,
}
