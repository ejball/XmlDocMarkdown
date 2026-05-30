using System.Globalization;
using System.Reflection;
using XmlDocGen.Core.Nodes;

namespace XmlDocGen.Core.CSharp;

/// <summary>Builds structured C# signatures from documentation nodes.</summary>
public abstract class CSharpSignatureBuilder
{
	/// <summary>Gets the default full-signature builder.</summary>
	public static CSharpSignatureBuilder Full { get; } = new FullCSharpSignatureBuilder();

	/// <summary>Gets the default short-signature builder.</summary>
	public static CSharpSignatureBuilder Short { get; } = new ShortCSharpSignatureBuilder();

	/// <summary>Builds a C# signature for a node.</summary>
	public abstract CSharpSignature GetSignature(XmlDocNode node);
}

/// <summary>A C# signature and its token stream.</summary>
public sealed class CSharpSignature
{
	/// <summary>Initializes a new instance of the <see cref="CSharpSignature"/> class.</summary>
	public CSharpSignature(IEnumerable<CSharpToken> tokens)
	{
		Tokens = [.. tokens];
		Text = string.Concat(Tokens.Select(x => x.Text));
	}

	/// <summary>Gets the rendered signature text.</summary>
	public string Text { get; }

	/// <summary>Gets the signature tokens.</summary>
	public IReadOnlyList<CSharpToken> Tokens { get; }

	/// <inheritdoc />
	public override string ToString() => Text;
}

/// <summary>A token in a C# signature.</summary>
public sealed record CSharpToken(CSharpTokenKind Kind, string Text, MemberInfo? Target = null);

/// <summary>Kinds of C# signature tokens.</summary>
public enum CSharpTokenKind
{
	/// <summary>A keyword.</summary>
	Keyword,
	/// <summary>An identifier.</summary>
	Identifier,
	/// <summary>A type name.</summary>
	TypeName,
	/// <summary>Punctuation.</summary>
	Punctuation,
	/// <summary>Whitespace.</summary>
	Whitespace,
	/// <summary>A literal value.</summary>
	Literal,
	/// <summary>Plain text.</summary>
	Text,
}

internal sealed class FullCSharpSignatureBuilder : CSharpSignatureBuilder
{
	public override CSharpSignature GetSignature(XmlDocNode node) => new(CSharpSignatureRendering.Render(node, full: true));
}

internal sealed class ShortCSharpSignatureBuilder : CSharpSignatureBuilder
{
	public override CSharpSignature GetSignature(XmlDocNode node) => new(CSharpSignatureRendering.Render(node, full: false));
}

internal static class CSharpSignatureRendering
{
	public static IEnumerable<CSharpToken> Render(XmlDocNode node, bool full)
	{
		return node switch
		{
			XmlDocTypeNode typeNode => RenderType(typeNode, full),
			XmlDocMemberNode memberNode => RenderMember(memberNode, full),
			_ => [new CSharpToken(CSharpTokenKind.Identifier, node.Name)],
		};
	}

	private static IEnumerable<CSharpToken> RenderType(XmlDocTypeNode node, bool full)
	{
		if (full)
		{
			yield return Keyword(GetAccessModifier(node.Visibility));
			yield return Space();
			if (node.Type is { IsClass: true, IsSealed: true, IsAbstract: false })
			{
				yield return Keyword("sealed");
				yield return Space();
			}
			else if (ReflectionFacts.IsStatic(node.Type))
			{
				yield return Keyword("static");
				yield return Space();
			}
			else if (node.Type is { IsClass: true, IsAbstract: true, IsSealed: false })
			{
				yield return Keyword("abstract");
				yield return Space();
			}
		}

		foreach (var token in RenderTypeKind(node.Kind))
			yield return token;
		yield return Space();
		yield return Identifier(ReflectionFacts.GetShortName(node.Type));
		foreach (var token in RenderGenericParameters(node.Type.GenericTypeParameters, includeVariance: full))
			yield return token;
	}

