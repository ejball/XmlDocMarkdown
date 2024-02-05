using XmlDocMarkdown.Core;

namespace XmlDocMarkdown.Docusaurus;

internal sealed class PathBuilderFactory : IPathBuilderFactory
{
	public IPathBuilder Create() => new DocusaurusPathBuilder();
}
