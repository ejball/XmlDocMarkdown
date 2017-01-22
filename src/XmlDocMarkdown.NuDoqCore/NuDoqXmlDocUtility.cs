using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
using NuDoq;
using XmlDocMarkdown.Core;
using NuDoqException = NuDoq.Exception;

namespace XmlDocMarkdown.NuDoqCore
{
	public static class NuDoqXmlDocUtility
	{
		public static XmlDocAssembly CreateXmlDocAssembly(Assembly assembly)
		{
			var instance = new XmlDocAssembly { Info = assembly };
			try
			{
				var documentMembers = DocReader.Read(assembly);
				var visitor = new OurVisitor(instance.Members);
				documentMembers.Accept(visitor);
			}
			catch (FileNotFoundException)
			{
			}
			return instance;
		}

		private sealed class OurVisitor : Visitor
		{
			public OurVisitor(ICollection<XmlDocMember> members)
			{
				m_members = members;
				m_listKinds = new Stack<XmlDocListKind>();
			}

			public override void VisitAssembly(AssemblyMembers assembly)
			{
				m_memberIdMap = assembly.IdMap;
				base.VisitAssembly(assembly);
			}

			public override void VisitMember(Member member)
			{
				m_member = new XmlDocMember { Info = member.Info };

				base.VisitMember(member);

				m_members.Add(m_member);
				m_member = null;
			}

			public override void VisitSummary(Summary summary)
			{
				StartSection();

				base.VisitSummary(summary);

				EndSection(m_member?.Summary);
			}

			public override void VisitTypeParam(TypeParam typeParam)
			{
				StartSection();

				base.VisitTypeParam(typeParam);

				if (m_member != null)
				{
					var typeParameter = new XmlDocParameter { Name = typeParam.Name };
					m_member.TypeParameters.Add(typeParameter);
					EndSection(typeParameter.Description);
				}
			}

			public override void VisitParam(Param param)
			{
				StartSection();

				base.VisitParam(param);

				if (m_member != null)
				{
					var parameter = new XmlDocParameter { Name = param.Name };
					m_member.Parameters.Add(parameter);
					EndSection(parameter.Description);
				}
			}

			public override void VisitReturns(Returns returns)
			{
				StartSection();

				base.VisitReturns(returns);

				EndSection(m_member?.ReturnValue);
			}

			public override void VisitValue(Value value)
			{
				StartSection();

				base.VisitValue(value);

				EndSection(m_member?.PropertyValue);
			}

			public override void VisitRemarks(Remarks remarks)
			{
				StartSection();

				base.VisitRemarks(remarks);

				EndSection(m_member?.Remarks);
			}

			public override void VisitException(NuDoqException exception)
			{
				StartSection();

				base.VisitException(exception);

				if (m_member != null)
				{
					var xmlDocException = new XmlDocException { ExceptionType = m_memberIdMap?.FindMember(exception.Cref) };
					m_member.Exceptions.Add(xmlDocException);
					EndSection(xmlDocException.Condition);
				}
			}

			public override void VisitExample(Example example)
			{
				StartSection();

				base.VisitExample(example);

				EndSection(m_member?.Examples);
			}

			public override void VisitPara(Para para)
			{
				NextBlock();

				base.VisitPara(para);

				NextBlock();
			}

			public override void VisitCode(Code code)
			{
				NextBlock();

				m_block.IsCode = true;
				m_block.Inlines.Add(new XmlDocInline { Text = code.Content });

				NextBlock();
			}

			public override void VisitList(List list)
			{
				m_listKinds.Push(GetListKind(list));
				NextBlock();

				base.VisitList(list);

				m_listKinds.Pop();
				NextBlock();
			}

			public override void VisitListHeader(ListHeader header)
			{
				m_isListHeader = true;
				NextBlock();

				base.VisitListHeader(header);

				NextBlock();
				m_isListHeader = false;
			}

			public override void VisitItem(Item item)
			{
				NextBlock();

				base.VisitItem(item);

				NextBlock();
			}

			public override void VisitTerm(Term term)
			{
				m_isListTerm = true;
				NextBlock();

				base.VisitTerm(term);

				m_isListTerm = false;
				NextBlock();
			}

			public override void VisitDescription(Description description)
			{
				NextBlock();

				base.VisitDescription(description);

				NextBlock();
			}

			public override void VisitText(Text text)
			{
				m_block?.Inlines.Add(new XmlDocInline { Text = text.Content });
			}

			public override void VisitC(C code)
			{
				m_block?.Inlines.Add(new XmlDocInline { Text = code.Content, IsCode = true });
			}

			public override void VisitSee(See see)
			{
				m_block?.Inlines.Add(new XmlDocInline { See = m_memberIdMap?.FindMember(see.Cref) });
			}

			public override void VisitParamRef(ParamRef paramRef)
			{
				m_block?.Inlines.Add(new XmlDocInline { Text = paramRef.Name, IsParamRef = true });
			}

			public override void VisitTypeParamRef(TypeParamRef typeParamRef)
			{
				m_block?.Inlines.Add(new XmlDocInline { Text = typeParamRef.Name, IsTypeParamRef = true });
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

			private void StartSection()
			{
				NextBlock();

				m_blocks = new List<XmlDocBlock>();
			}

			private void EndSection(Collection<XmlDocBlock> blocks)
			{
				NextBlock();

				if (m_blocks != null)
				{
					if (m_member != null)
					{
						foreach (var block in m_blocks)
							blocks.Add(block);
					}
					m_blocks = null;
				}
			}

			private static XmlDocListKind GetListKind(List list)
			{
				switch (list.Type)
				{
				case ListType.Bullet:
					return XmlDocListKind.Bullet;
				case ListType.Number:
					return XmlDocListKind.Number;
				case ListType.Table:
					return XmlDocListKind.Table;
				default:
					return XmlDocListKind.Other;
				}
			}

			private readonly ICollection<XmlDocMember> m_members;
			private XmlDocMember m_member;
			private List<XmlDocBlock> m_blocks;
			private XmlDocBlock m_block;
			private MemberIdMap m_memberIdMap;
			private readonly Stack<XmlDocListKind> m_listKinds;
			private bool m_isListHeader;
			private bool m_isListTerm;
		}
	}
}