	private static IEnumerable<CSharpToken> RenderMember(XmlDocMemberNode node, bool full)
	{
		var member = node.Member;
		if (full)
		{
			yield return Keyword(GetAccessModifier(node.Visibility));
			yield return Space();
			if (ReflectionFacts.IsStatic(member))
			{
				yield return Keyword("static");
				yield return Space();
			}
			else if (ReflectionFacts.IsAbstract(member))
			{
				yield return Keyword("abstract");
				yield return Space();
			}
			else if (ReflectionFacts.IsVirtual(member))
			{
				yield return Keyword("virtual");
				yield return Space();
			}
		}

		switch (member)
		{
			case ConstructorInfo constructor:
				yield return Identifier(ReflectionFacts.GetShortName(constructor));
				foreach (var token in RenderParameters(constructor.GetParameters()))
					yield return token;
				break;
			case MethodInfo method:
				if (full)
				{
					yield return TypeName(RenderTypeName(method.ReturnType.GetTypeInfo()), method.ReturnType.GetTypeInfo());
					yield return Space();
				}
				yield return Identifier(GetOperatorKeywordName(ReflectionFacts.GetShortName(method)));
				foreach (var token in RenderGenericParameters(method.GetGenericArguments(), includeVariance: false))
					yield return token;
				foreach (var token in RenderParameters(method.GetParameters()))
					yield return token;
				break;
			case PropertyInfo property:
				if (full)
				{
					yield return TypeName(RenderTypeName(property.PropertyType.GetTypeInfo()), property.PropertyType.GetTypeInfo());
					yield return Space();
				}
				yield return Identifier(property.GetIndexParameters().Length == 0 ? property.Name : "this");
				if (property.GetIndexParameters().Length != 0)
				{
					yield return Punctuation("[");
					foreach (var token in RenderParameterList(property.GetIndexParameters()))
						yield return token;
					yield return Punctuation("]");
				}
				if (full)
				{
					yield return Space();
					yield return Text(GetPropertyAccessors(property));
				}
				break;
			case EventInfo @event:
				if (full)
				{
					yield return Keyword("event");
					yield return Space();
					yield return TypeName(RenderTypeName(@event.EventHandlerType!.GetTypeInfo()), @event.EventHandlerType!.GetTypeInfo());
					yield return Space();
				}
				yield return Identifier(@event.Name);
				break;
			case FieldInfo field:
				if (full)
				{
					if (field.IsLiteral)
					{
						yield return Keyword("const");
						yield return Space();
					}
					else if (field.IsInitOnly)
					{
						yield return Keyword("readonly");
						yield return Space();
					}
					yield return TypeName(RenderTypeName(field.FieldType.GetTypeInfo()), field.FieldType.GetTypeInfo());
					yield return Space();
				}
				yield return Identifier(field.Name);
				break;
		}
	}

	private static IEnumerable<CSharpToken> RenderTypeKind(XmlDocTypeKind kind)
	{
		var parts = kind switch
		{
			XmlDocTypeKind.Record => new[] { "record" },
			XmlDocTypeKind.RecordStruct => ["record", "struct"],
			XmlDocTypeKind.Class => ["class"],
			XmlDocTypeKind.Interface => ["interface"],
			XmlDocTypeKind.Struct => ["struct"],
			XmlDocTypeKind.Enum => ["enum"],
			XmlDocTypeKind.Delegate => ["delegate"],
			_ => ["class"],
		};
		foreach (var part in parts)
		{
			if (part != "record")
				yield return Space();
			yield return Keyword(part);
		}
	}

	private static IEnumerable<CSharpToken> RenderGenericParameters(Type[] parameters, bool includeVariance)
	{
		if (parameters.Length == 0)
			yield break;
		yield return Punctuation("<");
		for (var index = 0; index < parameters.Length; index++)
		{
			if (index != 0)
			{
				yield return Punctuation(",");
				yield return Space();
			}
			if (includeVariance && parameters[index].GetTypeInfo().GenericParameterAttributes.HasFlag(GenericParameterAttributes.Covariant))
			{
				yield return Keyword("out");
				yield return Space();
			}
			if (includeVariance && parameters[index].GetTypeInfo().GenericParameterAttributes.HasFlag(GenericParameterAttributes.Contravariant))
			{
				yield return Keyword("in");
				yield return Space();
			}
			yield return Identifier(parameters[index].Name);
		}
		yield return Punctuation(">");
	}

	private static IEnumerable<CSharpToken> RenderParameters(ParameterInfo[] parameters)
	{
		yield return Punctuation("(");
		foreach (var token in RenderParameterList(parameters))
			yield return token;
		yield return Punctuation(")");
	}

