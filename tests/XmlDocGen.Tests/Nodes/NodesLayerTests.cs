using System.ComponentModel;
using System.Reflection;
using ExampleAssembly;
using NUnit.Framework;
using XmlDocGen.Core.Nodes;
using XmlDocGen.Core.Xml;
using XmlDocGen.Tests.Fixtures;

namespace XmlDocGen.Tests.Nodes;

internal sealed class NodesLayerTests
{
	[Test]
	public void TreeCreatesAssemblyNamespaceTypesAndMembers()
	{
		var tree = TestSupport.CreateExampleTree();
		var typeNode = (XmlDocTypeNode) tree.FindNode(XmlDocRef.ForType(typeof(ExampleClass)))!;
		var propertyNode = (XmlDocMemberNode) tree.FindNode(XmlDocRef.ForMember(typeof(ExampleClass).GetProperty(nameof(ExampleClass.Id))!))!;

		Assert.That(tree.Assemblies.Single().Namespaces, Has.Some.Property(nameof(XmlDocNamespaceNode.Name)).EqualTo("ExampleAssembly"));
		Assert.That(typeNode.Kind, Is.EqualTo(XmlDocTypeKind.Class));
		Assert.That(typeNode.Members, Does.Contain(propertyNode));
		Assert.That(propertyNode.MemberKind, Is.EqualTo(XmlDocMemberKind.Property));
	}

	[Test]
	public void VisibilityCanBeSubclassedAndComposed()
	{
		var tree = TestSupport.CreateExampleTree();
		var visibility = new NoMemberVisibility().ExcludeObsolete().ExcludeUnbrowsable();

		Assert.That(tree.EnumerateNodes(visibility), Has.Some.InstanceOf<XmlDocTypeNode>());
		Assert.That(tree.EnumerateNodes(visibility), Has.None.InstanceOf<XmlDocMemberNode>());
	}

	[Test]
	public void TryGetAttributeSurfacesReflectionAttributes()
	{
		var tree = TestSupport.CreateExampleTree();
		var obsoleteType = typeof(ExampleClass).Assembly.GetType("ExampleAssembly.ExampleObsoleteClass")!;
		var obsoleteNode = tree.FindNode(XmlDocRef.ForType(obsoleteType))!;

		Assert.That(obsoleteNode.MemberInfo?.GetCustomAttributes<ObsoleteAttribute>().FirstOrDefault(), Is.Not.Null);
	}

	[Test]
	public void VisibilityFiltersCompilerAndBrowsableAttributes()
	{
		var tree = TestSupport.CreateExampleTree();
		var unbrowsable = tree.FindNode(XmlDocRef.ForType(typeof(ExampleUnbrowsableClass)))!;

		Assert.That(unbrowsable.MemberInfo?.GetCustomAttributes<EditorBrowsableAttribute>().FirstOrDefault(), Is.Not.Null);
		Assert.That(XmlDocNodeVisibility.Public.IsVisible(unbrowsable), Is.True);
		Assert.That(XmlDocNodeVisibility.Public.ExcludeUnbrowsable().IsVisible(unbrowsable), Is.False);
	}

	[Test]
	public void InheritDocResolvesCrefAndInterfaceMembers()
	{
		var tree = TestSupport.CreateTestTree();
		var baseMethod = typeof(InheritDocDerived).GetMethod(nameof(InheritDocDerived.BaseMethod))!;
		var interfaceMethod = typeof(InheritDocDerived).GetMethod(nameof(InheritDocDerived.InterfaceMethod))!;
		var pathMethod = typeof(InheritDocDerived).GetMethod(nameof(InheritDocDerived.PathMethod))!;

		Assert.That(tree.FindNode(XmlDocRef.ForMember(baseMethod))!.XmlMember?.Summary.Single().Inlines.Single().Text, Is.EqualTo("Inherited base summary."));
		Assert.That(tree.FindNode(XmlDocRef.ForMember(interfaceMethod))!.XmlMember?.Summary.Single().Inlines.Single().Text, Is.EqualTo("Inherited interface summary."));
		Assert.That(tree.FindNode(XmlDocRef.ForMember(pathMethod))!.XmlMember?.Summary, Is.Empty);
		Assert.That(tree.FindNode(XmlDocRef.ForMember(pathMethod))!.XmlMember?.Remarks.Single().Inlines.Single().Text, Is.EqualTo("Path-filtered remarks."));
	}

	private sealed class NoMemberVisibility : XmlDocNodeVisibility
	{
		public override bool IsVisible(XmlDocNode node) => Public.IsVisible(node) && node is not XmlDocMemberNode;
	}
}
