using System.Collections.ObjectModel;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Xml.XPath;

namespace XmlDocGen.Core.Xml;

/// <summary>An XML documentation identifier, e.g. <c>T:My.Type</c> or <c>M:My.Type.Method(System.Int32)</c>.</summary>
public readonly struct XmlDocRef : IEquatable<XmlDocRef>
{
	/// <summary>Initializes a new instance of the <see cref="XmlDocRef"/> struct.</summary>
	public XmlDocRef(string value)
	{
		ArgumentException.ThrowIfNullOrEmpty(value);
		if (value.Length < 3 || value[1] != ':')
			throw new ArgumentException("XML documentation identifiers must start with a one-letter prefix and ':'.", nameof(value));

		Value = value;
	}

	/// <summary>Gets the raw identifier string.</summary>
	public string Value { get; }

	/// <summary>Builds a reference from a reflection type.</summary>
	public static XmlDocRef ForType(Type type) => new(XmlDocRefUtility.GetXmlDocRef(type.GetTypeInfo())!);

	/// <summary>Builds a reference from a reflected member.</summary>
	public static XmlDocRef ForMember(MemberInfo member) => new(XmlDocRefUtility.GetXmlDocRef(member)!);

	/// <summary>Builds a reference for a namespace.</summary>
	public static XmlDocRef ForNamespace(string namespaceName) => new("N:" + namespaceName);

	/// <inheritdoc />
	public bool Equals(XmlDocRef other) => StringComparer.Ordinal.Equals(Value, other.Value);

	/// <inheritdoc />
	public override bool Equals(object? obj) => obj is XmlDocRef other && Equals(other);

	/// <inheritdoc />
	public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(Value);

	/// <inheritdoc />
	public override string ToString() => Value;

	/// <summary>Compares two references for equality.</summary>
	public static bool operator ==(XmlDocRef left, XmlDocRef right) => left.Equals(right);

	/// <summary>Compares two references for inequality.</summary>
	public static bool operator !=(XmlDocRef left, XmlDocRef right) => !left.Equals(right);
}

/// <summary>An in-memory representation of a compiler-generated XML documentation file.</summary>
public sealed class XmlDocXmlFile
{
	/// <summary>Initializes a new empty XML documentation file.</summary>
	public XmlDocXmlFile()
	{
		Members = [];
		m_membersByRef = new Dictionary<XmlDocRef, XmlDocXmlMember>();
	}

	/// <summary>Initializes a new instance of the <see cref="XmlDocXmlFile"/> class.</summary>
	public XmlDocXmlFile(XDocument document)
	{
		ArgumentNullException.ThrowIfNull(document);
		AssemblyName = document.Root?.Element("assembly")?.Element("name")?.Value;
		Members = [.. document.Root?.Elements("members").Elements("member").Where(x => x.Attribute("name") is not null).Select(x => new XmlDocXmlMember(x)) ?? []];
		m_membersByRef = Members.GroupBy(x => x.Ref).ToDictionary(x => x.Key, x => x.First());
	}

	/// <summary>Loads XML documentation from a file path.</summary>
	public static XmlDocXmlFile Load(string path) => new(LoadDocument(path));

	/// <summary>Loads XML documentation from a stream.</summary>
	public static XmlDocXmlFile Load(Stream stream) => new(XDocument.Load(stream));

	/// <summary>Parses XML documentation from a string.</summary>
	public static XmlDocXmlFile Parse(string xml) => new(XDocument.Parse(xml));

	/// <summary>Gets the assembly name from the XML documentation file, if present.</summary>
	public string? AssemblyName { get; }

	/// <summary>Gets the parsed members.</summary>
	public IReadOnlyList<XmlDocXmlMember> Members { get; }

	/// <summary>Finds documentation for the given reference.</summary>
	public XmlDocXmlMember? FindMember(XmlDocRef reference) => m_membersByRef.GetValueOrDefault(reference);

	private static XDocument LoadDocument(string path)
	{
		var document = XDocument.Load(path);
		ResolveIncludes(document, Path.GetDirectoryName(Path.GetFullPath(path)) ?? "");
		return document;
	}

	private static void ResolveIncludes(XDocument document, string basePath)
	{
		foreach (var include in document.Descendants("include").ToList())
		{
			var file = include.Attribute("file")?.Value;
			var path = include.Attribute("path")?.Value;
			if (string.IsNullOrWhiteSpace(file) || string.IsNullOrWhiteSpace(path))
				continue;

			var includePath = Path.GetFullPath(Path.Combine(basePath, file));
			var includeDocument = XDocument.Load(includePath);
			include.ReplaceWith(includeDocument.XPathSelectElements(path).Select(CloneElement));
		}
	}

