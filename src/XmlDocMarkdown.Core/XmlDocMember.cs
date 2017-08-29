using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace XmlDocMarkdown.Core
{
	public sealed class XmlDocMember
	{
		public XmlDocMember(XElement xMember)
		{
			XmlDocName = xMember.Attribute("name").Value;

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

		public Collection<XmlDocBlock> Summary { get; } = new Collection<XmlDocBlock>();

		public Collection<XmlDocParameter> TypeParameters { get; } = new Collection<XmlDocParameter>();

		public Collection<XmlDocParameter> Parameters { get; } = new Collection<XmlDocParameter>();

		public Collection<XmlDocBlock> ReturnValue { get; } = new Collection<XmlDocBlock>();

		public Collection<XmlDocBlock> PropertyValue { get; } = new Collection<XmlDocBlock>();

		public Collection<XmlDocException> Exceptions { get; } = new Collection<XmlDocException>();

		public Collection<XmlDocBlock> Remarks { get; } = new Collection<XmlDocBlock>();

		public Collection<XmlDocBlock> Examples { get; } = new Collection<XmlDocBlock>();

		public Collection<XmlDocSeeAlso> SeeAlso { get; } = new Collection<XmlDocSeeAlso>();

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
				var xElement = xNode as XElement;
				if (xElement != null)
					AddElement(xElement);

				var xText = xNode as XText;
				if (xText != null)
					m_block?.Inlines.Add(new XmlDocInline { Text = TrimText(xText.Value) });
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
					m_block.Inlines.Add(new XmlDocInline { Text = TrimText(xElement.Value, isCode: true) });
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
					m_block?.Inlines.Add(new XmlDocInline { Text = xElement.Value, SeeRef = xElement.Attribute("cref")?.Value });
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

			private static XmlDocListKind GetListKind(XElement xElement)
			{
				switch (xElement.Attribute("type")?.Value?.ToLowerInvariant())
				{
				case "bullet":
					return XmlDocListKind.Bullet;
				case "number":
					return XmlDocListKind.Number;
				case "table":
					return XmlDocListKind.Table;
				default:
					return XmlDocListKind.Other;
				}
			}

			private static string TrimText(string text, bool isCode = false)
			{
				// trimming logic adapted from https://github.com/kzu/NuDoq
				var lines = text.Split(new[] { Environment.NewLine, "\n" }, isCode ? StringSplitOptions.None : StringSplitOptions.RemoveEmptyEntries).ToList();

				if (lines.Count != 0 && lines[0].Trim().Length == 0)
					lines.RemoveAt(0);
				if (lines.Count != 0 && lines[lines.Count - 1].Trim().Length == 0)
					lines.RemoveAt(lines.Count - 1);

				if (lines.Count == 0)
					return "";

				int indentLength = lines[0].Length - lines[0].TrimStart().Length;
				if (indentLength <= 4 && lines[0].Length != 0 && lines[0][0] != '\t')
					indentLength = 0;

				text = string.Join(isCode ? Environment.NewLine : " ", lines.Select(x => x.Length <= indentLength ? "" : x.Substring(indentLength)));

				if (!isCode)
					text = Regex.Replace(text, @"\s+", " ");

				return text;
			}

			List<XmlDocBlock> m_blocks;
			XmlDocBlock m_block;
			readonly Stack<XmlDocListKind> m_listKinds;
			bool m_isListHeader;
			bool m_isListTerm;
		}
	}
}
