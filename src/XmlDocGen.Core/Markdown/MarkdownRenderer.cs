using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using XmlDocGen.Core.CSharp;
using XmlDocGen.Core.Nodes;
using XmlDocGen.Core.Pages;
using XmlDocGen.Core.Xml;

namespace XmlDocGen.Core.Markdown;

/// <summary>Renders node and XML content as Markdown building blocks.</summary>
public class MarkdownRenderer
{
	/// <summary>Writes a C# signature block.</summary>
	public virtual void WriteSignature(MarkdownWriter writer, XmlDocNode node, XmlDocPageContext context)
	{
		writer.WriteLine("```csharp");
		writer.WriteLine(CSharpSignatureBuilder.Full.GetSignature(node).Text);
		writer.WriteLine("```");
	}

	/// <summary>Writes a summary section.</summary>
	public virtual void WriteSummary(MarkdownWriter writer, XmlDocNode node, XmlDocPageContext context) => WriteBlocks(writer, node.XmlMember?.Summary, context, node);

	/// <summary>Writes a remarks section.</summary>
	public virtual void WriteRemarks(MarkdownWriter writer, XmlDocNode node, XmlDocPageContext context)
	{
		if (node.XmlMember?.Remarks.Count > 0)
		{
			writer.WriteLine();
			writer.WriteHeading(2, "Remarks");
			writer.WriteLine();
			WriteBlocks(writer, node.XmlMember.Remarks, context, node);
		}
	}

	/// <summary>Writes a parameter section.</summary>
	public virtual void WriteParameters(MarkdownWriter writer, XmlDocMemberNode member, XmlDocPageContext context)
	{
		var typeParameters = member.XmlMember?.TypeParameters ?? [];
		var parameters = member.XmlMember?.Parameters ?? [];
		if (typeParameters.Count + parameters.Count == 0)
			return;

		if (typeParameters.Count > 0)
			WriteParameterTable(writer, member, context, "Type Parameters", "type parameter", "type-parameter", typeParameters);
		if (parameters.Count > 0)
			WriteParameterTable(writer, member, context, "Parameters", "parameter", "parameter", parameters);
	}

	/// <summary>Writes a return-value section.</summary>
	public virtual void WriteReturnValue(MarkdownWriter writer, XmlDocMemberNode member, XmlDocPageContext context)
	{
		if (member.XmlMember?.ReturnValue.Count > 0)
		{
			writer.WriteLine();
			writer.WriteHeading(2, "Returns");
			writer.WriteLine();
			WriteBlocks(writer, member.XmlMember.ReturnValue, context, member);
		}
	}

	/// <summary>Writes a property-value section.</summary>
	public virtual void WritePropertyValue(MarkdownWriter writer, XmlDocMemberNode member, XmlDocPageContext context)
	{
		if (member.XmlMember?.PropertyValue.Count > 0)
		{
			writer.WriteLine();
			writer.WriteHeading(2, "Property Value");
			writer.WriteLine();
			WriteBlocks(writer, member.XmlMember.PropertyValue, context, member);
		}
	}

	/// <summary>Writes an exception section.</summary>
	public virtual void WriteExceptions(MarkdownWriter writer, XmlDocMemberNode member, XmlDocPageContext context)
	{
		if (member.XmlMember?.Exceptions.Count > 0)
		{
			writer.WriteLine();
			writer.WriteHeading(2, "Exceptions");
			writer.WriteLine();
			writer.WriteTableRow("exception", "condition");
			writer.WriteLine("| --- | --- |");
			foreach (var exception in member.XmlMember.Exceptions)
			{
				var name = exception.ExceptionTypeRef is null ? "" : exception.ExceptionTypeRef.Value.ShortName;
				if (exception.ExceptionTypeRef is { } reference && context.GetLinkUrl(reference) is { } url)
					name = $"[{name}]({url})";
				writer.WriteMarkdownTableRow(name, RenderBlocksInline(exception.Condition, context, member));
			}
		}
	}

