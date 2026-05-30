using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using XmlDocGen.Core.Xml;

namespace XmlDocGen.Core.Nodes;

/// <summary>An assembly documentation node.</summary>
public sealed class XmlDocAssemblyNode : XmlDocNode
{
	private XmlDocAssemblyNode(Assembly assembly, XmlDocXmlFile xml)
		: base(null, null)
	{
		ReflectionAssembly = assembly;
		Xml = xml;
		Name = assembly.GetName().Name ?? assembly.FullName ?? "Assembly";
		Ref = new XmlDocRef("A:" + Name);

		var namespaces = assembly.DefinedTypes.Where(IsDocumentableType).GroupBy(x => x.Namespace ?? "global").OrderBy(x => x.Key, StringComparer.OrdinalIgnoreCase);
		foreach (var group in namespaces)
			AddChild(new XmlDocNamespaceNode(this, group.Key, [.. group.OrderBy(x => x.FullName, StringComparer.OrdinalIgnoreCase)]));
		Namespaces = [.. Children.OfType<XmlDocNamespaceNode>()];
	}

	/// <summary>Creates an assembly node.</summary>
	public static XmlDocAssemblyNode Create(Assembly assembly, XmlDocXmlFile xml) => new(assembly, xml);

	/// <inheritdoc />
	public override string Name { get; }

	/// <inheritdoc />
	public override XmlDocRef Ref { get; }

	/// <summary>Gets the reflected assembly.</summary>
	public Assembly ReflectionAssembly { get; }

	/// <summary>Gets the XML documentation file associated with the assembly.</summary>
	public XmlDocXmlFile Xml { get; }

	/// <summary>Gets the XML documentation file associated with the assembly.</summary>
	public XmlDocXmlFile XmlFile => Xml;

	/// <summary>Gets namespace nodes in this assembly.</summary>
	public IReadOnlyList<XmlDocNamespaceNode> Namespaces { get; }

	/// <inheritdoc />
	public override XmlDocVisibility Visibility => XmlDocVisibility.Public;

	private static bool IsDocumentableType(TypeInfo type) => type.Name.Length != 0 && type.Name[0] != '<' && !type.GetCustomAttributes<CompilerGeneratedAttribute>().Any() && type.DeclaringType is null;
}
