using XmlDocMarkdown.Core;

namespace XmlDocMarkdown.Docusaurus;

internal class PathBuilderFactory : IPathBuilderFactory
{
	public IPathBuilder Create() => new DocusaurusPathBuilder();
}