	/// <summary>Writes an examples section.</summary>
	public virtual void WriteExamples(MarkdownWriter writer, XmlDocNode node, XmlDocPageContext context)
	{
		if (node.XmlMember?.Examples.Count > 0)
		{
			writer.WriteLine();
			writer.WriteHeading(2, "Examples");
			writer.WriteLine();
			WriteBlocks(writer, node.XmlMember.Examples, context, node);
		}
	}

	/// <summary>Writes a child-node overview section.</summary>
	public virtual void WriteChildren(MarkdownWriter writer, XmlDocNode node, XmlDocPageContext context)
	{
		if (node is XmlDocAssemblyNode)
			return;

		var children = node.Children.Where(x => context.FindPage(x) is not null).OrderBy(GetOverviewText, StringComparer.OrdinalIgnoreCase).ToList();
		if (children.Count == 0)
			return;

		writer.WriteLine();
		if (node is not XmlDocNamespaceNode)
		{
			writer.WriteHeading(2, GetChildrenHeading(node));
			writer.WriteLine();
		}
		writer.WriteTableRow(GetChildrenNameHeader(node), "description");
		writer.WriteLine("| --- | --- |");
		foreach (var group in children.GroupBy(x => context.FindPage(x)!))
		{
			var child = group.First();
			var page = group.Key;
			var url = context.UrlMapper.GetUrl(context.Page, page, child);
			var summary = RenderBlocksInline(child.XmlMember?.Summary ?? [], context, child);
			if (group.Count() > 1)
				summary += $" ({group.Count()} {GetPluralKindName(child)})";
			writer.WriteMarkdownTableRow(RenderOverviewLink(child, url), summary);
		}
	}

	/// <summary>Writes a see-also section.</summary>
	public virtual void WriteSeeAlso(MarkdownWriter writer, XmlDocNode node, XmlDocPageContext context)
	{
		var links = GetSeeAlsoLinks(node, context).ToList();
		if (links.Count > 0)
		{
			writer.WriteLine();
			writer.WriteHeading(2, "See Also");
			writer.WriteLine();
			foreach (var (text, url) in links)
				writer.WriteLine(url is null ? "* " + Escape(text) : $"* [{Escape(text)}]({url})");
		}
	}

	/// <summary>Writes inline XML documentation content.</summary>
	public virtual void WriteInlines(MarkdownWriter writer, IEnumerable<XmlDocXmlInline> inlines, XmlDocPageContext context) => writer.Write(RenderInlines(inlines, context, currentNode: null));

	/// <summary>Writes blocks of XML documentation content.</summary>
	protected void WriteBlocks(MarkdownWriter writer, IEnumerable<XmlDocXmlBlock>? blocks, XmlDocPageContext context) => WriteBlocks(writer, blocks, context, currentNode: null);

	/// <summary>Writes blocks of XML documentation content.</summary>
	protected void WriteBlocks(MarkdownWriter writer, IEnumerable<XmlDocXmlBlock>? blocks, XmlDocPageContext context, XmlDocNode? currentNode)
	{
		if (blocks is null)
			return;

		var blockList = blocks.ToList();
		for (var index = 0; index < blockList.Count; index++)
		{
			var block = blockList[index];
			if (index != 0)
				writer.WriteLine();

			if (block.IsCode)
			{
				writer.WriteLine("```" + (block.Language ?? "csharp"));
				foreach (var inline in block.Inlines)
					writer.WriteLine(inline.Text ?? "");
				writer.WriteLine("```");
			}
			else if (block.ListKind == XmlDocXmlListKind.Table)
			{
				index = WriteTableList(writer, blockList, index, context, currentNode);
			}
			else if (block.ListKind == XmlDocXmlListKind.Definition)
			{
				index = WriteDefinitionList(writer, blockList, index, context, currentNode);
			}
			else if (block.ListKind is XmlDocXmlListKind.Bullet or XmlDocXmlListKind.Number)
			{
				var prefix = block.ListKind == XmlDocXmlListKind.Number ? "1. " : "* ";
				writer.WriteLine(new string(' ', block.ListDepth * 2) + prefix + RenderInlines(block.Inlines, context, currentNode));
			}
			else
			{
				writer.WriteLine(RenderInlines(block.Inlines, context, currentNode));
			}
		}
	}

