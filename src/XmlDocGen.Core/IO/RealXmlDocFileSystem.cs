using XmlDocGen.Core.Sites;

namespace XmlDocGen.Core.IO;

internal sealed class RealXmlDocFileSystem : IXmlDocFileSystem
{
	public bool FileExists(string path) => File.Exists(path);

	public string ReadAllText(string path) => File.ReadAllText(path);

	public void WriteAllText(string path, string text)
	{
		var directory = Path.GetDirectoryName(path);
		if (!string.IsNullOrEmpty(directory))
			Directory.CreateDirectory(directory);
		File.WriteAllText(path, text);
	}

	public void DeleteFile(string path)
	{
		if (File.Exists(path))
			File.Delete(path);
	}

	public IEnumerable<string> EnumerateFiles(string directory) => Directory.Exists(directory) ? Directory.EnumerateFiles(directory, "*", SearchOption.AllDirectories) : [];
}
