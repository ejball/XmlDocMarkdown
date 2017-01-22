using System.Collections.ObjectModel;
using System.Reflection;

namespace XmlDocMarkdown.Core
{
	public sealed class XmlDocMember
	{
		public MemberInfo Info { get; set; }

		public Collection<XmlDocBlock> Summary { get; } = new Collection<XmlDocBlock>();

		public Collection<XmlDocParameter> TypeParameters { get; } = new Collection<XmlDocParameter>();

		public Collection<XmlDocParameter> Parameters { get; } = new Collection<XmlDocParameter>();

		public Collection<XmlDocBlock> ReturnValue { get; } = new Collection<XmlDocBlock>();

		public Collection<XmlDocBlock> PropertyValue { get; } = new Collection<XmlDocBlock>();

		public Collection<XmlDocException> Exceptions { get; } = new Collection<XmlDocException>();

		public Collection<XmlDocBlock> Remarks { get; } = new Collection<XmlDocBlock>();

		public Collection<XmlDocBlock> Examples { get; } = new Collection<XmlDocBlock>();

		public override string ToString()
		{
			return Info.ToString();
		}
	}
}