	private static void WriteParameterTable(MarkdownWriter writer, XmlDocMemberNode member, XmlDocPageContext context, string heading, string columnName, string anchorPrefix, IEnumerable<XmlDocXmlParameter> parameters)
	{
		writer.WriteLine();
		writer.WriteHeading(2, heading);
		writer.WriteLine();
		writer.WriteTableRow(columnName, "description");
		writer.WriteLine("| --- | --- |");
		foreach (var parameter in parameters)
		{
			var name = $"<a id=\"{CreateParameterAnchor(anchorPrefix, parameter.Name)}\"></a>`{Escape(parameter.Name)}`";
			writer.WriteMarkdownTableRow(name, RenderBlocksInline(parameter.Description, context, member));
		}
	}

	private static int WriteTableList(MarkdownWriter writer, List<XmlDocXmlBlock> blocks, int startIndex, XmlDocPageContext context, XmlDocNode? currentNode)
	{
		var depth = blocks[startIndex].ListDepth;
		var tableBlocks = ReadListBlocks(blocks, startIndex, XmlDocXmlListKind.Table, depth, out var endIndex);
		var header = tableBlocks.Where(static x => x.IsListHeader).Select(x => RenderInlines(x.Inlines, context, currentNode)).ToList();
		if (header.Count == 0)
			header = ["term", "description"];
		writer.WriteMarkdownTableRow([.. header]);
		writer.WriteLine("| " + string.Join(" | ", header.Select(static _ => "---")) + " |");

		var cells = tableBlocks.Where(static x => !x.IsListHeader).Select(x => RenderInlines(x.Inlines, context, currentNode)).ToList();
		for (var index = 0; index < cells.Count; index += header.Count)
			writer.WriteMarkdownTableRow([.. PadCells(cells.Skip(index).Take(header.Count).ToList(), header.Count)]);
		return endIndex;
	}

	private static List<XmlDocXmlBlock> ReadListBlocks(List<XmlDocXmlBlock> blocks, int startIndex, XmlDocXmlListKind listKind, int depth, out int endIndex)
	{
		var listBlocks = new List<XmlDocXmlBlock>();
		endIndex = startIndex;
		for (var index = startIndex; index < blocks.Count; index++)
		{
			var block = blocks[index];
			if (block.ListKind != listKind || block.ListDepth != depth)
				break;

			listBlocks.Add(block);
			endIndex = index;
		}
		return listBlocks;
	}

	private static int WriteDefinitionList(MarkdownWriter writer, List<XmlDocXmlBlock> blocks, int startIndex, XmlDocPageContext context, XmlDocNode? currentNode)
	{
		var depth = blocks[startIndex].ListDepth;
		var rows = ReadListRows(blocks, startIndex, XmlDocXmlListKind.Definition, depth, context, currentNode, out var endIndex).Where(static x => !x.IsHeader).ToList();
		foreach (var (row, index) in rows.Select(static (row, index) => (row, index)))
		{
			if (index != 0)
				writer.WriteLine();
			var term = row.Cells.Count > 0 ? row.Cells[0] : "";
			var description = row.Cells.Count > 1 ? row.Cells[1] : "";
			writer.WriteLine("* **" + term + "**" + (description.Length == 0 ? "" : ": " + description));
		}
		return endIndex;
	}

	private static List<XmlDocListRow> ReadListRows(List<XmlDocXmlBlock> blocks, int startIndex, XmlDocXmlListKind listKind, int depth, XmlDocPageContext context, XmlDocNode? currentNode, out int endIndex)
	{
		var rows = new List<XmlDocListRow>();
		var cells = new List<string>();
		var isHeader = blocks[startIndex].IsListHeader;
		endIndex = startIndex;
		for (var index = startIndex; index < blocks.Count; index++)
		{
			var block = blocks[index];
			if (block.ListKind != listKind || block.ListDepth != depth)
				break;

			if ((block.IsListTerm || block.IsListHeader != isHeader) && cells.Count != 0)
			{
				rows.Add(new XmlDocListRow(isHeader, [.. cells]));
				cells.Clear();
				isHeader = block.IsListHeader;
			}
			cells.Add(RenderInlines(block.Inlines, context, currentNode));
			endIndex = index;
		}
		if (cells.Count != 0)
			rows.Add(new XmlDocListRow(isHeader, [.. cells]));
		return rows;
	}