	private static XElement CloneElement(XElement element) => new(element);

	private readonly IReadOnlyDictionary<XmlDocRef, XmlDocXmlMember> m_membersByRef;
}

/// <summary>The parsed XML documentation for a single member.</summary>
public sealed class XmlDocXmlMember
{
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

/// <summary>A block of parsed XML documentation content.</summary>
public sealed class XmlDocXmlBlock
{
	/// <summary>Gets inline content in this block.</summary>
	public Collection<XmlDocXmlInline> Inlines { get; } = [];

	/// <summary>Gets or sets a value indicating whether this block is fenced code.</summary>
	public bool IsCode { get; set; }

	/// <summary>Gets or sets the code language hint.</summary>
	public string? Language { get; set; }

	/// <summary>Gets or sets the list kind, when this block is part of a list.</summary>
	public XmlDocXmlListKind? ListKind { get; set; }

	/// <summary>Gets or sets the list depth.</summary>
	public int ListDepth { get; set; }

	/// <summary>Gets or sets a value indicating whether this block is a list header.</summary>
	public bool IsListHeader { get; set; }

	/// <summary>Gets or sets a value indicating whether this block is a definition-list term.</summary>
	public bool IsListTerm { get; set; }
}

/// <summary>Parsed inline XML documentation content.</summary>
public sealed class XmlDocXmlInline
{
	/// <summary>Initializes a new instance of the <see cref="XmlDocXmlInline"/> class.</summary>
	public XmlDocXmlInline(XmlDocXmlInlineKind kind, string? text)
	{
		Kind = kind;
		Text = text;
	}

	/// <summary>Gets the inline kind.</summary>
	public XmlDocXmlInlineKind Kind { get; }

	/// <summary>Gets the display text.</summary>
	public string? Text { get; }

	/// <summary>Gets the referenced XML documentation identifier.</summary>
	public XmlDocRef? Ref { get; init; }

	/// <summary>Gets the linked URL.</summary>
	public string? Href { get; init; }

	/// <summary>Gets the language keyword.</summary>
	public string? Langword { get; init; }

	/// <summary>Gets the referenced parameter or type parameter name.</summary>
	public string? Name => Text;
}

/// <summary>Kinds of inline XML documentation content.</summary>
public enum XmlDocXmlInlineKind
{
	/// <summary>Plain text.</summary>
	Text,
	/// <summary>Inline code.</summary>
	Code,
	/// <summary>A <c>see cref</c> link.</summary>
	SeeCref,
	/// <summary>A <c>see href</c> link.</summary>
	SeeHref,
	/// <summary>A <c>see langword</c> value.</summary>
	SeeLangword,
	/// <summary>A parameter reference.</summary>
	ParamRef,
	/// <summary>A type parameter reference.</summary>
	TypeParamRef,
}

/// <summary>Parsed XML documentation for a parameter.</summary>
public sealed class XmlDocXmlParameter(string name)
{
	/// <summary>Gets the parameter name.</summary>
	public string Name { get; } = name;

	/// <summary>Gets the parameter description.</summary>
	public Collection<XmlDocXmlBlock> Description { get; } = [];
}

/// <summary>Parsed XML documentation for an exception.</summary>
public sealed class XmlDocXmlException(XmlDocRef? exceptionTypeRef)
{
	/// <summary>Gets the exception type reference.</summary>
	public XmlDocRef? ExceptionTypeRef { get; } = exceptionTypeRef;

	/// <summary>Gets the documented condition.</summary>
	public Collection<XmlDocXmlBlock> Condition { get; } = [];
}

/// <summary>Parsed XML documentation for a see-also item.</summary>
public sealed class XmlDocXmlSeeAlso(XmlDocRef? reference, string? href, string? text)
{
	/// <summary>Gets the referenced XML documentation identifier.</summary>
	public XmlDocRef? Ref { get; } = reference;

	/// <summary>Gets the external URL.</summary>
	public string? Href { get; } = href;

	/// <summary>Gets the display text.</summary>
	public string? Text { get; } = text;
}

/// <summary>The raw XML inheritdoc directive.</summary>
public sealed class XmlDocXmlInheritDoc(XmlDocRef? cref, string? path)
{
	/// <summary>Gets the optional inherited member reference.</summary>
	public XmlDocRef? Cref { get; } = cref;

