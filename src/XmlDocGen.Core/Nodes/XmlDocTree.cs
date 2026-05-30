using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using XmlDocGen.Core.Xml;

namespace XmlDocGen.Core.Nodes;

/// <summary>A documentation tree built from one or more assemblies.</summary>
public sealed class XmlDocTree
{
	private XmlDocTree(IEnumerable<XmlDocAssemblyNode> assemblies)
	{
		Assemblies = [.. assemblies.OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase)];
		m_nodesByRef = Assemblies.SelectMany(x => x.DescendantsAndSelf()).GroupBy(x => x.Ref).ToDictionary(x => x.Key, x => x.First());
		m_nodesByMember = Assemblies.SelectMany(x => x.DescendantsAndSelf()).Where(x => x.MemberInfo is not null).GroupBy(x => x.MemberInfo!).ToDictionary(x => x.Key, x => x.First());
	}

	/// <summary>Creates a tree from assembly nodes.</summary>
	public static XmlDocTree Create(IEnumerable<XmlDocAssemblyNode> assemblies) => new(assemblies);

	/// <summary>Creates a tree from reflected assemblies and XML documentation files.</summary>
	public static XmlDocTree Create(IEnumerable<(Assembly Assembly, XmlDocXmlFile Xml)> inputs) => new(inputs.Select(x => XmlDocAssemblyNode.Create(x.Assembly, x.Xml)));

	/// <summary>Gets the assembly roots.</summary>
	public IReadOnlyList<XmlDocAssemblyNode> Assemblies { get; }

	/// <summary>Finds any node in the tree by reference.</summary>
	public XmlDocNode? FindNode(XmlDocRef reference) => m_nodesByRef.GetValueOrDefault(reference);

	/// <summary>Finds the node documenting a reflection type or member.</summary>
	public XmlDocNode? FindNode(MemberInfo member) => m_nodesByMember.GetValueOrDefault(member);

	/// <summary>Enumerates every node in the tree.</summary>
	public IEnumerable<XmlDocNode> DescendantsAndSelf() => Assemblies.SelectMany(x => x.DescendantsAndSelf());

	/// <summary>Enumerates every visible node in the tree.</summary>
	public IEnumerable<XmlDocNode> DescendantsAndSelf(XmlDocNodeVisibility visibility) => Assemblies.SelectMany(x => x.DescendantsAndSelf(visibility));

	private readonly IReadOnlyDictionary<XmlDocRef, XmlDocNode> m_nodesByRef;
	private readonly IReadOnlyDictionary<MemberInfo, XmlDocNode> m_nodesByMember;
}
