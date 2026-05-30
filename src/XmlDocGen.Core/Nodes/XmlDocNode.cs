using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using XmlDocGen.Core.Xml;

namespace XmlDocGen.Core.Nodes;

/// <summary>Base class for an assembly, namespace, type, or member documentation node.</summary>
public abstract class XmlDocNode
{
	private protected XmlDocNode(XmlDocNode? parent, XmlDocXmlMember? xml)
	{
		Parent = parent;
		XmlMember = xml;
		if (parent is null && this is XmlDocAssemblyNode assembly)
			Assembly = assembly;
		else
			Assembly = parent?.Assembly ?? throw new InvalidOperationException("Non-assembly nodes need a parent.");
	}

	/// <summary>Gets the simple display name.</summary>
	public abstract string Name { get; }

	/// <summary>Gets this node's XML documentation reference.</summary>
	public abstract XmlDocRef Ref { get; }

	/// <summary>Gets the parent node, or null for an assembly root.</summary>
	public XmlDocNode? Parent { get; }

	/// <summary>Gets the owning assembly node.</summary>
	public XmlDocAssemblyNode Assembly { get; }

	/// <summary>Gets child nodes.</summary>
	public IReadOnlyList<XmlDocNode> Children => m_children;

	/// <summary>Gets the associated XML documentation, if any.</summary>
	public XmlDocXmlMember? XmlMember { get; }

	/// <summary>Gets a value indicating whether this node is obsolete.</summary>
	public bool IsObsolete => MemberInfo?.GetCustomAttributes<ObsoleteAttribute>().Any() == true;

	/// <summary>Gets a value indicating whether this node is browsable.</summary>
	public bool IsBrowsable => MemberInfo?.GetCustomAttributes<EditorBrowsableAttribute>().Any(x => x.State == EditorBrowsableState.Never) != true;

	/// <summary>Gets a value indicating whether this node is compiler-generated.</summary>
	public bool IsCompilerGenerated => MemberInfo?.GetCustomAttributes<CompilerGeneratedAttribute>().Any() == true;

	/// <summary>Gets the exact visibility of this node.</summary>
	public abstract XmlDocVisibility Visibility { get; }

	/// <summary>Gets the reflected member associated with this node.</summary>
	public virtual MemberInfo? MemberInfo => null;

	/// <summary>Gets visible immediate children.</summary>
	public IEnumerable<XmlDocNode> GetChildren(XmlDocNodeVisibility visibility) => Children.Where(visibility.IsVisible);

	/// <summary>Enumerates this node and every descendant.</summary>
	public IEnumerable<XmlDocNode> DescendantsAndSelf()
	{
		yield return this;
		foreach (var child in Children.SelectMany(x => x.DescendantsAndSelf()))
			yield return child;
	}

	/// <summary>Enumerates this node and visible descendants.</summary>
	public IEnumerable<XmlDocNode> DescendantsAndSelf(XmlDocNodeVisibility visibility)
	{
		if (visibility.IsVisible(this))
			yield return this;
		foreach (var child in Children.SelectMany(x => x.DescendantsAndSelf(visibility)))
			yield return child;
	}

	/// <summary>Surfaces an attribute applied to this node, if present.</summary>
	public bool TryGetAttribute<T>([NotNullWhen(true)] out T? attribute)
		where T : Attribute
	{
		attribute = MemberInfo?.GetCustomAttributes<T>().FirstOrDefault();
		return attribute is not null;
	}

	private protected void AddChild(XmlDocNode child) => m_children.Add(child);

	private readonly Collection<XmlDocNode> m_children = [];
}
