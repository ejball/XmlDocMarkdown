using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Text.Json;
using XmlDocGen.Core.Nodes;
using XmlDocGen.Core.Xml;

namespace XmlDocGen.Core.Pages;

/// <summary>Maps rendered pages and nodes to URLs.</summary>
public abstract class XmlDocUrlMapper
{
	/// <summary>Gets the GitHub-style URL mapper.</summary>
	public static XmlDocUrlMapper GitHub { get; } = new RelativeUrlMapper(".md");

	/// <summary>Gets the Docusaurus-style URL mapper.</summary>
	public static XmlDocUrlMapper Docusaurus { get; } = new RelativeUrlMapper("");

	/// <summary>Gets a URL from one page to a target node.</summary>
	public abstract string GetUrl(XmlDocPage fromPage, XmlDocPage targetPage, XmlDocNode targetNode);

	private sealed class RelativeUrlMapper(string extension) : XmlDocUrlMapper
	{
		public override string GetUrl(XmlDocPage fromPage, XmlDocPage targetPage, XmlDocNode targetNode)
		{
			var relative = MakeRelative(fromPage.Path + extension, targetPage.Path + extension);
			var fragment = targetNode == targetPage.Nodes[0] ? "" : "#" + Slug(XmlDocPageHeadings.GetHeadingText(targetPage, targetNode));
			return relative + fragment;
		}

		private static string MakeRelative(string fromPath, string toPath)
		{
			var from = new Uri("file:///" + fromPath.Replace('\\', '/'));
			var to = new Uri("file:///" + toPath.Replace('\\', '/'));
			var value = from.MakeRelativeUri(to).OriginalString;
			return value.Length == 0 ? "./" + to.Segments.Last() : value[0] == '.' ? value : "./" + value;
		}

		private static string Slug(string value) => string.Concat(value.ToLowerInvariant().Select(ch => char.IsLetterOrDigit(ch) ? ch : '-')).Trim('-');
	}
}
