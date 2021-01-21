using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Xml.Linq;

namespace XmlDocMarkdown.Core
{
	internal sealed class XmlDocMember
	{
		public XmlDocMember(XElement xMember)
		{
			XmlDocName = xMember.Attribute("name")?.Value;

			foreach (var xElement in xMember.Elements())
			{
				switch (xElement.Name.LocalName)
				{
					case "summary":
						AddBlocks(xElement, Summary);
						break;
					case "typeparam":
						AddParameter(xElement, TypeParameters);
						break;
					case "param":
						AddParameter(xElement, Parameters);
						break;
					case "returns":
						AddBlocks(xElement, ReturnValue);
						break;
					case "value":
						AddBlocks(xElement, PropertyValue);
						break;
					case "exception":
						AddException(xElement, Exceptions);
						break;
					case "remarks":
						AddBlocks(xElement, Remarks);
						break;
					case "example":
						AddBlocks(xElement, Examples);
						break;
					case "seealso":
						AddSeeAlso(xElement, SeeAlso);
						break;
				}
			}
		}

		public string XmlDocName { get; set; }

		public Collection<XmlDocBlock> Summary { get; } = new();

		public Collection<XmlDocParameter> TypeParameters { get; } = new();

		public Collection<XmlDocParameter> Parameters { get; } = new();

		public Collection<XmlDocBlock> ReturnValue { get; } = new();

		public Collection<XmlDocBlock> PropertyValue { get; } = new();

		public Collection<XmlDocException> Exceptions { get; } = new();

		public Collection<XmlDocBlock> Remarks { get; } = new();

		public Collection<XmlDocBlock> Examples { get; } = new();

		public Collection<XmlDocSeeAlso> SeeAlso { get; } = new();

		public override string ToString() => XmlDocName;

		private static void AddBlocks(XElement xElement, ICollection<XmlDocBlock> blocks)
		{
			var generator = new BlockGenerator();
			generator.AddNodes(xElement.Nodes());
			foreach (var block in generator.GetBlocks())
				blocks.Add(block);
		}

		private static void AddParameter(XElement xElement, ICollection<XmlDocParameter> parameters)
		{
			var parameter = new XmlDocParameter { Name = xElement.Attribute("name")?.Value };
			AddBlocks(xElement, parameter.Description);
			parameters.Add(parameter);
		}

		private static void AddException(XElement xElement, ICollection<XmlDocException> exceptions)
		{
			var exception = new XmlDocException { ExceptionTypeRef = xElement.Attribute("cref")?.Value };
			AddBlocks(xElement, exception.Condition);
			exceptions.Add(exception);
		}

		private void AddSeeAlso(XElement xElement, Collection<XmlDocSeeAlso> seeAlso)
		{
			seeAlso.Add(new XmlDocSeeAlso { Ref = xElement.Attribute("cref")?.Value });
		}

		private sealed class BlockGenerator
		{
			public BlockGenerator()
			{
				m_blocks = new List<XmlDocBlock>();
				m_listKinds = new Stack<XmlDocListKind>();

				NextBlock();
			}

			public void AddNodes(IEnumerable<XNode> xNodes)
			{
				foreach (var xNode in xNodes)
					AddNode(xNode);
			}

			public IReadOnlyList<XmlDocBlock> GetBlocks()
			{
				NextBlock();

				var blocks = m_blocks;
				m_blocks = null;
				return blocks;
			}

			private void AddNode(XNode xNode)
			{
				if (xNode is XElement xElement)
					AddElement(xElement);

				if (xNode is XText xText)
					m_block?.Inlines.Add(new XmlDocInline { Text = xText.Value });
			}

			private void AddElement(XElement xElement)
			{
				switch (xElement.Name.LocalName)
				{
					case "para":
						NextBlock();
						AddNodes(xElement.Nodes());
						NextBlock();
						break;

					case "code":
						NextBlock();
						m_block.IsCode = true;
						m_block.Inlines.Add(new XmlDocInline { Text = TrimCode(xElement.Value) });
						NextBlock();
						break;

					case "list":
						m_listKinds.Push(GetListKind(xElement));
						NextBlock();
						AddNodes(xElement.Nodes());
						m_listKinds.Pop();
						NextBlock();
						break;

					case "listheader":
						m_isListHeader = true;
						NextBlock();
						AddNodes(xElement.Nodes());
						NextBlock();
						m_isListHeader = false;
						break;

					case "item":
						NextBlock();
						AddNodes(xElement.Nodes());
						NextBlock();
						break;

					case "term":
						m_isListTerm = true;
						NextBlock();
						AddNodes(xElement.Nodes());
						m_isListTerm = false;
						NextBlock();
						break;

					case "description":
						NextBlock();
						AddNodes(xElement.Nodes());
						NextBlock();
						break;

					case "c":
						m_block?.Inlines.Add(new XmlDocInline { Text = xElement.Value, IsCode = true });
						break;

					case "see":
						m_block?.Inlines.Add(new XmlDocInline { Text = xElement.Value, SeeRef = xElement.Attribute("cref")?.Value, LinkUrl = xElement.Attribute("href")?.Value, LangWord = xElement.Attribute("langword")?.Value });
						break;

					case "a":
						m_block?.Inlines.Add(new XmlDocInline { Text = xElement.Value, LinkUrl = xElement.Attribute("href")?.Value });
						break;

					case "paramref":
						m_block?.Inlines.Add(new XmlDocInline { Text = (string) xElement.Attribute("name"), IsParamRef = true });
						break;

					case "typeparamref":
						m_block?.Inlines.Add(new XmlDocInline { Text = (string) xElement.Attribute("name"), IsTypeParamRef = true });
						break;

					default:
						AddNodes(xElement.Nodes());
						break;
				}
			}

			private void NextBlock()
			{
				if (m_blocks != null && m_block != null && m_block.Inlines.Count != 0)
					m_blocks.Add(m_block);
				m_block = new XmlDocBlock();

				if (m_listKinds.Count != 0)
				{
					m_block.ListKind = m_listKinds.Peek();
					m_block.ListDepth = m_listKinds.Count - 1;
					m_block.IsListHeader = m_isListHeader;
					m_block.IsListTerm = m_isListTerm;
				}
			}

			private static XmlDocListKind GetListKind(XElement xElement) =>
				xElement.Attribute("type")?.Value.ToLowerInvariant() switch
				{
					"bullet" => XmlDocListKind.Bullet,
					"number" => XmlDocListKind.Number,
					"table" => XmlDocListKind.Table,
					_ => XmlDocListKind.Other,
				};

			private static string TrimCode(string text)
			{
				// trimming logic adapted from https://github.com/kzu/NuDoq
				var lines = text.Split(new[] { Environment.NewLine, "\n" }, StringSplitOptions.None).ToList();

				if (lines.Count != 0 && lines[0].Trim().Length == 0)
					lines.RemoveAt(0);
				if (lines.Count != 0 && lines[lines.Count - 1].Trim().Length == 0)
					lines.RemoveAt(lines.Count - 1);

				if (lines.Count == 0)
					return "";

				var indentLength = lines[0].Length - lines[0].TrimStart().Length;
				if (indentLength <= 4 && lines[0].Length != 0 && lines[0][0] != '\t')
					indentLength = 0;

				text = string.Join(Environment.NewLine, lines.Select(x => x.Length <= indentLength ? "" : x.Substring(indentLength)));

				return text;
			}

			private List<XmlDocBlock> m_blocks;
			private XmlDocBlock m_block;
			private readonly Stack<XmlDocListKind> m_listKinds;
			private bool m_isListHeader;
			private bool m_isListTerm;
		}
	}
}