	private static IEnumerable<string> PadCells(List<string> cells, int count)
	{
		for (var index = 0; index < count; index++)
			yield return index < cells.Count ? cells[index] : "";
	}

	private static string RenderBlocksInline(IEnumerable<XmlDocXmlBlock> blocks, XmlDocPageContext context, XmlDocNode? currentNode) => string.Join(" ", blocks.Select(x => RenderInlines(x.Inlines, context, currentNode)));

	private static string GetChildrenHeading(XmlDocNode node) => node switch
	{
		XmlDocTypeNode => "Public Members",
		_ => "Children",
	};

	private static string GetChildrenNameHeader(XmlDocNode node) => node is XmlDocNamespaceNode ? "public type" : "name";

	private static string GetPluralKindName(XmlDocNode node) => node switch
	{
		XmlDocMemberNode { MemberKind: XmlDocMemberKind.Property } => "properties",
		XmlDocMemberNode { MemberKind: XmlDocMemberKind.Constructor } => "constructors",
		XmlDocMemberNode member => member.MemberKind.ToString().ToLowerInvariant() + "s",
		XmlDocTypeNode type => type.Kind.ToString().ToLowerInvariant() + "s",
		_ => "items",
	};

	private static string GetOverviewText(XmlDocNode node) => node is XmlDocMemberNode memberNode ? GetMemberOverviewText(memberNode) : CSharpSignatureBuilder.Short.GetSignature(node).Text;

	private static string RenderOverviewLink(XmlDocNode node, string url)
	{
		return node switch
		{
			XmlDocTypeNode type => GetTypePrefix(type) + Link(GetTypeDisplayName(type.TypeInfo), url),
			XmlDocMemberNode member => RenderMemberOverviewLink(member, url),
			_ => Link(GetOverviewText(node), url),
		};
	}

	private static string RenderMemberOverviewLink(XmlDocMemberNode node, string url)
	{
		var (name, suffix) = GetMemberNameAndSuffix(node);
		return GetMemberPrefix(node) + Link(name, url) + suffix;
	}

	private static string Link(string text, string url) => $"[{Escape(text)}]({url})";

	private static string GetTypePrefix(XmlDocTypeNode node)
	{
		var parts = new List<string>();
		if (node.TypeInfo.GetCustomAttribute<FlagsAttribute>() is not null)
			parts.Add("[Flags]");
		if (ReflectionFacts.IsStatic(node.TypeInfo))
			parts.Add("static");
		else if (node.TypeInfo is { IsClass: true, IsAbstract: true })
			parts.Add("abstract");
		if (node.IsReadOnly)
			parts.Add("readonly");
		if (node.IsRefStruct)
			parts.Add("ref");
		parts.Add(GetTypeKindText(node.Kind));
		return string.Join(" ", parts) + " ";
	}

	private static string GetTypeKindText(XmlDocTypeKind kind) => kind switch
	{
		XmlDocTypeKind.RecordStruct => "record struct",
		_ => kind.ToString().ToLowerInvariant(),
	};

	private static string GetTypeDisplayName(TypeInfo type)
	{
		var name = ReflectionFacts.GetShortName(type);
		return type.GenericTypeParameters.Length == 0 ? name : name + "<" + string.Join(',', type.GenericTypeParameters.Select(x => x.Name)) + ">";
	}

	private static string GetMemberOverviewText(XmlDocMemberNode node)
	{
		var (name, suffix) = GetMemberNameAndSuffix(node);
		return GetMemberPrefix(node) + name + suffix;
	}

	private static (string Name, string Suffix) GetMemberNameAndSuffix(XmlDocMemberNode node)
	{
		return node.Member switch
		{
			ConstructorInfo constructor => (node.Name, GetParameterSuffix(constructor.GetParameters())),
			MethodInfo method => GetMethodNameAndSuffix(node, method),
			PropertyInfo property => (node.Name, GetPropertySuffix(property)),
			_ => (node.Name, ""),
		};
	}

