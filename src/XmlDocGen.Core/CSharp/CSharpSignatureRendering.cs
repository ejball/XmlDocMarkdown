using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using XmlDocGen.Core.Nodes;

namespace XmlDocGen.Core.CSharp;

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
			if (node.TypeInfo.GetCustomAttribute<FlagsAttribute>() is not null)
			{
				yield return Text("[Flags]");
				yield return Text("\n");
			}
			yield return Keyword(GetAccessModifier(node.Visibility));
			yield return Space();
			if (node.TypeInfo is { IsClass: true, IsSealed: true, IsAbstract: false })
			{
				yield return Keyword("sealed");
				yield return Space();
			}
			else if (ReflectionFacts.IsStatic(node.Type))
			{
				yield return Keyword("static");
				yield return Space();
			}
			else if (node.TypeInfo is { IsClass: true, IsAbstract: true, IsSealed: false })
			{
				yield return Keyword("abstract");
				yield return Space();
			}
		}

		if (node.IsReadOnly)
		{
			yield return Keyword("readonly");
			yield return Space();
		}
		if (node.IsRefStruct)
		{
			yield return Keyword("ref");
			yield return Space();
		}
		foreach (var token in RenderTypeKind(node.Kind))
		{
			yield return token;
		}
		yield return Space();
		yield return Identifier(ReflectionFacts.GetShortName(node.Type));
		foreach (var token in RenderGenericParameters(node.TypeInfo.GenericTypeParameters, includeVariance: full))
			yield return token;
		if (full && node.Kind != XmlDocTypeKind.Enum)
		{
			foreach (var token in RenderTypeBases(node.TypeInfo))
				yield return token;
			foreach (var token in RenderGenericConstraints(node.TypeInfo.GenericTypeParameters))
				yield return token;
		}
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
			if (IsAbstractForSignature(member))
			{
				yield return Keyword("abstract");
				yield return Space();
			}
			else if (IsVirtualForSignature(member))
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
					foreach (var token in RenderRefKind(method.ReturnParameter, method.ReturnType))
						yield return token;
					yield return TypeName(RenderTypeName(method.ReturnType, s_nullability.Create(method.ReturnParameter), GetTupleElementNames(method.ReturnParameter)), method.ReturnType.GetTypeInfo());
					yield return Space();
				}
				yield return method.Name.StartsWith("op_", StringComparison.Ordinal) ? Operator(GetOperatorKeywordName(ReflectionFacts.GetShortName(method))) : Identifier(GetOperatorKeywordName(ReflectionFacts.GetShortName(method)));
				foreach (var token in RenderGenericParameters(method.GetGenericArguments(), includeVariance: false))
					yield return token;
				foreach (var token in RenderParameters(method.GetParameters(), method.IsDefined(typeof(ExtensionAttribute))))
					yield return token;
				foreach (var token in RenderGenericConstraints(method.GetGenericArguments()))
					yield return token;
				break;
			case PropertyInfo property:
				if (full)
				{
					if (node.IsRequired)
					{
						yield return Keyword("required");
						yield return Space();
					}
					foreach (var token in RenderRefKind(property.GetMethod?.ReturnParameter, property.PropertyType))
						yield return token;
					yield return TypeName(RenderTypeName(property.PropertyType, s_nullability.Create(property), GetTupleElementNames(property)), property.PropertyType.GetTypeInfo());
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
				yield return Space();
				yield return Text(GetPropertyAccessors(property));
				break;
			case EventInfo @event:
				if (full)
				{
					yield return Keyword("event");
					yield return Space();
					yield return TypeName(RenderTypeName(@event.EventHandlerType!, null, []), @event.EventHandlerType!.GetTypeInfo());
					yield return Space();
				}
				yield return Identifier(@event.Name);
				break;
			case FieldInfo field:
				if (full)
				{
					if (node.IsRequired)
					{
						yield return Keyword("required");
						yield return Space();
					}
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
					yield return TypeName(RenderTypeName(field.FieldType, s_nullability.Create(field), GetTupleElementNames(field)), field.FieldType.GetTypeInfo());
					yield return Space();
				}
				yield return Identifier(field.Name);
				if (full && field.IsLiteral)
				{
					yield return Space();
					yield return Operator("=");
					yield return Space();
					yield return Literal(RenderLiteral(field.GetRawConstantValue()));
				}
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
			if (part != parts[0])
				yield return Space();
			yield return Keyword(part);
		}
	}

	private static IEnumerable<CSharpToken> RenderTypeBases(TypeInfo type)
	{
		var bases = new List<Type>();
		if (type.BaseType is { } baseType && baseType != typeof(object) && baseType != typeof(ValueType) && baseType != typeof(Enum) && baseType != typeof(MulticastDelegate))
			bases.Add(baseType);
		bases.AddRange(type.ImplementedInterfaces.OrderBy(x => x.FullName, StringComparer.Ordinal));
		if (bases.Count == 0)
			yield break;

		yield return Space();
		yield return Punctuation(":");
		yield return Space();
		for (var index = 0; index < bases.Count; index++)
		{
			if (index != 0)
			{
				yield return Punctuation(",");
				yield return Space();
			}
			yield return TypeName(RenderTypeName(bases[index], null, []), bases[index].GetTypeInfo());
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

	private static IEnumerable<CSharpToken> RenderGenericConstraints(Type[] parameters)
	{
		foreach (var parameter in parameters.Where(x => x.IsGenericParameter))
		{
			var constraints = GetGenericConstraints(parameter).ToList();
			if (constraints.Count == 0)
				continue;

			yield return Space();
			yield return Keyword("where");
			yield return Space();
			yield return Identifier(parameter.Name);
			yield return Space();
			yield return Punctuation(":");
			yield return Space();
			for (var index = 0; index < constraints.Count; index++)
			{
				if (index != 0)
				{
					yield return Punctuation(",");
					yield return Space();
				}
				foreach (var token in constraints[index])
					yield return token;
			}
		}
	}

	private static IEnumerable<IReadOnlyList<CSharpToken>> GetGenericConstraints(Type parameter)
	{
		var attributes = parameter.GetTypeInfo().GenericParameterAttributes;
		if (HasAttribute(parameter, "System.Runtime.CompilerServices.IsUnmanagedAttribute"))
			yield return [Keyword("unmanaged")];
		else if (attributes.HasFlag(GenericParameterAttributes.NotNullableValueTypeConstraint))
			yield return [Keyword("struct")];
		else if (HasNotNullConstraint(parameter))
			yield return [Keyword("notnull")];
		else if (attributes.HasFlag(GenericParameterAttributes.ReferenceTypeConstraint))
			yield return [Keyword(HasNullableConstraint(parameter) ? "class?" : "class")];

		foreach (var constraint in parameter.GetGenericParameterConstraints().Where(x => x != typeof(ValueType)))
			yield return [TypeName(RenderTypeName(constraint, null, []), constraint.GetTypeInfo())];

		if (attributes.HasFlag(GenericParameterAttributes.DefaultConstructorConstraint) && !attributes.HasFlag(GenericParameterAttributes.NotNullableValueTypeConstraint))
			yield return [Keyword("new"), Punctuation("("), Punctuation(")")];
	}

	private static IEnumerable<CSharpToken> RenderParameters(ParameterInfo[] parameters, bool isExtensionMethod = false)
	{
		yield return Punctuation("(");
		foreach (var token in RenderParameterList(parameters, isExtensionMethod))
			yield return token;
		yield return Punctuation(")");
	}

	private static IEnumerable<CSharpToken> RenderParameterList(ParameterInfo[] parameters, bool isExtensionMethod = false)
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
				if (IsScoped(parameter))
				{
					yield return Keyword("scoped");
					yield return Space();
				}
				var byRefKind = GetParameterRefKind(parameter);
				if (byRefKind == "ref readonly")
				{
					yield return Keyword("ref");
					yield return Space();
					yield return Keyword("readonly");
				}
				else
				{
					yield return Keyword(byRefKind);
				}
				yield return Space();
			}
			if (index == 0 && isExtensionMethod)
			{
				yield return Keyword("this");
				yield return Space();
			}
			if (parameter.GetCustomAttributes<ParamArrayAttribute>().Any())
			{
				yield return Keyword("params");
				yield return Space();
			}
			yield return TypeName(RenderTypeName(parameter.ParameterType, s_nullability.Create(parameter), GetTupleElementNames(parameter)), parameter.ParameterType.GetTypeInfo());
			yield return Space();
			yield return Identifier(parameter.Name ?? "P_" + index.ToString(CultureInfo.InvariantCulture));
			if (parameter.HasDefaultValue)
			{
				yield return Space();
				yield return Operator("=");
				yield return Space();
				yield return Literal(RenderLiteral(parameter.DefaultValue));
			}
		}
	}

	private static IEnumerable<CSharpToken> RenderRefKind(ParameterInfo? parameter, Type type)
	{
		if (!type.IsByRef || parameter is null)
			yield break;

		var byRefKind = GetParameterRefKind(parameter);
		if (byRefKind == "ref readonly")
		{
			yield return Keyword("ref");
			yield return Space();
			yield return Keyword("readonly");
		}
		else
		{
			yield return Keyword(byRefKind);
		}
		yield return Space();
	}

	private static string GetParameterRefKind(ParameterInfo parameter)
	{
		if (parameter.IsOut)
			return "out";
		if (IsRefReadOnlyParameter(parameter))
			return "ref readonly";
		if (parameter.IsIn)
			return "in";
		if (IsReadOnlyRef(parameter))
			return "ref readonly";
		return "ref";
	}

	private static string RenderTypeName(Type type, NullabilityInfo? nullability, IReadOnlyList<string?> tupleElementNames)
	{
		if (type.IsByRef)
			return RenderTypeName(type.GetElementType()!, nullability?.ElementType, tupleElementNames);
		if (type.IsPointer)
			return RenderTypeName(type.GetElementType()!, nullability?.ElementType, []) + "*";
		if (type.IsFunctionPointer)
			return RenderFunctionPointerTypeName(type);
		var nullable = Nullable.GetUnderlyingType(type);
		if (nullable is not null)
			return RenderTypeName(nullable, nullability?.GenericTypeArguments.FirstOrDefault(), []) + "?";
		if (type.IsArray)
			return RenderTypeName(type.GetElementType()!, nullability?.ElementType, []) + "[]" + GetNullableReferenceSuffix(type, nullability);
		if (IsValueTuple(type))
			return RenderTupleTypeName(type, nullability, tupleElementNames) + GetNullableReferenceSuffix(type, nullability);
		var builtIn = TryGetBuiltInTypeName(type);
		if (builtIn is not null)
			return builtIn + GetNullableReferenceSuffix(type, nullability);
		return ReflectionFacts.GetShortName(type.GetTypeInfo()) + RenderGenericArguments(type.GenericTypeArguments, nullability?.GenericTypeArguments.ToList() ?? []) + GetNullableReferenceSuffix(type, nullability);
	}

	private static string RenderGenericArguments(Type[] arguments, List<NullabilityInfo> nullability) => arguments.Length == 0 ? "" : "<" + string.Join(", ", arguments.Select((x, index) => RenderTypeName(x, index < nullability.Count ? nullability[index] : null, []))) + ">";

	private static string RenderFunctionPointerTypeName(Type type)
	{
		var parameterTypes = type.GetFunctionPointerParameterTypes();
		var returnType = type.GetFunctionPointerReturnType();
		return "delegate*<" + string.Join(", ", parameterTypes.Append(returnType).Select(static x => RenderTypeName(x, null, []))) + ">";
	}

	private static string RenderTupleTypeName(Type type, NullabilityInfo? nullability, IReadOnlyList<string?> tupleElementNames)
	{
		var types = GetValueTupleElementTypes(type).ToList();
		var nullabilityArguments = nullability?.GenericTypeArguments.ToList() ?? [];
		var parts = new List<string>();
		for (var index = 0; index < types.Count; index++)
		{
			var elementText = RenderTypeName(types[index], index < nullabilityArguments.Count ? nullabilityArguments[index] : null, []);
			if (index < tupleElementNames.Count && !string.IsNullOrWhiteSpace(tupleElementNames[index]))
				elementText += " " + tupleElementNames[index];
			parts.Add(elementText);
		}
		return "(" + string.Join(", ", parts) + ")";
	}

	private static IEnumerable<Type> GetValueTupleElementTypes(Type type)
	{
		foreach (var argument in type.GenericTypeArguments)
		{
			if (argument.IsGenericType && argument.GetGenericTypeDefinition() == typeof(ValueTuple<,,,,,,,>))
			{
				foreach (var nested in GetValueTupleElementTypes(argument))
					yield return nested;
			}
			else
			{
				yield return argument;
			}
		}
	}

	private static bool IsValueTuple(Type type) => type.IsGenericType && type.FullName?.StartsWith("System.ValueTuple`", StringComparison.Ordinal) == true;

	private static string GetNullableReferenceSuffix(Type type, NullabilityInfo? nullability) => !type.IsValueType && nullability?.ReadState == NullabilityState.Nullable ? "?" : "";

	private static string GetPropertyAccessors(PropertyInfo property)
	{
		var get = property.GetMethod is not null;
		var set = property.SetMethod is not null;
		var setName = IsInitOnly(property) ? "init" : "set";
		return (get, set) switch
		{
			(true, true) => "{ get; " + setName + "; }",
			(true, false) => "{ get; }",
			(false, true) => "{ " + setName + "; }",
			_ => "{ }",
		};
	}

	private static bool IsInitOnly(PropertyInfo property) => property.SetMethod?.ReturnParameter.GetRequiredCustomModifiers().Contains(typeof(IsExternalInit)) == true;

	private static string RenderLiteral(object? value)
	{
		return value switch
		{
			null => "null",
			string text => "\"" + EscapeString(text) + "\"",
			char ch => "'" + EscapeChar(ch) + "'",
			bool flag => flag ? "true" : "false",
			float number when float.IsNaN(number) => "float.NaN",
			float number when float.IsPositiveInfinity(number) => "float.PositiveInfinity",
			float number when float.IsNegativeInfinity(number) => "float.NegativeInfinity",
			float number => number.ToString(CultureInfo.InvariantCulture) + "F",
			double number when double.IsNaN(number) => "double.NaN",
			double number when double.IsPositiveInfinity(number) => "double.PositiveInfinity",
			double number when double.IsNegativeInfinity(number) => "double.NegativeInfinity",
			double number => number.ToString(CultureInfo.InvariantCulture) + "D",
			decimal number => number.ToString(CultureInfo.InvariantCulture) + "M",
			Enum enumValue => RenderEnumLiteral(enumValue),
			DateTime => "default",
			_ => Convert.ToString(value, CultureInfo.InvariantCulture) ?? "default",
		};
	}

	private static string RenderEnumLiteral(Enum value)
	{
		var typeName = value.GetType().Name;
		var text = value.ToString();
		return text.Contains(',', StringComparison.Ordinal) ? string.Join(" | ", text.Split(',').Select(x => typeName + "." + x.Trim())) : typeName + "." + text;
	}

	private static string EscapeString(string text) => string.Concat(text.Select(EscapeChar));

	private static string EscapeChar(char ch)
	{
		return ch switch
		{
			'\0' => "\\0",
			'\a' => "\\a",
			'\b' => "\\b",
			'\f' => "\\f",
			'\n' => "\\n",
			'\r' => "\\r",
			'\t' => "\\t",
			'\v' => "\\v",
			'\\' => "\\\\",
			'\'' => "\\'",
			'\"' => "\\\"",
			< ' ' or > '~' => "\\u" + ((int) ch).ToString("X4", CultureInfo.InvariantCulture),
			_ => ch.ToString(),
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
		"op_CheckedAddition" => "operator checked +",
		"op_Subtraction" => "operator -",
		"op_CheckedSubtraction" => "operator checked -",
		"op_Multiply" => "operator *",
		"op_CheckedMultiply" => "operator checked *",
		"op_Division" => "operator /",
		"op_Modulus" => "operator %",
		"op_BitwiseAnd" => "operator &",
		"op_BitwiseOr" => "operator |",
		"op_ExclusiveOr" => "operator ^",
		"op_LeftShift" => "operator <<",
		"op_RightShift" => "operator >>",
		"op_UnsignedRightShift" => "operator >>>",
		"op_Equality" => "operator ==",
		"op_Inequality" => "operator !=",
		"op_LessThan" => "operator <",
		"op_LessThanOrEqual" => "operator <=",
		"op_GreaterThan" => "operator >",
		"op_GreaterThanOrEqual" => "operator >=",
		"op_UnaryPlus" => "operator +",
		"op_UnaryNegation" => "operator -",
		"op_CheckedUnaryNegation" => "operator checked -",
		"op_Increment" => "operator ++",
		"op_CheckedIncrement" => "operator checked ++",
		"op_Decrement" => "operator --",
		"op_CheckedDecrement" => "operator checked --",
		"op_LogicalNot" => "operator !",
		"op_OnesComplement" => "operator ~",
		"op_True" => "operator true",
		"op_False" => "operator false",
		"op_Implicit" => "implicit operator",
		"op_Explicit" => "explicit operator",
		"op_CheckedExplicit" => "explicit operator checked",
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
		if (type == typeof(IntPtr)) return "nint";
		if (type == typeof(UIntPtr)) return "nuint";
		return null;
	}

	private static List<string?> GetTupleElementNames(ICustomAttributeProvider provider) => provider.GetCustomAttributes(typeof(TupleElementNamesAttribute), inherit: false).OfType<TupleElementNamesAttribute>().FirstOrDefault()?.TransformNames.ToList() ?? [];

	private static bool IsAbstractForSignature(MemberInfo member) => ReflectionFacts.IsAbstract(member) || member switch
	{
		MethodBase method => method.IsAbstract,
		PropertyInfo property => property.GetMethod?.IsAbstract == true || property.SetMethod?.IsAbstract == true,
		EventInfo @event => @event.AddMethod?.IsAbstract == true || @event.RemoveMethod?.IsAbstract == true,
		_ => false,
	};

	private static bool IsVirtualForSignature(MemberInfo member) => ReflectionFacts.IsVirtual(member) || member switch
	{
		MethodInfo method => method is { IsVirtual: true, IsFinal: false, IsAbstract: false },
		PropertyInfo property => IsVirtualAccessor(property.GetMethod) || IsVirtualAccessor(property.SetMethod),
		EventInfo @event => IsVirtualAccessor(@event.AddMethod) || IsVirtualAccessor(@event.RemoveMethod),
		_ => false,
	};

	private static bool IsVirtualAccessor(MethodInfo? method) => method is { IsVirtual: true, IsFinal: false, IsAbstract: false };

	private static bool IsReadOnlyRef(ParameterInfo parameter) => parameter.GetRequiredCustomModifiers().Contains(typeof(IsReadOnlyAttribute)) || HasAttribute(parameter, "System.Runtime.CompilerServices.IsReadOnlyAttribute");

	private static bool IsRefReadOnlyParameter(ParameterInfo parameter) => parameter.IsIn && HasAttribute(parameter, "System.Runtime.CompilerServices.RequiresLocationAttribute");

	private static bool IsScoped(ParameterInfo parameter) => parameter.GetCustomAttributes().Any(static x => x.GetType().FullName == "System.Runtime.CompilerServices.ScopedRefAttribute");

	private static bool HasAttribute(ParameterInfo parameter, string attributeName) => parameter.GetCustomAttributes(inherit: false).Any(x => x.GetType().FullName == attributeName);

	private static bool HasAttribute(Type type, string attributeName) => type.GetCustomAttributes(inherit: false).Any(x => x.GetType().FullName == attributeName);

	private static bool HasNotNullConstraint(Type type) => GetNullableConstraintFlag(type) == 1 && !type.GetTypeInfo().GenericParameterAttributes.HasFlag(GenericParameterAttributes.ReferenceTypeConstraint);

	private static bool HasNullableConstraint(Type type) => GetNullableConstraintFlag(type) == 2;

	private static byte? GetNullableConstraintFlag(Type type)
	{
		var attribute = type.GetCustomAttributes(inherit: false).FirstOrDefault(static x => x.GetType().FullName == "System.Runtime.CompilerServices.NullableAttribute");
		if (attribute is null)
			return null;

		if (attribute.GetType().GetField("NullableFlag")?.GetValue(attribute) is byte flag)
			return flag;
		if (attribute.GetType().GetField("NullableFlags")?.GetValue(attribute) is byte[] flags && flags.Length != 0)
			return flags[0];
		return null;
	}

	private static CSharpToken Keyword(string text) => new(CSharpTokenKind.Keyword, text);
	private static CSharpToken Identifier(string text) => new(CSharpTokenKind.Identifier, s_keywords.Contains(text) ? "@" + text : text);
	private static CSharpToken TypeName(string text, MemberInfo target) => new(CSharpTokenKind.TypeName, text, target);
	private static CSharpToken Operator(string text) => new(CSharpTokenKind.Operator, text);
	private static CSharpToken Punctuation(string text) => new(CSharpTokenKind.Punctuation, text);
	private static CSharpToken Space() => new(CSharpTokenKind.Whitespace, " ");
	private static CSharpToken Text(string text) => new(CSharpTokenKind.Text, text);
	private static CSharpToken Literal(string text) => new(CSharpTokenKind.Literal, text);

	private static readonly HashSet<string> s_keywords =
	[
		"abstract", "as", "base", "bool", "break", "byte", "case", "catch", "char", "checked", "class", "const", "continue", "decimal", "default", "delegate", "do", "double", "else", "enum", "event", "explicit", "extern", "false", "finally", "fixed", "float", "for", "foreach", "goto", "if", "implicit", "in", "int", "interface", "internal", "is", "lock", "long", "namespace", "new", "null", "object", "operator", "out", "override", "params", "private", "protected", "public", "readonly", "ref", "return", "sbyte", "sealed", "short", "sizeof", "stackalloc", "static", "string", "struct", "switch", "this", "throw", "true", "try", "typeof", "uint", "ulong", "unchecked", "unsafe", "ushort", "using", "virtual", "void", "volatile", "while",
	];
	private static readonly NullabilityInfoContext s_nullability = new();
}
