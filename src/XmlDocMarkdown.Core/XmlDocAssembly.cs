using System.Collections.ObjectModel;
using System.Linq;
using System.Xml.Linq;

namespace XmlDocMarkdown.Core
{
	internal sealed class XmlDocAssembly
	{
		public XmlDocAssembly()
		{
		}

		public XmlDocAssembly(XDocument xDocument)
		{
			var xElement = xDocument?.Root;
			if (xElement != null)
			{
				foreach (var xMember in xElement.Elements("members").Elements("member").Where(x => x.Attribute("name") != null))
					Members.Add(new XmlDocMember(xMember));
			}
		}

		public Collection<XmlDocMember> Members { get; } = new Collection<XmlDocMember>();

		public XmlDocMember FindMember(string xmlDocName) => Members.FirstOrDefault(x => x.XmlDocName == xmlDocName);
	}
}
