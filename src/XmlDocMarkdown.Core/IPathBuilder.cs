using System.Reflection;

namespace XmlDocMarkdown.Core;

public interface IPathBuilder
{
	public IPathBuilder WithNamespace(string @namespace);
	public IPathBuilder WithType(TypeInfo typeInfo);
	public IPathBuilder WithMemberName(string name);
	public IPathBuilder WithPermalinkPretty();
	public IPathBuilder WithTypeFolders();
	public string Build();
}
