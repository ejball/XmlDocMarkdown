using System.Collections.ObjectModel;
using System.Xml.Linq;

namespace XmlDocMarkdown.Core;

internal sealed class XmlDocFile
{
	public XmlDocFile()
	{
	}

	public XmlDocFile(XDocument xDocument)
	{
		if (xDocument.Root is { } xElement)
		{
			foreach (var xMember in xElement.Elements("members").Elements("member").Where(x => x.Attribute("name") is not null))
				Members.Add(new XmlDocMember(xMember));
		}
	}

	public Collection<XmlDocMember> Members { get; } = [];

	public XmlDocMember? FindMember(string? xmlDocName) => Members.FirstOrDefault(x => x.XmlDocName == xmlDocName);
}
