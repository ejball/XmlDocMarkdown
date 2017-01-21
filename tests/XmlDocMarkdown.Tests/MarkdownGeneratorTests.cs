using NUnit.Framework;
using Shouldly;
using XmlDocMarkdown.Core;

namespace XmlDocMarkdown.Tests
{
	[TestFixture]
	public class MarkdownGeneratorTests
	{
		[Test]
		public void ToDo()
		{
			new MarkdownGenerator().ShouldNotBeNull();
		}
	}
}
