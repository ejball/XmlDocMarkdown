namespace XmlDocGen.Core.Pages;

/// <summary>Renders a page to a file.</summary>
public abstract class XmlDocPageRenderer
{
	/// <summary>Renders a page to a file.</summary>
	public abstract XmlDocRenderedFile RenderPage(XmlDocPage page, XmlDocPageContext context);
}
