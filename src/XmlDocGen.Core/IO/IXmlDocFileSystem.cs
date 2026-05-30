using XmlDocGen.Core.Sites;

namespace XmlDocGen.Core.IO;

/// <summary>Abstracts file-system operations.</summary>
public interface IXmlDocFileSystem
{
	/// <summary>Returns true if the file exists.</summary>
	bool FileExists(string path);

	/// <summary>Reads all text from a file.</summary>
	string ReadAllText(string path);

	/// <summary>Writes all text to a file.</summary>
	void WriteAllText(string path, string text);

	/// <summary>Deletes a file.</summary>
	void DeleteFile(string path);

	/// <summary>Enumerates files under a directory.</summary>
	IEnumerable<string> EnumerateFiles(string directory);
}
