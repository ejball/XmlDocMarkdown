using System.Collections.ObjectModel;
using System.Reflection;

namespace XmlDocMarkdown.Core
{
	public sealed class XmlDocException
	{
		public MemberInfo ExceptionType { get; set; }

		public Collection<XmlDocBlock> Condition { get; } = new Collection<XmlDocBlock>();
	}
}