	/// <summary>Gets the optional XML path.</summary>
	public string? Path { get; } = path;
}

/// <summary>Kinds of XML documentation lists.</summary>
public enum XmlDocXmlListKind
{
	/// <summary>A bullet list.</summary>
	Bullet,
	/// <summary>A numbered list.</summary>
	Number,
	/// <summary>A table.</summary>
	Table,
	/// <summary>A definition list.</summary>
	Definition,
}

internal static partial class XmlDocRefUtility
{
	public static string? GetXmlDocRef(MemberInfo? memberInfo)
	{
		switch (memberInfo)
		{
			case null:
				return null;
			case TypeInfo typeInfo:
				return "T:" + GetXmlDocTypePart(typeInfo);
			case MethodBase methodBase:
				var methodInfo = methodBase as MethodInfo;
				return "M:" + GetXmlDocMemberPart(methodBase) + (methodBase.IsGenericMethodDefinition ? $"``{methodBase.GetGenericArguments().Length}" : "") + GetXmlDocParameters(methodBase.GetParameters()) + (methodInfo?.Name is "op_Implicit" or "op_Explicit" ? $"~{GetXmlDocTypePart(methodInfo.ReturnType.GetTypeInfo())}" : "");
			case PropertyInfo propertyInfo:
				return "P:" + GetXmlDocMemberPart(propertyInfo) + (propertyInfo.GetIndexParameters().Length == 0 ? "" : GetXmlDocParameters(propertyInfo.GetIndexParameters()));
			case EventInfo eventInfo:
				return "E:" + GetXmlDocMemberPart(eventInfo);
			case FieldInfo fieldInfo:
				return "F:" + GetXmlDocMemberPart(fieldInfo);
			default:
				throw new InvalidOperationException("Unexpected member: " + memberInfo);
		}
	}

	public static string GetShortNameForXmlDocRef(XmlDocRef reference)
	{
		var match = XmlDocNameRegex().Match(reference.Value);
		return match.Success ? match.Groups["name"].Value : reference.Value;
	}

	private static string GetXmlDocTypePart(TypeInfo typeInfo)
	{
		var builder = new StringBuilder();
		if (typeInfo.IsArray)
		{
			builder.Append(GetXmlDocTypePart(typeInfo.GetElementType()!.GetTypeInfo()));
			builder.Append("[]");
		}
		else if (typeInfo.IsByRef)
		{
			builder.Append(GetXmlDocTypePart(typeInfo.GetElementType()!.GetTypeInfo()));
			builder.Append('@');
		}
		else if (!typeInfo.IsGenericParameter)
		{
			if (typeInfo.DeclaringType is not null)
				builder.Append(GetXmlDocTypePart(typeInfo.DeclaringType.GetTypeInfo()) + ".");
			else if (!string.IsNullOrEmpty(typeInfo.Namespace))
				builder.Append(typeInfo.Namespace + ".");

			var tickIndex = typeInfo.Name.IndexOf('`', StringComparison.Ordinal);
			if (typeInfo is { IsGenericType: true, IsGenericTypeDefinition: false } && tickIndex != -1)
			{
				builder.Append(typeInfo.Name.AsSpan(0, tickIndex));
				builder.Append('{');
				builder.Append(string.Join(",", typeInfo.GenericTypeArguments.Select(x => GetXmlDocTypePart(x.GetTypeInfo()))));
				builder.Append('}');
			}
			else
			{
				builder.Append(typeInfo.Name);
			}
		}
		else if (typeInfo.DeclaringMethod is { } declaringMethod)
		{
			builder.Append("``" + declaringMethod.GetGenericArguments().ToList().IndexOf(typeInfo.AsType()));
		}
		else
		{
			builder.Append("`" + typeInfo.DeclaringType!.GetTypeInfo().GenericTypeParameters.ToList().IndexOf(typeInfo.AsType()));
		}

		return builder.ToString();
	}

	private static string GetXmlDocMemberPart(MemberInfo memberInfo) => GetXmlDocTypePart(memberInfo.DeclaringType!.GetTypeInfo()) + "." + memberInfo.Name.Replace('.', '#');

	private static string GetXmlDocParameters(ParameterInfo[] parameters) => parameters.Length == 0 ? "" : "(" + string.Join(",", parameters.Select(x => GetXmlDocTypePart(x.ParameterType.GetTypeInfo()))) + ")";

	[GeneratedRegex(@"^[A-Z]:([^\.]+\.)*(?'name'[^\.\(\{`]+)", RegexOptions.ExplicitCapture | RegexOptions.CultureInvariant)]
	private static partial Regex XmlDocNameRegex();
}
