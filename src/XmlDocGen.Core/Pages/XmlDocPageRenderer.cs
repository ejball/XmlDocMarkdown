using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Text.Json;
using XmlDocGen.Core.Nodes;
using XmlDocGen.Core.Xml;

namespace XmlDocGen.Core.Pages;

/// <summary>Renders a page to a file.</summary>
public abstract class XmlDocPageRenderer
{
	/// <summary>Renders a page to a file.</summary>
	public abstract XmlDocRenderedFile RenderPage(XmlDocPage page, XmlDocPageContext context);
}