	private static (string Name, string Suffix) GetMethodNameAndSuffix(XmlDocMemberNode node, MethodInfo method)
	{
		var signature = CSharpSignatureBuilder.Short.GetSignature(node).Text;
		var parameterIndex = signature.IndexOf('(', StringComparison.Ordinal);
		if (parameterIndex != -1)
			return (signature[..parameterIndex], GetParameterSuffix(method.GetParameters()));
		return (signature, "");
	}

	private static string GetPropertySuffix(PropertyInfo property) => " " + GetPropertyAccessors(property);

	private static string GetParameterSuffix(ParameterInfo[] parameters) => parameters.Length == 0 ? "()" : "(...)";

	private static string GetPropertyAccessors(PropertyInfo property)
	{
		var get = property.GetMethod is not null;
		var set = property.SetMethod is not null;
		var setName = property.SetMethod?.ReturnParameter.GetRequiredCustomModifiers().Any(x => x.FullName == "System.Runtime.CompilerServices.IsExternalInit") == true ? "init" : "set";
		return (get, set) switch
		{
			(true, true) => "{ get; " + setName + "; }",
			(true, false) => "{ get; }",
			(false, true) => "{ " + setName + "; }",
			_ => "{ }",
		};
	}

	private static string GetMemberPrefix(XmlDocMemberNode node)
	{
		var parts = new List<string>();
		if (IsOverride(node.Member))
			parts.Add("override");
		else if (ReflectionFacts.IsStatic(node.Member))
			parts.Add("static");
		else if (ReflectionFacts.IsVirtual(node.Member))
			parts.Add("virtual");

		if (node.Member is FieldInfo { IsLiteral: true })
			parts.Add("const");
		else if (node.Member is FieldInfo { IsInitOnly: true })
			parts.Add("readonly");
		else if (node.Member is EventInfo)
			parts.Add("event");

		return parts.Count == 0 ? "" : string.Join(" ", parts) + " ";
	}

	private static bool IsOverride(MemberInfo member) => member switch
	{
		MethodInfo method => method.GetBaseDefinition() != method && method.GetBaseDefinition().DeclaringType != method.DeclaringType,
		PropertyInfo property => IsOverride(property.GetMethod) || IsOverride(property.SetMethod),
		EventInfo @event => IsOverride(@event.AddMethod) || IsOverride(@event.RemoveMethod),
		_ => false,
	};

	private static bool IsOverride(MethodInfo? method) => method is not null && method.GetBaseDefinition() != method && method.GetBaseDefinition().DeclaringType != method.DeclaringType;

	private static IEnumerable<(string Text, string? Url)> GetSeeAlsoLinks(XmlDocNode node, XmlDocPageContext context)
	{
		var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		foreach (var link in GetExplicitSeeAlsoLinks(node, context).Concat(GetAutomaticSeeAlsoLinks(node, context)))
		{
			var key = link.Text + "\n" + link.Url;
			if (seen.Add(key))
				yield return link;
		}
	}

	private static IEnumerable<(string Text, string? Url)> GetExplicitSeeAlsoLinks(XmlDocNode node, XmlDocPageContext context)
	{
		foreach (var seeAlso in node.XmlMember?.SeeAlso ?? [])
		{
			var text = seeAlso.Text;
			var url = seeAlso.Href;
			if (seeAlso.Ref is { } reference)
			{
				text = string.IsNullOrWhiteSpace(text) ? reference.ShortName : text;
				url = context.GetLinkUrl(reference);
			}
			if (!string.IsNullOrWhiteSpace(text) || url is not null)
				yield return (text ?? url!, url);
		}
	}

