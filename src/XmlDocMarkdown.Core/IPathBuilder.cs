using System.Reflection;

namespace XmlDocMarkdown.Core;

public interface IPathBuilder
{
	public IPathBuilder WithNamespace(string @namespace);
	public IPathBuilder WithType(TypeInfo typeInfo);
	public IPathBuilder WithMemberGroup(string name);
	public IPathBuilder WithPermalinkPretty();
	public string Build();
}
