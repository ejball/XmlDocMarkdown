using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;

namespace XmlDocMarkdown.Core
{
	public sealed class XmlDocAssembly
	{
		public Assembly Info { get; set; }

		public Collection<XmlDocMember> Members { get; } = new Collection<XmlDocMember>();

		public XmlDocMember FindMember(MemberInfo info)
		{
			return Members.FirstOrDefault(x => x.Info == info);
		}
	}
}
