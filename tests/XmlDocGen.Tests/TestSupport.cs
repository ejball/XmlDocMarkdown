using System.Reflection;
using ExampleAssembly;
using XmlDocGen.Core.Nodes;
using XmlDocGen.Core.Xml;
using XmlDocGen.Tests.Fixtures;

namespace XmlDocGen.Tests;

internal static class TestSupport
{
	public static XmlDocTree CreateExampleTree()
	{
		var assembly = typeof(ExampleClass).GetTypeInfo().Assembly;
		return XmlDocTree.Create([(assembly, XmlDocXmlFile.Load(Path.ChangeExtension(assembly.Location, ".xml")))]);
	}

	public static XmlDocTree CreateTestTree()
	{
		var assembly = typeof(InheritDocDerived).GetTypeInfo().Assembly;
		return XmlDocTree.Create([(assembly, XmlDocXmlFile.Load(Path.ChangeExtension(assembly.Location, ".xml")))]);
	}
}
