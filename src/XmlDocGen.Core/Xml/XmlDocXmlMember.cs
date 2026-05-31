using System.Collections.ObjectModel;
using System.Xml.Linq;
using System.Xml.XPath;

namespace XmlDocGen.Core.Xml;

/// <summary>The parsed XML documentation for a single member.</summary>
public sealed class XmlDocXmlMember
{
	/// <summary>Gets this member's XML documentation reference.</summary>
	public XmlDocRef Ref { get; }

	/// <summary>Gets the summary blocks.</summary>
	public Collection<XmlDocXmlBlock> Summary { get; } = [];

	/// <summary>Gets the type parameter documentation.</summary>
	public Collection<XmlDocXmlParameter> TypeParameters { get; } = [];

	/// <summary>Gets the parameter documentation.</summary>
	public Collection<XmlDocXmlParameter> Parameters { get; } = [];

	/// <summary>Gets the return-value documentation.</summary>
	public Collection<XmlDocXmlBlock> ReturnValue { get; } = [];

	/// <summary>Gets the property-value documentation.</summary>
	public Collection<XmlDocXmlBlock> PropertyValue { get; } = [];

	/// <summary>Gets the exception documentation.</summary>
	public Collection<XmlDocXmlException> Exceptions { get; } = [];

	/// <summary>Gets the remarks documentation.</summary>
	public Collection<XmlDocXmlBlock> Remarks { get; } = [];

	/// <summary>Gets the example documentation.</summary>
	public Collection<XmlDocXmlBlock> Examples { get; } = [];

	/// <summary>Gets the see-also documentation.</summary>
	public Collection<XmlDocXmlSeeAlso> SeeAlso { get; } = [];

	/// <summary>Gets the raw inheritdoc directive, if present.</summary>
	public XmlDocXmlInheritDoc? InheritDoc { get; }

	internal XmlDocXmlMember(XElement element)
	{
		m_element = new XElement(element);
		Ref = new XmlDocRef(element.Attribute("name")!.Value);

		foreach (var child in element.Elements())
		{
			switch (child.Name.LocalName)
			{
				case "summary":
					AddBlocks(child, Summary);
					break;
				case "typeparam":
					TypeParameters.Add(CreateParameter(child));
					break;
				case "param":
					Parameters.Add(CreateParameter(child));
					break;
				case "returns":
					AddBlocks(child, ReturnValue);
					break;
				case "value":
					AddBlocks(child, PropertyValue);
					break;
				case "exception":
					Exceptions.Add(CreateException(child));
					break;
				case "remarks":
					AddBlocks(child, Remarks);
					break;
				case "example":
					AddBlocks(child, Examples);
					break;
				case "seealso":
					SeeAlso.Add(CreateSeeAlso(child));
					break;
				case "inheritdoc":
					InheritDoc = CreateInheritDoc(child);
					break;
			}
		}
	}

	internal XmlDocXmlMember ApplyInheritDocPath(string? path)
	{
		if (string.IsNullOrWhiteSpace(path))
			return this;

		var selected = SelectElements(path).ToList();
		if (selected.Count == 0)
			return this;

		var element = new XElement("member", new XAttribute("name", Ref.Value));
		element.Add(selected.Select(static x => new XElement(x)));
		return new XmlDocXmlMember(element);
	}

	private static void AddBlocks(XElement element, Collection<XmlDocXmlBlock> blocks)
	{
		var generator = new BlockGenerator();
		generator.AddNodes(element.Nodes());
		foreach (var block in generator.GetBlocks())
			blocks.Add(block);
	}

	private static XmlDocXmlParameter CreateParameter(XElement element)
	{
		var parameter = new XmlDocXmlParameter(element.Attribute("name")?.Value ?? "");
		AddBlocks(element, parameter.Description);
		return parameter;
	}

	private static XmlDocXmlException CreateException(XElement element)
	{
		var exception = new XmlDocXmlException(CreateRef(element.Attribute("cref")?.Value));
		AddBlocks(element, exception.Condition);
		return exception;
	}

	private static XmlDocXmlSeeAlso CreateSeeAlso(XElement element) => new(CreateRef(element.Attribute("cref")?.Value), element.Attribute("href")?.Value, element.Value);

	private static XmlDocXmlInheritDoc CreateInheritDoc(XElement element) => new(CreateRef(element.Attribute("cref")?.Value), element.Attribute("path")?.Value);

	private static XmlDocRef? CreateRef(string? value) => string.IsNullOrWhiteSpace(value) ? null : new XmlDocRef(value);

	private IEnumerable<XElement> SelectElements(string path)
	{
		var normalizedPath = path.Length != 0 && path[0] == '/' ? "." + path : path;
		return m_element.XPathSelectElements(normalizedPath);
	}

	private readonly XElement m_element;

	private sealed class BlockGenerator
	{
		public void AddNodes(IEnumerable<XNode> nodes)
		{
			foreach (var node in nodes)
				AddNode(node);
		}