	private static IEnumerable<CSharpToken> RenderParameterList(ParameterInfo[] parameters)
	{
		for (var index = 0; index < parameters.Length; index++)
		{
			var parameter = parameters[index];
			if (index != 0)
			{
				yield return Punctuation(",");
				yield return Space();
			}
			if (parameter.ParameterType.IsByRef)
			{
				yield return Keyword(parameter.IsOut ? "out" : "ref");
				yield return Space();
			}
			if (parameter.GetCustomAttributes<ParamArrayAttribute>().Any())
			{
				yield return Keyword("params");
				yield return Space();
			}
			yield return TypeName(RenderTypeName(parameter.ParameterType.GetTypeInfo()), parameter.ParameterType.GetTypeInfo());
			yield return Space();
			yield return Identifier(parameter.Name ?? "P_" + index.ToString(CultureInfo.InvariantCulture));
		}
	}

	private static string RenderTypeName(TypeInfo type)
	{
		if (type.IsByRef)
			return RenderTypeName(type.GetElementType()!.GetTypeInfo());
		var nullable = Nullable.GetUnderlyingType(type.AsType());
		if (nullable is not null)
			return RenderTypeName(nullable.GetTypeInfo()) + "?";
		if (type.IsArray)
			return RenderTypeName(type.GetElementType()!.GetTypeInfo()) + "[]";
		var builtIn = TryGetBuiltInTypeName(type.AsType());
		if (builtIn is not null)
			return builtIn;
		return ReflectionFacts.GetShortName(type) + RenderGenericArguments(type.GenericTypeArguments);
	}

	private static string RenderGenericArguments(Type[] arguments) => arguments.Length == 0 ? "" : "<" + string.Join(", ", arguments.Select(x => RenderTypeName(x.GetTypeInfo()))) + ">";

	private static string GetPropertyAccessors(PropertyInfo property)
	{
		var get = property.GetMethod is not null;
		var set = property.SetMethod is not null;
		return (get, set) switch
		{
			(true, true) => "{ get; set; }",
			(true, false) => "{ get; }",
			(false, true) => "{ set; }",
			_ => "{ }",
		};
	}

	private static string GetAccessModifier(XmlDocVisibility visibility) => visibility switch
	{
		XmlDocVisibility.Public => "public",
		XmlDocVisibility.Protected => "protected",
		XmlDocVisibility.ProtectedInternal => "protected internal",
		XmlDocVisibility.Internal => "internal",
		_ => "private",
	};

	private static string GetOperatorKeywordName(string name) => name switch
	{
		"op_Addition" => "operator +",
		"op_Subtraction" => "operator -",
		"op_Multiply" => "operator *",
		"op_Division" => "operator /",
		"op_Equality" => "operator ==",
		"op_Inequality" => "operator !=",
		"op_Implicit" => "implicit operator",
		"op_Explicit" => "explicit operator",
		_ => name,
	};

	private static string? TryGetBuiltInTypeName(Type type)
	{
		if (type == typeof(void)) return "void";
		if (type == typeof(bool)) return "bool";
		if (type == typeof(byte)) return "byte";
		if (type == typeof(sbyte)) return "sbyte";
		if (type == typeof(char)) return "char";
		if (type == typeof(decimal)) return "decimal";
		if (type == typeof(double)) return "double";
		if (type == typeof(float)) return "float";
		if (type == typeof(int)) return "int";
		if (type == typeof(uint)) return "uint";
		if (type == typeof(long)) return "long";
		if (type == typeof(ulong)) return "ulong";
		if (type == typeof(object)) return "object";
		if (type == typeof(short)) return "short";
		if (type == typeof(ushort)) return "ushort";
		if (type == typeof(string)) return "string";
		return null;
	}

	private static CSharpToken Keyword(string text) => new(CSharpTokenKind.Keyword, text);
	private static CSharpToken Identifier(string text) => new(CSharpTokenKind.Identifier, text);
	private static CSharpToken TypeName(string text, MemberInfo target) => new(CSharpTokenKind.TypeName, text, target);
	private static CSharpToken Punctuation(string text) => new(CSharpTokenKind.Punctuation, text);
	private static CSharpToken Space() => new(CSharpTokenKind.Whitespace, " ");
	private static CSharpToken Text(string text) => new(CSharpTokenKind.Text, text);
}

internal sealed class FullCSharpSignatureBuilderPlaceholder
{
}
