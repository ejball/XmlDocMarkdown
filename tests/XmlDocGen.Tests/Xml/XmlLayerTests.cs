using System.Xml.Linq;
using NUnit.Framework;
using XmlDocGen.Core.Xml;

namespace XmlDocGen.Tests.Xml;

internal sealed class XmlLayerTests
{
	[Test]
	public void RefValidatesXmlDocumentationIdentifiers()
	{
		Assert.That(new XmlDocRef("T:Example.Widget").Value, Is.EqualTo("T:Example.Widget"));
		Assert.That(() => new XmlDocRef("Example.Widget"), Throws.ArgumentException);
	}

	[Test]
	public void ParsesInlineKinds()
	{
		var file = new XmlDocXmlFile(XDocument.Parse("""
			<doc><members><member name="M:Example.Widget.Run``1(System.String)">
			<summary>Use <c>code</c>, <see cref="T:System.String" />, <see href="https://example.test/">site</see>, <see langword="null" />, <paramref name="value" />, and <typeparamref name="T" />.</summary>
			<typeparam name="T">The type.</typeparam><param name="value">The value.</param>
			</member></members></doc>
			"""));

		var member = file.FindMember(new XmlDocRef("M:Example.Widget.Run``1(System.String)"))!;

		Assert.That(member.Summary.Single().Inlines.Select(x => x.Kind), Does.Contain(XmlDocXmlInlineKind.Code));
		Assert.That(member.Summary.Single().Inlines.Select(x => x.Kind), Does.Contain(XmlDocXmlInlineKind.SeeCref));
		Assert.That(member.Summary.Single().Inlines.Select(x => x.Kind), Does.Contain(XmlDocXmlInlineKind.SeeHref));
		Assert.That(member.Summary.Single().Inlines.Select(x => x.Kind), Does.Contain(XmlDocXmlInlineKind.SeeLangword));
		Assert.That(member.Summary.Single().Inlines.Select(x => x.Kind), Does.Contain(XmlDocXmlInlineKind.ParamRef));
		Assert.That(member.Summary.Single().Inlines.Select(x => x.Kind), Does.Contain(XmlDocXmlInlineKind.TypeParamRef));
	}

	[Test]
	public void ParsesBlocksListsAndSections()
	{
		var file = new XmlDocXmlFile(XDocument.Parse("""
			<doc><members><member name="M:Example.Widget.Run">
			<summary><para>First.</para><para>Second.</para></summary>
			<remarks><list type="bullet"><item><description>Item.</description></item></list></remarks>
			<example><code lang="csharp">Console.WriteLine();</code></example>
			<returns>The result.</returns><value>The value.</value><exception cref="T:System.InvalidOperationException">Bad state.</exception>
			</member></members></doc>
			"""));

		var member = file.FindMember(new XmlDocRef("M:Example.Widget.Run"))!;

		Assert.That(member.Summary, Has.Count.EqualTo(2));
		Assert.That(member.Remarks.Single().ListKind, Is.EqualTo(XmlDocXmlListKind.Bullet));
		Assert.That(member.Examples.Single().IsCode, Is.True);
		Assert.That(member.Examples.Single().Language, Is.EqualTo("csharp"));
		Assert.That(member.ReturnValue, Is.Not.Empty);
		Assert.That(member.PropertyValue, Is.Not.Empty);
		Assert.That(member.Exceptions.Single().ExceptionTypeRef, Is.EqualTo(new XmlDocRef("T:System.InvalidOperationException")));
	}

	[Test]
	public void FileIgnoresIncludes()
	{
		var file = new XmlDocXmlFile(XDocument.Parse("""
			<doc><members><member name="T:Example.Widget"><include file="include.xml" path="/docs/summary" /></member></members></doc>
			"""));

		Assert.That(file.FindMember(new XmlDocRef("T:Example.Widget"))?.Summary, Is.Empty);
	}
}