		public List<XmlDocXmlBlock> GetBlocks()
		{
			NextBlock();
			return m_blocks;
		}

		private void AddNode(XNode node)
		{
			if (node is XText text)
				m_current.Inlines.Add(new XmlDocXmlInline(XmlDocXmlInlineKind.Text, text.Value));
			else if (node is XElement element)
				AddElement(element);
		}

		private void AddElement(XElement element)
		{
			switch (element.Name.LocalName)
			{
				case "para":
					NextBlock();
					AddNodes(element.Nodes());
					NextBlock();
					break;
				case "code":
					NextBlock();
					m_current.IsCode = true;
					m_current.Language = element.Attribute("lang")?.Value;
					m_current.Inlines.Add(new XmlDocXmlInline(XmlDocXmlInlineKind.Text, TrimCode(element.Value)));
					NextBlock();
					break;
				case "list":
					m_listKinds.Push(GetListKind(element));
					NextBlock();
					AddNodes(element.Nodes());
					m_listKinds.Pop();
					NextBlock();
					break;
				case "listheader":
					m_isListHeader = true;
					NextBlock();
					AddNodes(element.Nodes());
					NextBlock();
					m_isListHeader = false;
					break;
				case "item":
				case "description":
					NextBlock();
					AddNodes(element.Nodes());
					NextBlock();
					break;
				case "term":
					m_isListTerm = true;
					NextBlock();
					AddNodes(element.Nodes());
					m_isListTerm = false;
					NextBlock();
					break;
				case "c":
					m_current.Inlines.Add(new XmlDocXmlInline(XmlDocXmlInlineKind.Code, element.Value));
					break;
				case "see":
					m_current.Inlines.Add(CreateSeeInline(element));
					break;
				case "a":
					m_current.Inlines.Add(new XmlDocXmlInline(XmlDocXmlInlineKind.SeeHref, element.Value) { Href = element.Attribute("href")?.Value });
					break;
				case "paramref":
					m_current.Inlines.Add(new XmlDocXmlInline(XmlDocXmlInlineKind.ParamRef, element.Attribute("name")?.Value ?? ""));
					break;
				case "typeparamref":
					m_current.Inlines.Add(new XmlDocXmlInline(XmlDocXmlInlineKind.TypeParamRef, element.Attribute("name")?.Value ?? ""));
					break;
				default:
					AddNodes(element.Nodes());
					break;
			}
		}

		private void NextBlock()
		{
			if (m_current.Inlines.Count != 0)
				m_blocks.Add(m_current);

			m_current = new XmlDocXmlBlock();
			if (m_listKinds.Count != 0)
			{
				m_current.ListKind = m_listKinds.Peek();
				m_current.ListDepth = m_listKinds.Count - 1;
				m_current.IsListHeader = m_isListHeader;
				m_current.IsListTerm = m_isListTerm;
			}
		}

		private static XmlDocXmlInline CreateSeeInline(XElement element)
		{
			if (element.Attribute("cref")?.Value is { } cref)
				return new XmlDocXmlInline(XmlDocXmlInlineKind.SeeCref, element.Value) { Ref = new XmlDocRef(cref) };
			if (element.Attribute("href")?.Value is { } href)
				return new XmlDocXmlInline(XmlDocXmlInlineKind.SeeHref, element.Value) { Href = href };
			if (element.Attribute("langword")?.Value is { } langword)
				return new XmlDocXmlInline(XmlDocXmlInlineKind.SeeLangword, element.Value) { Langword = langword };

			return new XmlDocXmlInline(XmlDocXmlInlineKind.Text, element.Value);
		}

		private static XmlDocXmlListKind GetListKind(XElement element) => element.Attribute("type")?.Value.ToLowerInvariant() switch
		{
			"bullet" => XmlDocXmlListKind.Bullet,
			"number" => XmlDocXmlListKind.Number,
			"table" => XmlDocXmlListKind.Table,
			"definition" => XmlDocXmlListKind.Definition,
			_ => XmlDocXmlListKind.Bullet,
		};

		private static string TrimCode(string text)
		{
			var lines = text.Split([Environment.NewLine, "\n"], StringSplitOptions.None).ToList();
			if (lines.Count != 0 && lines[0].Trim().Length == 0)
				lines.RemoveAt(0);
			if (lines.Count != 0 && lines[^1].Trim().Length == 0)
				lines.RemoveAt(lines.Count - 1);
			if (lines.Count == 0)
				return "";

			var indentLength = lines[0].Length - lines[0].TrimStart().Length;
			if (indentLength <= 4 && lines[0].Length != 0 && lines[0][0] != '\t')
				indentLength = 0;

			return string.Join(Environment.NewLine, lines.Select(x => x.Length <= indentLength ? "" : x[indentLength..]));
		}

		private readonly Stack<XmlDocXmlListKind> m_listKinds = [];
		private readonly List<XmlDocXmlBlock> m_blocks = [];
		private XmlDocXmlBlock m_current = new();
		private bool m_isListHeader;
		private bool m_isListTerm;
	}
}
