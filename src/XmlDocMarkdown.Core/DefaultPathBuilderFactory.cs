namespace XmlDocMarkdown.Core;

internal class DefaultPathBuilderFactory : IPathBuilderFactory
{
	public IPathBuilder Create() => new JekyllPathBuilder();
}