	private static IEnumerable<(string Text, string? Url)> GetAutomaticSeeAlsoLinks(XmlDocNode node, XmlDocPageContext context)
	{
		if (node.Parent is XmlDocNamespaceNode namespaceNode && context.FindPage(namespaceNode) is { } namespacePage)
			yield return ("namespace " + namespaceNode.Name, context.UrlMapper.GetUrl(context.Page, namespacePage, namespaceNode));
		else if (node.Parent is XmlDocTypeNode typeNode && context.FindPage(typeNode) is { } typePage)
			yield return (GetTypeLabel(typeNode.TypeInfo), context.UrlMapper.GetUrl(context.Page, typePage, typeNode));

		if (node is XmlDocTypeNode type)
		{
			foreach (var baseType in GetRelatedTypes(type.TypeInfo))
			{
				if (context.GetLinkUrl(baseType.GetTypeInfo()) is { } url)
					yield return (GetTypeLabel(baseType.GetTypeInfo()), url);
			}
		}

		if (node.MemberInfo is { } member && context.GetSourceUrl(member) is { } sourceUrl)
			yield return ("source", sourceUrl);
	}

	private static IEnumerable<Type> GetRelatedTypes(TypeInfo type)
	{
		if (type.BaseType is { } baseType && baseType != typeof(object) && baseType != typeof(ValueType) && baseType != typeof(Enum) && baseType != typeof(MulticastDelegate))
			yield return GetLinkableType(baseType);
		foreach (var interfaceType in type.ImplementedInterfaces.OrderBy(x => x.FullName, StringComparer.Ordinal))
			yield return GetLinkableType(interfaceType);
	}

	private static Type GetLinkableType(Type type) => type is { IsGenericType: true, IsGenericTypeDefinition: false } ? type.GetGenericTypeDefinition() : type;

	private static string GetTypeLabel(TypeInfo type) => (type.IsInterface ? "interface " : ReflectionFacts.GetTypeKind(type).ToString().ToLowerInvariant() + " ") + ReflectionFacts.GetShortName(type);

	private static string RenderInlines(IEnumerable<XmlDocXmlInline> inlines, XmlDocPageContext context, XmlDocNode? currentNode) => Regex.Replace(string.Concat(inlines.Select(x => RenderInline(x, context, currentNode))), @"\s+", " ").Trim();

	private static string RenderInline(XmlDocXmlInline inline, XmlDocPageContext context, XmlDocNode? currentNode)
	{
		var text = inline.Text ?? "";
		if (inline.Kind == XmlDocXmlInlineKind.SeeCref && inline.Ref is { } reference)
		{
			text = string.IsNullOrWhiteSpace(text) ? reference.ShortName : text;
			var url = context.GetLinkUrl(reference);
			return url is null ? Code(text) : $"[{Code(text)}]({url})";
		}
		if (inline.Kind == XmlDocXmlInlineKind.SeeHref && inline.Href is { } href)
			return $"[{Escape(string.IsNullOrWhiteSpace(text) ? href : text)}]({href})";
		if (inline.Kind == XmlDocXmlInlineKind.SeeLangword)
			return Code(inline.Langword ?? text);
		if (inline.Kind == XmlDocXmlInlineKind.Code)
			return Code(text);
		if (inline.Kind is XmlDocXmlInlineKind.ParamRef or XmlDocXmlInlineKind.TypeParamRef)
		{
			var anchorPrefix = inline.Kind == XmlDocXmlInlineKind.TypeParamRef ? "type-parameter" : "parameter";
			if (currentNode is XmlDocMemberNode)
			{
				var anchor = CreateParameterAnchor(anchorPrefix, text);
				var url = context.FindPage(currentNode) is { } targetPage && targetPage != context.Page ? context.UrlMapper.GetUrl(context.Page, targetPage, currentNode) + "#" + anchor : "#" + anchor;
				return $"[`{Escape(text)}`]({url})";
			}
			return Code(text);
		}
		return Escape(text);
	}

	private static string CreateParameterAnchor(string prefix, string name) => prefix + "-" + Regex.Replace(name.ToLowerInvariant(), "[^a-z0-9_-]+", "-").Trim('-');

	private static string Code(string value)
	{
		var ticks = new string('`', Regex.Matches(value, "`+").Select(x => x.Length).Concat([0]).Max() + 1);
		return ticks + value + ticks;
	}

	private static string Escape(string value) => WebUtility.HtmlEncode(value).Replace("|", "&#x7C;", StringComparison.Ordinal);

	private sealed record XmlDocListRow(bool IsHeader, IReadOnlyList<string> Cells);
}
