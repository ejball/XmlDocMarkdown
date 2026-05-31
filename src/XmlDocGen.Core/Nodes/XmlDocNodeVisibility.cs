using System.ComponentModel;

namespace XmlDocGen.Core.Nodes;

/// <summary>A composable node-visibility filter.</summary>
public abstract class XmlDocNodeVisibility
{
	/// <summary>Gets a filter that includes public nodes.</summary>
	public static XmlDocNodeVisibility Public { get; } = Create(XmlDocVisibility.Public);

	/// <summary>Gets a filter that includes public and protected nodes.</summary>
	public static XmlDocNodeVisibility Protected { get; } = Create(XmlDocVisibility.Protected);

	/// <summary>Gets a filter that includes public, protected, and internal nodes.</summary>
	public static XmlDocNodeVisibility Internal { get; } = Create(XmlDocVisibility.Internal);

	/// <summary>Gets a filter that includes all nodes.</summary>
	public static XmlDocNodeVisibility Private { get; } = Create(XmlDocVisibility.Private);

	/// <summary>Creates a minimum-visibility filter.</summary>
	public static XmlDocNodeVisibility Create(XmlDocVisibility minimum) => new PredicateVisibility(node => node is XmlDocAssemblyNode or XmlDocNamespaceNode || (int) node.Visibility >= (int) minimum);

	/// <summary>Creates a custom predicate filter.</summary>
	public static XmlDocNodeVisibility Create(Func<XmlDocNode, bool> predicate) => new PredicateVisibility(predicate);

	/// <summary>Combines this filter with another filter.</summary>
	public XmlDocNodeVisibility And(XmlDocNodeVisibility other) => new PredicateVisibility(node => IsVisible(node) && other.IsVisible(node));

	/// <summary>Excludes obsolete nodes.</summary>
	public XmlDocNodeVisibility ExcludeObsolete() => Exclude(node => node.IsObsolete);

	/// <summary>Excludes nodes marked with <see cref="EditorBrowsableState.Never"/>.</summary>
	public XmlDocNodeVisibility ExcludeUnbrowsable() => Exclude(node => !node.IsBrowsable);

	/// <summary>Excludes compiler-generated nodes.</summary>
	public XmlDocNodeVisibility ExcludeCompilerGenerated() => Exclude(node => node.IsCompilerGenerated);

	/// <summary>Excludes nodes matching a predicate.</summary>
	public XmlDocNodeVisibility Exclude(Func<XmlDocNode, bool> shouldExclude) => new PredicateVisibility(node => IsVisible(node) && !shouldExclude(node));

	/// <summary>Returns true if the node is included.</summary>
	public abstract bool IsVisible(XmlDocNode node);

	/// <summary>Returns true if the node is included.</summary>
	public bool Includes(XmlDocNode node) => IsVisible(node);

	private sealed class PredicateVisibility(Func<XmlDocNode, bool> predicate) : XmlDocNodeVisibility
	{
		public override bool IsVisible(XmlDocNode node) => predicate(node);
	}
}
