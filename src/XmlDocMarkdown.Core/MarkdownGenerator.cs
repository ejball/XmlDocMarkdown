using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace XmlDocMarkdown.Core
{
	public sealed class MarkdownGenerator
	{
		public string NewLine { get; set; }

		public IReadOnlyList<NamedText> GenerateOutput(Assembly assembly, XmlDocAssembly xmlDocAssembly)
		{
			return DoGenerateOutput(assembly, xmlDocAssembly).ToList();
		}

		private IEnumerable<NamedText> DoGenerateOutput(Assembly assembly, XmlDocAssembly xmlDocAssembly)
		{
			var assemblyName = assembly.GetName().Name;
			var assemblyFilePath = assembly.Modules.FirstOrDefault()?.FullyQualifiedName;
			var assemblyFileName = assemblyFilePath != null ? Path.GetFileName(assemblyFilePath) : assemblyName;

			var visibleTypes = assembly
				.ExportedTypes
				.Select(x => x.GetTypeInfo())
				.Where(x => !IsObsolete(x))
				.ToList();

			var membersByXmlDocName = visibleTypes
				.Where(x => new[] { TypeKind.Class, TypeKind.Struct, TypeKind.Interface }.Contains(GetTypeKind(x)))
				.SelectMany(x => x.DeclaredMembers)
				.Where(IsPublic)
				.Where(x => !(x is TypeInfo) && IsVisibleMember(x))
				.Concat(visibleTypes)
				.ToDictionary(XmlDocUtility.GetXmlDocRef, x => x);

			var context = new MarkdownContext(xmlDocAssembly, membersByXmlDocName, assemblyFileName);

			var visibleTypeRecords = visibleTypes
				.Select(typeInfo => new
				{
					TypeInfo = typeInfo,
					Path = GetTypeUriName(typeInfo) + ".md",
					ShortName = GetShortName(typeInfo),
					ShortSignature = GetShortSignature(typeInfo),
					Namespace = GetNamespaceName(typeInfo),
				})
				.ToList();

			var visibleNamespaceRecords = visibleTypeRecords
				.Where(x => x.TypeInfo.DeclaringType == null)
				.GroupBy(x => x.Namespace)
				.Select(ng => new
				{
					Namespace = ng.Key,
					Types = ng
						.OrderBy(x => x.ShortName, StringComparer.OrdinalIgnoreCase)
						.ThenBy(x => x.Path, StringComparer.OrdinalIgnoreCase)
						.ToList()
				})
				.OrderBy(x => x.Namespace, StringComparer.OrdinalIgnoreCase)
				.ToList();

			yield return CreateNamedText(GetAssemblyUriName(assembly) + ".md", writer =>
			{
				writer.WriteLine($"# {assemblyName} assembly");

				string namespaceCountText = visibleNamespaceRecords.Count == 1 ? "1 namespace" : $"{visibleNamespaceRecords.Count} namespaces";
				int typeCount = visibleNamespaceRecords.SelectMany(x => x.Types).Count();
				string typeCountText = typeCount == 1 ? "1 public type" : $"{typeCount} public types";

				writer.WriteLine();
				writer.WriteLine($"The assembly `{assemblyFileName}` has {typeCountText} in {namespaceCountText}.");

				foreach (var group in visibleNamespaceRecords)
				{
					writer.WriteLine();
					writer.WriteLine($"## {group.Namespace} namespace");

					writer.WriteLine();
					writer.WriteLine("| public type | description |");
					writer.WriteLine("| --- | --- |");
					foreach (var typeInfo in group.Types)
					{
						string typeText = GetShortSignatureMarkdown(typeInfo.ShortSignature, $"{GetNamespaceUriName(group.Namespace)}/{typeInfo.Path}");
						string summaryText = GetShortSummaryMarkdown(xmlDocAssembly, typeInfo.TypeInfo, context);
						writer.WriteLine($"| {typeText} | {summaryText} |");
					}
				}

				writer.WriteLine();
				writer.WriteLine(GetCodeGenComment(assemblyFileName));
			});

			foreach (var visibleTypeRecord in visibleTypeRecords)
			{
				yield return WriteMemberPage(
					path: $"{GetNamespaceUriName(visibleTypeRecord.Namespace)}/{visibleTypeRecord.Path}",
					memberInfo: visibleTypeRecord.TypeInfo,
					context: context);

				var typeKind = GetTypeKind(visibleTypeRecord.TypeInfo);
				if (typeKind == TypeKind.Class || typeKind == TypeKind.Struct || typeKind == TypeKind.Interface)
				{
					var memberGroups = visibleTypeRecord.TypeInfo
						.DeclaredMembers
						.Where(IsPublic)
						.Where(x => !(x is TypeInfo) && IsVisibleMember(x))
						.GroupBy(GetMemberUriName)
						.Select(tg => new
						{
							MemberUriName = tg.Key,
							Members = OrderMembers(tg, x => x).ToList(),
						})
						.ToList();

					foreach (var memberGroup in memberGroups)
					{
						yield return WriteMemberPage(
							path: $"{GetNamespaceUriName(visibleTypeRecord.Namespace)}/{GetTypeUriName(visibleTypeRecord.TypeInfo)}/{memberGroup.MemberUriName}.md",
							memberGroup: memberGroup.Members,
							context: context);
					}
				}
			}
		}

		private NamedText CreateNamedText(string name, Action<MarkdownWriter> writeTo)
		{
			using (var stringWriter = new StringWriter())
			{
				if (NewLine != null)
					stringWriter.NewLine = NewLine;

				var code = new MarkdownWriter(stringWriter);
				writeTo(code);
				return new NamedText(name, stringWriter.ToString());
			}
		}

		private static Collection<XmlDocBlock> GetSummary(XmlDocAssembly xmlDocAssembly, MemberInfo member)
		{
			return GetSummary(xmlDocAssembly.FindMember(XmlDocUtility.GetXmlDocRef(member)), member);
		}

		private static Collection<XmlDocBlock> GetSummary(XmlDocMember xmlDocMember, MemberInfo member)
		{
			var summary = xmlDocMember?.Summary;

			if (summary == null || summary.Count == 0)
			{
				var constructorInfo = member as ConstructorInfo;
				if (constructorInfo != null && !constructorInfo.IsStatic && constructorInfo.GetParameters().Length == 0)
					summary = new Collection<XmlDocBlock> { new XmlDocBlock { Inlines = { new XmlDocInline { Text = "The default constructor." } } } };
			}

			return summary;
		}

		private static string GetShortSummaryMarkdown(XmlDocAssembly xmlDocAssembly, MemberInfo member, MarkdownContext context)
		{
			return ToMarkdown(GetSummary(xmlDocAssembly, member)?.FirstOrDefault()?.Inlines, context) ?? "";
		}

		private static string GetAssemblyUriName(Assembly assembly)
		{
			return $"{assembly.GetName().Name}";
		}

		private static string GetNamespaceUriName(string namespaceName)
		{
			return namespaceName ?? "global";
		}

		private static string GetTypeUriName(TypeInfo typeInfo)
		{
			return GetFullTypeName(typeInfo, x =>
			{
				int genericTypeCount = x.GenericTypeParameters.Length;
				return GetShortName(x) + (genericTypeCount == 0 ? "" : $"-{genericTypeCount}");
			});
		}

		private static string GetMemberUriName(MemberInfo memberInfo)
		{
			var typeInfo = memberInfo as TypeInfo;
			return typeInfo != null ? GetTypeUriName(typeInfo) : GetShortName(memberInfo);
		}

		private static string GetShortSignatureMarkdown(ShortSignature shortSignature, string path)
		{
			return EscapeHtml($"{shortSignature.Prefix}[{shortSignature.Name}]({path}){shortSignature.Suffix}");
		}

		private static string EscapeHtml(string value)
		{
			return value.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");
		}

		private NamedText WriteMemberPage(string path, MemberInfo memberInfo, MarkdownContext context)
		{
			return WriteMemberPage(path, new[] { memberInfo }, context);
		}

		private NamedText WriteMemberPage(string path, IReadOnlyList<MemberInfo> memberGroup, MarkdownContext context)
		{
			return CreateNamedText(path, writer =>
			{
				for (int memberIndex = 0; memberIndex < memberGroup.Count; memberIndex++)
				{
					var memberInfo = memberGroup[memberIndex];
					var memberContext = new MarkdownContext(context, memberInfo);
					var typeInfo = memberInfo as TypeInfo;
					var typeKind = typeInfo == null ? default(TypeKind?) : GetTypeKind(typeInfo);

					if (memberIndex != 0)
						writer.WriteLine();

					writer.WriteLine($"# {EscapeHtml(GetMemberHeading(memberGroup, memberIndex))}");

					var xmlDocMember = memberContext.XmlDocAssembly.FindMember(XmlDocUtility.GetXmlDocRef(memberInfo));

					var summary = GetSummary(xmlDocMember, memberInfo);
					if (summary != null && summary.Count != 0)
					{
						writer.WriteLine();
						writer.WriteLines(ToMarkdown(summary, memberContext));
					}

					var seeAlsoMembers = new List<MemberInfo>();
					if (xmlDocMember != null)
					{
						foreach (var seeAlsoInfo in xmlDocMember.SeeAlso)
						{
							string xmlDocName = seeAlsoInfo.Ref;
							MemberInfo seeAlsoMember;
							if (memberContext.MembersByXmlDocName.TryGetValue(xmlDocName, out seeAlsoMember))
								seeAlsoMembers.Add(seeAlsoMember);
						}
					}

					writer.WriteLine();
					writer.WriteLine("```csharp");
					writer.WriteLine(GetFullSignature(memberInfo, seeAlsoMembers));
					writer.WriteLine("```");

					if (xmlDocMember != null)
					{
						var typeParameters = xmlDocMember.TypeParameters;
						var parameters = xmlDocMember.Parameters;
						if (typeParameters.Count + parameters.Count > 0)
						{
							writer.WriteLine();
							writer.WriteLine("| parameter | description |");
							writer.WriteLine("| --- | --- |");
							foreach (var typeParameter in typeParameters)
							{
								string description = ToMarkdown(typeParameter.Description.FirstOrDefault()?.Inlines, memberContext) ?? "";
								writer.WriteLine($"| {typeParameter.Name} | {description} |");
							}
							foreach (var parameter in parameters)
							{
								string description = ToMarkdown(parameter.Description.FirstOrDefault()?.Inlines, memberContext) ?? "";
								writer.WriteLine($"| {parameter.Name} | {description} |");
							}
						}
					}

					if (typeKind == TypeKind.Enum)
					{
						writer.WriteLine();
						writer.WriteLine("## Values");
						writer.WriteLine();
						writer.WriteLine("| name | value | description |");
						writer.WriteLine("| --- | --- | --- |");

						bool isFlags = IsFlagsEnum(typeInfo);
						foreach (var enumValue in typeInfo.DeclaredMembers.OfType<FieldInfo>().Where(x => x.IsPublic && x.IsLiteral))
						{
							object valueObject = enumValue.GetValue(null);
							string valueText = isFlags ? "0x" + Convert.ToString(Convert.ToInt64(valueObject), 16).ToUpperInvariant() :
								Enum.GetUnderlyingType(typeInfo.AsType()) == typeof(ulong) ? Convert.ToString(Convert.ToUInt64(valueObject)) :
									Convert.ToString(Convert.ToInt64(valueObject));
							string description = GetShortSummaryMarkdown(memberContext.XmlDocAssembly, enumValue, memberContext);
							writer.WriteLine($"| {enumValue.Name} | `{valueText}` | {description} |");
						}
					}
					else if (typeKind == TypeKind.Class || typeKind == TypeKind.Struct || typeKind == TypeKind.Interface)
					{
						var innerMemberGroups = OrderMembers(typeInfo
							.DeclaredMembers
							.Where(IsPublic)
							.Where(IsVisibleMember)
							.GroupBy(x => GetShortSignature(x))
							.Select(tg => new
							{
								ShortSignature = tg.Key,
								Members = tg.OrderBy(x => (x as TypeInfo)?.GenericTypeParameters.Length ?? 0).ToList()
							}), x => x.Members[0]).ToList();

						if (innerMemberGroups.Count != 0)
						{
							writer.WriteLine();
							writer.WriteLine(typeKind == TypeKind.Interface ? "## Members" : "## Public Members");
							writer.WriteLine();
							writer.WriteLine("| name | description |");
							writer.WriteLine("| --- | --- |");

							foreach (var innerMemberGroup in innerMemberGroups)
							{
								var innerMembers = innerMemberGroup.Members;
								var firstInnerMember = innerMembers[0];
								string memberPath = firstInnerMember is TypeInfo ?
									$"{GetMemberUriName(firstInnerMember)}.md" :
									$"{GetTypeUriName(typeInfo)}/{GetMemberUriName(firstInnerMember)}.md";
								string memberText = GetShortSignatureMarkdown(innerMemberGroup.ShortSignature, memberPath);
								string summaryText = GetShortSummaryMarkdown(memberContext.XmlDocAssembly, firstInnerMember, memberContext);
								if (innerMembers.Count != 1)
									summaryText += $" ({innerMembers.Count} {GetMemberGroupNoun(innerMembers)})";

								writer.WriteLine($"| {memberText} | {summaryText} |");
							}
						}
					}

					var returnValue = xmlDocMember?.ReturnValue;
					if (returnValue != null && returnValue.Count != 0)
					{
						writer.WriteLine();
						writer.WriteLine("## Return Value");
						writer.WriteLine();
						writer.WriteLines(ToMarkdown(returnValue, memberContext));
					}

					var propertyValue = xmlDocMember?.PropertyValue;
					if (propertyValue != null && propertyValue.Count != 0)
					{
						writer.WriteLine();
						writer.WriteLine("## Property Value");
						writer.WriteLine();
						writer.WriteLines(ToMarkdown(propertyValue, memberContext));
					}

					var exceptions = xmlDocMember?.Exceptions;
					if (exceptions != null && exceptions.Count != 0)
					{
						writer.WriteLine();
						writer.WriteLine("## Exceptions");
						writer.WriteLine();
						writer.WriteLine("| exception | condition |");
						writer.WriteLine("| --- | --- |");

						foreach (var exception in exceptions)
						{
							MemberInfo exceptionMemberInfo;
							memberContext.MembersByXmlDocName.TryGetValue(exception.ExceptionTypeRef, out exceptionMemberInfo);
							string text = exceptionMemberInfo != null ? GetShortName(exceptionMemberInfo) : XmlDocUtility.GetShortNameForXmlDocRef(exception.ExceptionTypeRef);
							string link = WrapMarkdownRefLink(text, exceptionMemberInfo, memberContext);
							writer.WriteLine($"| {link} | {ToMarkdown(exception.Condition?.FirstOrDefault()?.Inlines, memberContext) ?? ""} |");
						}
					}

					var remarks = xmlDocMember?.Remarks;
					if (remarks != null && remarks.Count != 0)
					{
						writer.WriteLine();
						writer.WriteLine("## Remarks");
						writer.WriteLine();
						writer.WriteLines(ToMarkdown(remarks, memberContext));
					}

					var examples = xmlDocMember?.Examples;
					if (examples != null && examples.Count != 0)
					{
						writer.WriteLine();
						writer.WriteLine("## Examples");
						writer.WriteLine();
						writer.WriteLines(ToMarkdown(examples, memberContext));
					}

					writer.WriteLine();
					writer.WriteLine("## See Also");
					writer.WriteLine();

					var declaringType = memberInfo.DeclaringType?.GetTypeInfo();
					string declaringTypeXmlDocName = declaringType == null ? null : XmlDocUtility.GetXmlDocRef(declaringType);
					if (declaringType != null)
						seeAlsoMembers.Add(declaringType);

					foreach (var seeAlso in seeAlsoMembers
						.Select(GetGenericDefinition)
						.GroupBy(XmlDocUtility.GetXmlDocRef)
						.Select(x => new { Member = x.First(), XmlDocName = x.Key })
						.OrderBy(x => x.XmlDocName == declaringTypeXmlDocName)
						.Where(x => memberContext.MembersByXmlDocName.ContainsKey(x.XmlDocName)))
					{
						var shortSignature = GetShortSignature(seeAlso.Member, forSeeAlso: true);
						writer.WriteLine("* " + shortSignature.Prefix +
							WrapMarkdownRefLink(shortSignature.Name, seeAlso.Member, memberContext) + shortSignature.Suffix);
					}

					writer.WriteLine("* " + $"namespace\u00A0[{GetNamespaceName(declaringType ?? typeInfo)}](../{(typeInfo != null ? "" : "../")}{GetAssemblyUriName((declaringType ?? typeInfo).Assembly)}.md)");

					if (memberIndex < memberGroup.Count - 1)
					{
						writer.WriteLine();
						writer.WriteLine("---");
					}
				}

				writer.WriteLine();
				writer.WriteLine(GetCodeGenComment(context.AssemblyFileName));
			});
		}

		private MemberInfo GetGenericDefinition(MemberInfo memberInfo)
		{
			var typeInfo = memberInfo as TypeInfo;
			if (typeInfo != null)
				return typeInfo.IsGenericType ? typeInfo.GetGenericTypeDefinition().GetTypeInfo() : typeInfo;

			var methodInfo = memberInfo as MethodInfo;
			if (methodInfo != null)
				return methodInfo.IsGenericMethod ? methodInfo.GetGenericMethodDefinition() : methodInfo;

			return memberInfo;
		}

		private static bool IsVisibleMember(MemberInfo memberInfo)
		{
			return !(memberInfo is MethodBase) || memberInfo is ConstructorInfo || !((MethodBase) memberInfo).IsSpecialName;
		}

		private static string GetMemberHeading(IReadOnlyList<MemberInfo> membersInfos, int index)
		{
			var heading = $"{GetFullMemberName(membersInfos[index])} {GetMemberGroupNoun(new[] { membersInfos[index] })}";
			if (membersInfos.Count > 1)
				heading += $" ({index + 1} of {membersInfos.Count})";
			return heading;
		}

		private static string GetMemberGroupNoun(IReadOnlyList<MemberInfo> memberInfos)
		{
			bool plural = memberInfos.Count != 1;

			if (memberInfos.All(x => x is ConstructorInfo))
				return plural ? "constructors" : "constructor";
			else if (memberInfos.All(x => x is PropertyInfo))
				return plural ? "properties" : "property";
			else if (memberInfos.All(x => x is MethodInfo))
				return plural ? "methods" : "method";
			else if (memberInfos.All(x => x is EventInfo))
				return plural ? "events" : "event";
			else if (memberInfos.All(x => x is FieldInfo))
				return plural ? "fields" : "field";
			else if (memberInfos.All(x => x is TypeInfo))
				return GetTypeGroupNoun(memberInfos.Cast<TypeInfo>().ToList());
			else
				return plural ? "members" : "member";
		}

		private static string GetTypeGroupNoun(IReadOnlyList<TypeInfo> typeInfos)
		{
			bool plural = typeInfos.Count != 1;

			var typeKinds = typeInfos.Select(GetTypeKind).ToList();
			if (typeKinds.All(x => x == TypeKind.Class))
				return plural ? "classes" : "class";
			else if (typeKinds.All(x => x == TypeKind.Interface))
				return plural ? "interfaces" : "interface";
			else if (typeKinds.All(x => x == TypeKind.Struct))
				return plural ? "structures" : "structure";
			else if (typeKinds.All(x => x == TypeKind.Enum))
				return plural ? "enumerations" : "enumeration";
			else if (typeKinds.All(x => x == TypeKind.Delegate))
				return plural ? "delegates" : "delegate";
			else
				return plural ? "types" : "type";
		}

		private static string GetNamespaceName(TypeInfo typeInfo)
		{
			return typeInfo.Namespace ?? "global";
		}

		private static string GetShortName(MemberInfo memberInfo)
		{
			string name = memberInfo.Name;

			int tickIndex = name.IndexOf('`');
			if (tickIndex != -1)
				name = name.Substring(0, tickIndex);

			if (name == ".ctor")
				name = GetShortName(memberInfo.DeclaringType.GetTypeInfo());

			return name;
		}

		private static string GetFullMemberName(MemberInfo memberInfo)
		{
			var type = memberInfo as TypeInfo;
			if (type != null)
				return GetFullTypeName(type, t => GetShortName(t) + RenderShortGenericParameters(t.GenericTypeParameters));

			if (memberInfo is ConstructorInfo)
				return GetFullTypeName(memberInfo.DeclaringType.GetTypeInfo(), t => GetShortName(t) + RenderShortGenericParameters(t.GenericTypeParameters));

			return GetFullTypeName(memberInfo.DeclaringType.GetTypeInfo(), t => GetShortName(t) + RenderShortGenericParameters(t.GenericTypeParameters)) + "." +
				GetShortName(memberInfo) + RenderShortGenericParameters(GetGenericArguments(memberInfo));
		}

		private static string GetFullTypeName(TypeInfo typeInfo, Func<TypeInfo, string> render)
		{
			string name = render(typeInfo);
			if (typeInfo.DeclaringType != null)
				name = $"{GetFullTypeName(typeInfo.DeclaringType.GetTypeInfo(), render)}.{name}";
			return name;
		}

		private sealed class ShortSignature : IEquatable<ShortSignature>
		{
			public ShortSignature(string prefix, string name, string suffix)
			{
				Prefix = prefix;
				Name = name;
				Suffix = suffix;
			}

			public string Prefix { get; }

			public string Name { get; }

			public string Suffix { get; }

			public bool Equals(ShortSignature other)
			{
				return other != null && other.ToString() == ToString();
			}

			public override bool Equals(object obj)
			{
				return Equals(obj as ShortSignature);
			}

			public override int GetHashCode()
			{
				return ToString().GetHashCode();
			}

			public override string ToString()
			{
				return Prefix + Name + Suffix;
			}
		}

		private static ShortSignature GetShortSignature(MemberInfo memberInfo, bool forSeeAlso = false)
		{
			string name = GetShortName(memberInfo);
			string prefix = "";
			string suffix = "";

			var typeInfo = memberInfo as TypeInfo;
			if (typeInfo != null)
			{
				name += RenderShortGenericParameters(typeInfo.GenericTypeParameters);

				switch (GetTypeKind(typeInfo))
				{
				case TypeKind.Class:
					prefix = "class ";
					break;
				case TypeKind.Interface:
					prefix = "interface ";
					break;
				case TypeKind.Struct:
					prefix = "struct ";
					break;
				case TypeKind.Enum:
					prefix = "enum ";
					break;
				case TypeKind.Delegate:
					prefix = "delegate ";
					break;
				}

				if (!forSeeAlso && IsStatic(typeInfo))
					prefix = "static " + prefix;

				if (!forSeeAlso && IsFlagsEnum(typeInfo))
					prefix = "[Flags] " + prefix;
			}
			else
			{
				var eventInfo = memberInfo as EventInfo;
				var propertyInfo = memberInfo as PropertyInfo;
				var fieldInfo = memberInfo as FieldInfo;
				var methodBase = memberInfo as MethodBase;

				if (eventInfo != null)
				{
					prefix = "event ";
					if (!forSeeAlso && IsStatic(eventInfo))
						prefix = "static " + prefix;
				}
				else if (propertyInfo != null)
				{
					if (!forSeeAlso)
						suffix = GetPropertyGetSet(propertyInfo);

					if (forSeeAlso)
						prefix = "property ";
					else if (IsStatic(propertyInfo))
						prefix = "static ";
				}
				else if (fieldInfo != null)
				{
					if (forSeeAlso)
					{
						prefix = "field ";
					}
					else
					{
						if (IsConst(fieldInfo))
							prefix += "const ";
						if (IsStatic(fieldInfo))
							prefix = "static ";
						if (IsReadOnly(fieldInfo))
							prefix += "readonly ";
					}
				}
				else if (methodBase != null)
				{
					if (methodBase is MethodInfo)
						name += RenderShortGenericParameters(methodBase.GetGenericArguments());

					if (!forSeeAlso)
						suffix += methodBase.GetParameters().Length == 0 ? "()" : "(…)";

					if (forSeeAlso)
						prefix = "method ";
					else if (IsStatic(methodBase))
						prefix = "static " + prefix;
				}
			}

			return new ShortSignature(prefix: prefix.Replace(' ', '\u00A0'), name: name, suffix: suffix.Replace(' ', '\u00A0'));
		}

		private static string GetPropertyGetSet(PropertyInfo propertyInfo)
		{
			bool hasGet = propertyInfo.GetMethod?.IsPublic ?? false;
			bool hasSet = propertyInfo.SetMethod?.IsPublic ?? false;
			return hasGet && hasSet ? " { get; set; }" : hasGet ? " { get; }" : hasSet ? " { set; }" : "";
		}

		private static string GetFullSignature(MemberInfo memberInfo, ICollection<MemberInfo> seeAlsoMembers)
		{
			var stringBuilder = new StringBuilder();
			var lineBuilder = new StringBuilder();
			var segmentBuilder = new StringBuilder();
			const int maxLineLength = 112;

			foreach (string part in GetFullSignatureParts(memberInfo, seeAlsoMembers))
			{
				if (part == Environment.NewLine)
				{
					if (lineBuilder.Length != 0 && segmentBuilder.Length != 0 && lineBuilder.Length + segmentBuilder.Length > maxLineLength)
					{
						lineBuilder.AppendLine();
						stringBuilder.Append(lineBuilder);
						lineBuilder.Clear();

						lineBuilder.Append("    ");
					}

					lineBuilder.Append(segmentBuilder);
					segmentBuilder.Clear();

					lineBuilder.AppendLine();
					stringBuilder.Append(lineBuilder);
					lineBuilder.Clear();
				}
				else if (part.Length == 0)
				{
					if (lineBuilder.Length != 0 && segmentBuilder.Length != 0 && lineBuilder.Length + segmentBuilder.Length > maxLineLength)
					{
						lineBuilder.AppendLine();
						stringBuilder.Append(lineBuilder);
						lineBuilder.Clear();

						lineBuilder.Append("    ");
					}

					lineBuilder.Append(segmentBuilder);
					segmentBuilder.Clear();
				}
				else
				{
					segmentBuilder.Append(part);
				}
			}

			lineBuilder.Append(segmentBuilder);
			stringBuilder.Append(lineBuilder);
			return stringBuilder.ToString();
		}

		private static IEnumerable<string> GetFullSignatureParts(MemberInfo memberInfo, ICollection<MemberInfo> seeAlsoMembers)
		{
			var typeInfo = memberInfo as TypeInfo;
			var typeKind = typeInfo == null ? default(TypeKind?) : GetTypeKind(typeInfo);

			var attributeUsage = memberInfo.GetCustomAttribute<AttributeUsageAttribute>();
			if (attributeUsage != null)
			{
				yield return "[AttributeUsage(";

				yield return RenderConstant(attributeUsage.ValidOn);

				if (!attributeUsage.Inherited)
					yield return ", Inherited = false";

				if (attributeUsage.AllowMultiple)
					yield return ", AllowMultiple = true";

				yield return ")]";
				yield return Environment.NewLine;
			}

			if (IsFlagsEnum(memberInfo))
			{
				yield return "[Flags]";
				yield return Environment.NewLine;
			}

			yield return "public ";

			if (IsStatic(memberInfo))
			{
				yield return "static ";
			}
			else if (typeKind == TypeKind.Class)
			{
				if (typeInfo.IsAbstract)
					yield return "abstract ";
				if (typeInfo.IsSealed)
					yield return "sealed ";
			}

			if (IsConst(memberInfo))
				yield return "const ";
			if (IsReadOnly(memberInfo))
				yield return "readonly ";

			if (memberInfo is EventInfo)
				yield return "event ";

			switch (typeKind)
			{
			case TypeKind.Class:
				yield return "class ";
				break;
			case TypeKind.Interface:
				yield return "interface ";
				break;
			case TypeKind.Struct:
				yield return "struct ";
				break;
			case TypeKind.Enum:
				yield return "enum ";
				break;
			case TypeKind.Delegate:
				yield return "delegate ";
				break;
			}

			var valueType = GetValueType(memberInfo)?.GetTypeInfo();
			if (valueType != null)
			{
				yield return RenderTypeName(valueType, seeAlsoMembers);
				yield return " ";
			}

			yield return GetShortName(memberInfo);

			var genericParameters = GetGenericArguments(memberInfo);
			yield return RenderGenericParameters(genericParameters);

			if (typeKind == TypeKind.Class || typeKind == TypeKind.Struct || typeKind == TypeKind.Interface)
			{
				bool isFirstBase = true;
				if (typeKind == TypeKind.Class && typeInfo.BaseType != typeof(object))
				{
					yield return " : ";
					yield return RenderTypeName(typeInfo.BaseType.GetTypeInfo(), seeAlsoMembers);
					isFirstBase = false;
				}

				var baseInterfaces = typeInfo.ImplementedInterfaces.Select(x => x.GetTypeInfo()).Where(x => x.IsPublic).ToList();
				foreach (var baseInterface in baseInterfaces)
				{
					if (!(typeKind == TypeKind.Class && baseInterface.IsAssignableFrom(typeInfo.BaseType.GetTypeInfo())) &&
						!baseInterfaces.Any(x => XmlDocUtility.GetXmlDocRef(x) != XmlDocUtility.GetXmlDocRef(baseInterface) && baseInterface.IsAssignableFrom(x)))
					{
						yield return isFirstBase ? " : " : ", ";
						yield return RenderTypeName(baseInterface, seeAlsoMembers);
						isFirstBase = false;
					}
				}
			}

			if (typeKind == TypeKind.Enum && Enum.GetUnderlyingType(typeInfo.AsType()) != typeof(int))
			{
				yield return " : ";
				yield return RenderTypeName(Enum.GetUnderlyingType(typeInfo.AsType()).GetTypeInfo(), seeAlsoMembers);
			}

			var propertyInfo = memberInfo as PropertyInfo;
			if (propertyInfo != null)
				yield return GetPropertyGetSet(propertyInfo);

			var methodInfo = memberInfo as MethodBase ?? TryGetDelegateInvoke(memberInfo);
			if (methodInfo != null)
			{
				yield return "(";

				bool isFirstParameter = true;
				foreach (var parameterInfo in methodInfo.GetParameters())
				{
					if (isFirstParameter)
					{
						isFirstParameter = false;
					}
					else
					{
						yield return ", ";
						yield return "";
					}

					if (parameterInfo.ParameterType.IsByRef)
						yield return parameterInfo.IsOut ? "out " : "ref ";

					yield return RenderTypeName(parameterInfo.ParameterType.GetTypeInfo(), seeAlsoMembers);

					yield return " ";
					if (IsKeyword(parameterInfo.Name))
						yield return "@";
					yield return parameterInfo.Name;

					if (ParameterHasDefaultValue(parameterInfo))
					{
						yield return " = ";
						if (CanRenderParameterConstant(parameterInfo))
							yield return RenderConstant(parameterInfo.DefaultValue);
						else if (parameterInfo.ParameterType.GetTypeInfo().IsValueType || parameterInfo.ParameterType.IsGenericParameter)
							yield return $"default({RenderTypeName(parameterInfo.ParameterType.GetTypeInfo())})";
						else
							yield return "null";
					}
				}

				yield return ")";
			}

			if (genericParameters != null)
			{
				foreach (var genericParameter in genericParameters)
				{
					bool isFirstPart = true;

					if (genericParameter.GetTypeInfo().GenericParameterAttributes.HasFlag(GenericParameterAttributes.ReferenceTypeConstraint))
					{
						yield return Environment.NewLine;
						yield return $"    where {genericParameter.Name} : ";

						yield return "class";
						isFirstPart = false;
					}

					bool isStruct = genericParameter.GetTypeInfo().GenericParameterAttributes.HasFlag(GenericParameterAttributes.NotNullableValueTypeConstraint);
					if (isStruct)
					{
						if (isFirstPart)
						{
							yield return Environment.NewLine;
							yield return $"    where {genericParameter.Name} : ";
						}
						else
						{
							yield return ", ";
						}

						yield return "struct";
						isFirstPart = false;
					}

					var genericConstraints = genericParameter.GetTypeInfo().GetGenericParameterConstraints();
					foreach (var genericConstraint in genericConstraints.Where(x => x != typeof(ValueType)))
					{
						if (isFirstPart)
						{
							yield return Environment.NewLine;
							yield return $"    where {genericParameter.Name} : ";
						}
						else
						{
							yield return ", ";
						}

						yield return RenderTypeName(genericConstraint.GetTypeInfo(), seeAlsoMembers);
						isFirstPart = false;
					}

					if (!isStruct && genericParameter.GetTypeInfo().GenericParameterAttributes.HasFlag(GenericParameterAttributes.DefaultConstructorConstraint))
					{
						if (isFirstPart)
						{
							yield return Environment.NewLine;
							yield return $"    where {genericParameter.Name} : ";
						}
						else
						{
							yield return ", ";
						}

						yield return "new()";
					}
				}
			}

			if (typeKind == TypeKind.Delegate || memberInfo is EventInfo || memberInfo is FieldInfo)
				yield return ";";
		}

		private static Type GetValueType(MemberInfo member)
		{
			var eventInfo = member as EventInfo;
			if (eventInfo != null)
				return eventInfo.EventHandlerType;

			var propertyInfo = member as PropertyInfo;
			if (propertyInfo != null)
				return propertyInfo.PropertyType;

			var fieldInfo = member as FieldInfo;
			if (fieldInfo != null)
				return fieldInfo.FieldType;

			var methodInfo = member as MethodInfo ?? TryGetDelegateInvoke(member);
			if (methodInfo != null)
				return methodInfo.ReturnType;

			return null;
		}

		private static bool ParameterHasDefaultValue(ParameterInfo parameterInfo)
		{
			if (parameterInfo.Attributes.HasFlag(ParameterAttributes.HasDefault))
				return true;

			if (parameterInfo.ParameterType == typeof(decimal) || parameterInfo.ParameterType == typeof(decimal?))
				return parameterInfo.HasDefaultValue;

			return false;
		}

		private static bool CanRenderParameterConstant(ParameterInfo parameterInfo)
		{
			return TryGetBuiltInTypeName(parameterInfo.ParameterType) != null ||
				TryGetBuiltInTypeName(Nullable.GetUnderlyingType(parameterInfo.ParameterType)) != null ||
				parameterInfo.ParameterType.GetTypeInfo().IsEnum;
		}

		private static string RenderConstant(object value)
		{
			if (value == null)
				return "null";

			if (value is bool)
				return (bool) value ? "true" : "false";

			char? valueAsChar = value as char?;
			if (valueAsChar != null)
				return RenderChar(valueAsChar.Value);

			string valueAsString = value as string;
			if (valueAsString != null)
				return RenderString(valueAsString);

			Type type = value.GetType();
			if (type.GetTypeInfo().IsEnum)
			{
				return string.Join(" | ", value.ToString()
					.Split(new[] { ", " }, StringSplitOptions.None)
					.Select(x => $"{type.Name}.{x}"));
			}

			string rendered = Convert.ToString(value, CultureInfo.InvariantCulture);

			if (value is double)
				rendered += "m";

			return rendered;
		}

		private static string RenderString(string value)
		{
			var builder = new StringBuilder("\"");
			foreach (char ch in value)
				builder.Append(ch == '\'' ? "'" : EscapeChar(ch));
			return builder.Append('"').ToString();
		}

		private static string RenderChar(char ch)
		{
			return "'" + (ch == '\"' ? "\"" : EscapeChar(ch)) + "'";
		}

		private static string EscapeChar(char ch)
		{
			switch (ch)
			{
			case '\'':
				return @"\'";
			case '\"':
				return @"\""";
			case '\\':
				return @"\\";
			case '\a':
				return @"\a";
			case '\b':
				return @"\b";
			case '\f':
				return @"\f";
			case '\n':
				return @"\n";
			case '\r':
				return @"\r";
			case '\t':
				return @"\t";
			case '\v':
				return @"\v";
			default:
				return char.IsControl(ch) ? $"\\u{((int) ch):x4}" : ch.ToString();
			}
		}

		private static string RenderGenericParameters(Type[] genericParameters)
		{
			if (genericParameters == null)
				return "";

			var stringBuilder = new StringBuilder();
			for (int index = 0; index < genericParameters.Length; index++)
			{
				var genericParameter = genericParameters[index];
				stringBuilder.Append((index == 0 ? "<" : "") +
					(genericParameter.GetTypeInfo().GenericParameterAttributes.HasFlag(GenericParameterAttributes.Covariant) ? "out " : "") +
					(genericParameter.GetTypeInfo().GenericParameterAttributes.HasFlag(GenericParameterAttributes.Contravariant) ? "in " : "") +
					genericParameter.Name +
					(index < genericParameters.Length - 1 ? ", " : "") +
					(index == genericParameters.Length - 1 ? ">" : ""));
			}
			return stringBuilder.ToString();
		}

		private static string RenderShortGenericParameters(Type[] genericParameters)
		{
			if (genericParameters == null)
				return "";

			var stringBuilder = new StringBuilder();
			for (int index = 0; index < genericParameters.Length; index++)
			{
				var genericParameter = genericParameters[index];
				stringBuilder.Append((index == 0 ? "<" : "") +
					genericParameter.Name +
					(index < genericParameters.Length - 1 ? "," : "") +
					(index == genericParameters.Length - 1 ? ">" : ""));
			}
			return stringBuilder.ToString();
		}

		private static string RenderTypeName(TypeInfo typeInfo, ICollection<MemberInfo> seeAlso = null)
		{
			if (typeInfo.IsArray)
				return $"{RenderTypeName(typeInfo.GetElementType().GetTypeInfo(), seeAlso)}[]";

			if (typeInfo.IsByRef)
				return RenderTypeName(typeInfo.GetElementType().GetTypeInfo(), seeAlso);

			var nullableOfType = Nullable.GetUnderlyingType(typeInfo.AsType());
			if (nullableOfType != null)
				return $"{RenderTypeName(nullableOfType.GetTypeInfo(), seeAlso)}?";

			string builtIn = TryGetBuiltInTypeName(typeInfo.AsType());
			if (builtIn != null)
				return builtIn;

			seeAlso?.Add(typeInfo);

			return GetShortName(typeInfo) + RenderGenericArguments(typeInfo.GenericTypeArguments, seeAlso);
		}

		private static string RenderGenericArguments(Type[] genericArguments, ICollection<MemberInfo> seeAlso)
		{
			if (genericArguments == null)
				return "";

			var stringBuilder = new StringBuilder();
			for (int index = 0; index < genericArguments.Length; index++)
			{
				var genericArgument = genericArguments[index];
				stringBuilder.Append((index == 0 ? "<" : "") +
					RenderTypeName(genericArgument.GetTypeInfo(), seeAlso) +
					(index < genericArguments.Length - 1 ? ", " : "") +
					(index == genericArguments.Length - 1 ? ">" : ""));
			}
			return stringBuilder.ToString();
		}

		private static string TryGetBuiltInTypeName(Type type)
		{
			if (type == typeof(void))
				return "void";
			else if (type == typeof(bool))
				return "bool";
			else if (type == typeof(byte))
				return "byte";
			else if (type == typeof(sbyte))
				return "sbyte";
			else if (type == typeof(char))
				return "char";
			else if (type == typeof(decimal))
				return "decimal";
			else if (type == typeof(double))
				return "double";
			else if (type == typeof(float))
				return "float";
			else if (type == typeof(int))
				return "int";
			else if (type == typeof(uint))
				return "uint";
			else if (type == typeof(long))
				return "long";
			else if (type == typeof(ulong))
				return "ulong";
			else if (type == typeof(object))
				return "object";
			else if (type == typeof(short))
				return "short";
			else if (type == typeof(ushort))
				return "ushort";
			else if (type == typeof(string))
				return "string";
			else
				return null;
		}

		private static bool IsStatic(MemberInfo memberInfo)
		{
			var typeInfo = memberInfo as TypeInfo;
			if (typeInfo != null)
				return typeInfo.IsClass && typeInfo.IsAbstract && typeInfo.IsSealed;

			var eventInfo = memberInfo as EventInfo;
			if (eventInfo != null)
				return eventInfo.AddMethod.IsStatic;

			var propertyInfo = memberInfo as PropertyInfo;
			if (propertyInfo != null)
				return (propertyInfo.GetMethod ?? propertyInfo.SetMethod)?.IsStatic ?? false;

			var fieldInfo = memberInfo as FieldInfo;
			if (fieldInfo != null)
				return fieldInfo.IsStatic && !fieldInfo.IsLiteral;

			var methodBase = memberInfo as MethodBase;
			if (methodBase != null)
				return methodBase.IsStatic;

			return false;
		}

		private static bool IsConst(MemberInfo memberInfo)
		{
			return (memberInfo as FieldInfo)?.IsLiteral ?? false;
		}

		private static bool IsReadOnly(MemberInfo memberInfo)
		{
			return (memberInfo as FieldInfo)?.IsInitOnly ?? false;
		}

		private static bool IsFlagsEnum(MemberInfo memberInfo)
		{
			var type = memberInfo as TypeInfo;
			return type != null && type.IsEnum && type.GetCustomAttributes<FlagsAttribute>().Any();
		}

		private bool IsPublic(MemberInfo memberInfo)
		{
			var typeInfo = memberInfo as TypeInfo;
			if (typeInfo != null)
				return typeInfo.IsPublic || typeInfo.IsNestedPublic;

			var eventInfo = memberInfo as EventInfo;
			if (eventInfo != null)
				return (eventInfo.AddMethod?.IsPublic ?? false) || (eventInfo.RemoveMethod?.IsPublic ?? false) || (eventInfo.RaiseMethod?.IsPublic ?? false);

			var propertyInfo = memberInfo as PropertyInfo;
			if (propertyInfo != null)
				return (propertyInfo.GetMethod?.IsPublic ?? false) || (propertyInfo.SetMethod?.IsPublic ?? false);

			var fieldInfo = memberInfo as FieldInfo;
			if (fieldInfo != null)
				return fieldInfo.IsPublic;

			var methodBase = memberInfo as MethodBase;
			if (methodBase != null)
				return methodBase.IsPublic;

			return false;
		}

		private static bool IsObsolete(MemberInfo memberInfo)
		{
			return memberInfo.GetCustomAttributes<ObsoleteAttribute>().Any();
		}

		private enum TypeKind
		{
			Unknown,
			Class,
			Interface,
			Struct,
			Enum,
			Delegate,
		}

		private static TypeKind GetTypeKind(TypeInfo typeInfo)
		{
			if (typeof(Delegate).GetTypeInfo().IsAssignableFrom(typeInfo))
				return TypeKind.Delegate;
			else if (typeInfo.IsClass)
				return TypeKind.Class;
			else if (typeInfo.IsInterface)
				return TypeKind.Interface;
			else if (typeInfo.IsEnum)
				return TypeKind.Enum;
			else if (typeInfo.IsValueType)
				return TypeKind.Struct;
			else
				return TypeKind.Unknown;
		}

		private enum MemberOrder
		{
			Constructor,
			LifetimeProperty,
			LifetimeField,
			LifetimeMethod,
			InstanceProperty,
			InstanceField,
			InstanceEvent,
			InstanceMethod,
			StaticProperty,
			StaticField,
			StaticEvent,
			StaticMethod,
			Type,
			Unknown,
		}

		private static MemberOrder GetMemberOrder(MemberInfo memberInfo)
		{
			if (memberInfo is TypeInfo)
				return MemberOrder.Type;
			else if (memberInfo is ConstructorInfo)
				return MemberOrder.Constructor;
			else if (memberInfo is PropertyInfo)
				return GetPropertyOrder((PropertyInfo) memberInfo);
			else if (memberInfo is EventInfo)
				return GetEventOrder((EventInfo) memberInfo);
			else if (memberInfo is MethodInfo)
				return GetMethodOrder((MethodInfo) memberInfo);
			else if (memberInfo is FieldInfo)
				return GetFieldOrder((FieldInfo) memberInfo);
			else
				return MemberOrder.Unknown;
		}

		private static MemberOrder GetPropertyOrder(PropertyInfo propertyInfo)
		{
			var method = propertyInfo.GetMethod ?? propertyInfo.SetMethod;
			if (!method.IsStatic)
				return MemberOrder.InstanceProperty;
			else if (propertyInfo.PropertyType == propertyInfo.DeclaringType)
				return MemberOrder.LifetimeProperty;
			else
				return MemberOrder.StaticProperty;
		}

		private static MemberOrder GetEventOrder(EventInfo eventInfo)
		{
			var method = eventInfo.AddMethod ?? eventInfo.RemoveMethod;
			if (!method.IsStatic)
				return MemberOrder.InstanceEvent;
			else
				return MemberOrder.StaticEvent;
		}

		private static MemberOrder GetMethodOrder(MethodInfo methodInfo)
		{
			if (!methodInfo.IsStatic)
				return MemberOrder.InstanceMethod;
			else if (methodInfo.ReturnType == methodInfo.DeclaringType)
				return MemberOrder.LifetimeMethod;
			else
				return MemberOrder.StaticMethod;
		}

		private static MemberOrder GetFieldOrder(FieldInfo fieldInfo)
		{
			if (!fieldInfo.IsStatic)
				return MemberOrder.InstanceField;
			else if (fieldInfo.FieldType == fieldInfo.DeclaringType)
				return MemberOrder.LifetimeField;
			else
				return MemberOrder.StaticField;
		}

		private static IEnumerable<T> OrderMembers<T>(IEnumerable<T> items, Func<T, MemberInfo> getMemberInfo)
		{
			return items.OrderBy(x => (int) GetMemberOrder(getMemberInfo(x)))
				.ThenBy(x => GetShortName(getMemberInfo(x)).ToString(), StringComparer.OrdinalIgnoreCase)
				.ThenBy(x => GetGenericArguments(getMemberInfo(x)).Length)
				.ThenBy(x => GetParameters(getMemberInfo(x)).Length)
				.ThenBy(x => GetParameterShortNames(getMemberInfo(x)), StringComparer.OrdinalIgnoreCase);
		}

		private static MethodInfo TryGetDelegateInvoke(MemberInfo memberInfo)
		{
			var typeInfo = memberInfo as TypeInfo;
			return typeInfo != null && typeof(Delegate).GetTypeInfo().IsAssignableFrom(typeInfo) ? typeInfo.DeclaredMethods.FirstOrDefault(x => x.Name == "Invoke") : null;
		}

		private static Type[] GetGenericArguments(MemberInfo memberInfo)
		{
			var type = memberInfo as TypeInfo;
			if (type != null)
				return type.GenericTypeParameters;

			var method = memberInfo as MethodInfo;
			return method?.GetGenericArguments() ?? new Type[0];
		}

		private static ParameterInfo[] GetParameters(MemberInfo memberInfo)
		{
			var delegateInvoke = TryGetDelegateInvoke(memberInfo);
			if (delegateInvoke != null)
				return GetParameters(delegateInvoke);

			var method = memberInfo as MethodBase;
			return method?.GetParameters() ?? new ParameterInfo[0];
		}

		private static string GetParameterShortNames(MemberInfo memberInfo)
		{
			return string.Join(", ", GetParameters(memberInfo).Select(x => RenderTypeName(x.ParameterType.GetTypeInfo())));
		}

		private static bool IsKeyword(string value)
		{
			return s_keywords.Contains(value);
		}

		private static string ToMarkdown(XmlDocInline inline, MarkdownContext context)
		{
			string text = inline.Text ?? "";

			MemberInfo seeMemberInfo = null;
			if (inline.SeeRef != null)
				context.MembersByXmlDocName.TryGetValue(inline.SeeRef, out seeMemberInfo);

			if (text.Length == 0)
			{
				if (seeMemberInfo != null)
					text = GetShortName(seeMemberInfo);
				else if (inline.SeeRef != null)
					text = XmlDocUtility.GetShortNameForXmlDocRef(inline.SeeRef);
			}

			if (text.Length != 0)
			{
				if (inline.IsCode || seeMemberInfo != null)
					text = $"`{text}`";

				if (inline.IsParamRef || inline.IsTypeParamRef)
					text = $"*{text}*";

				text = WrapMarkdownRefLink(text, seeMemberInfo, context);
			}

			return text;
		}

		private static string WrapMarkdownRefLink(string text, MemberInfo memberInfo, MarkdownContext context)
		{
			if (memberInfo != null &&
				XmlDocUtility.GetXmlDocRef(memberInfo) != XmlDocUtility.GetXmlDocRef(context.MemberInfo))
			{
				string path;

				var typeInfo = memberInfo as TypeInfo;
				if (context.MemberInfo != null)
				{
					if (typeInfo != null)
					{
						if (typeInfo.Namespace == context.TypeInfo.Namespace)
							path = $"../{GetTypeUriName(typeInfo)}.md";
						else
							path = $"../../{GetNamespaceUriName(typeInfo.Namespace)}/{GetTypeUriName(typeInfo)}.md";
					}
					else
					{
						if (memberInfo.DeclaringType == context.TypeInfo.AsType())
							path = $"{GetMemberUriName(memberInfo)}.md";
						else if (memberInfo.DeclaringType?.Namespace == context.TypeInfo.Namespace)
							path = $"../{GetTypeUriName(memberInfo.DeclaringType.GetTypeInfo())}/{GetMemberUriName(memberInfo)}.md";
						else
							path = $"../../{GetNamespaceUriName(memberInfo.DeclaringType?.Namespace)}/{GetTypeUriName(memberInfo.DeclaringType.GetTypeInfo())}/{GetMemberUriName(memberInfo)}.md";
					}
				}
				else if (context.TypeInfo != null)
				{
					if (typeInfo != null)
					{
						if (typeInfo.Namespace == context.TypeInfo.Namespace)
							path = $"{GetTypeUriName(typeInfo)}.md";
						else
							path = $"../{GetNamespaceUriName(typeInfo.Namespace)}/{GetTypeUriName(typeInfo)}.md";
					}
					else
					{
						if (memberInfo.DeclaringType?.Namespace == context.TypeInfo.Namespace)
							path = $"{GetTypeUriName(memberInfo.DeclaringType.GetTypeInfo())}/{GetMemberUriName(memberInfo)}.md";
						else
							path = $"../{GetNamespaceUriName(memberInfo.DeclaringType?.Namespace)}/{GetTypeUriName(memberInfo.DeclaringType.GetTypeInfo())}/{GetMemberUriName(memberInfo)}.md";
					}
				}
				else
				{
					if (typeInfo != null)
						path = $"{GetNamespaceUriName(typeInfo.Namespace)}/{GetTypeUriName(typeInfo)}.md";
					else
						path = $"{GetNamespaceUriName(memberInfo.DeclaringType?.Namespace)}/{GetTypeUriName(memberInfo.DeclaringType.GetTypeInfo())}/{GetMemberUriName(memberInfo)}.md";
				}

				text = $"[{text}]({path})";
			}

			return EscapeHtml(text);
		}

		private static string ToMarkdown(IEnumerable<XmlDocInline> inlines, MarkdownContext context)
		{
			return inlines == null ? null : string.Concat(inlines.Select(x => ToMarkdown(x, context)));
		}

		private static IEnumerable<string> ToMarkdown(IReadOnlyList<XmlDocBlock> blocks, MarkdownContext context)
		{
			for (int index = 0; index < blocks.Count; index++)
			{
				if (index != 0)
					yield return "";

				var block = blocks[index];

				if (block.ListKind == XmlDocListKind.Bullet || block.ListKind == XmlDocListKind.Number)
				{
					int number = 0;
					while (true)
					{
						string prefix = new string(' ', block.ListDepth * 2) + (block.ListKind == XmlDocListKind.Number ? $"{++number}. " : "* ");
						string markdown = ToMarkdown(block.Inlines, context);

						if (block.IsListTerm)
						{
							markdown = $"**{markdown}**";

							var afterTermBlock = index + 1 < blocks.Count ? blocks[index + 1] : null;
							if (afterTermBlock != null && afterTermBlock.ListKind == block.ListKind && afterTermBlock.ListDepth == block.ListDepth && !afterTermBlock.IsListTerm)
							{
								markdown += " – " + ToMarkdown(afterTermBlock.Inlines, context);
								index++;
							}
						}

						yield return prefix + markdown;

						var nextBlock = index + 1 < blocks.Count ? blocks[index + 1] : null;
						if (nextBlock == null || nextBlock.ListKind != block.ListKind)
							break;

						block = nextBlock;
						index++;
					}
				}
				else
				{
					if (block.IsCode)
					{
						yield return "```csharp";
						foreach (var inline in block.Inlines)
							yield return inline.Text;
						yield return "```";
					}
					else
					{
						string markdown = ToMarkdown(block.Inlines, context);
						if (block.IsListHeader || block.IsListTerm)
							markdown = $"**{markdown}**";
						yield return markdown;
					}
				}
			}
		}

		private static string GetCodeGenComment(string assemblyName) => $"<!-- DO NOT EDIT: generated by xmldocmd for {assemblyName} -->";

		private class MarkdownContext
		{
			public MarkdownContext(XmlDocAssembly xmlDocAssembly, IReadOnlyDictionary<string, MemberInfo> membersByXmlDocName, string assemblyFileName)
			{
				XmlDocAssembly = xmlDocAssembly;
				MembersByXmlDocName = membersByXmlDocName;
				AssemblyFileName = assemblyFileName;
			}

			public MarkdownContext(MarkdownContext context, MemberInfo memberInfo)
			{
				XmlDocAssembly = context.XmlDocAssembly;
				MembersByXmlDocName = context.MembersByXmlDocName;
				AssemblyFileName = context.AssemblyFileName;

				var typeInfo = memberInfo as TypeInfo;
				if (typeInfo != null)
				{
					TypeInfo = typeInfo;
				}
				else
				{
					TypeInfo = memberInfo.DeclaringType.GetTypeInfo();
					MemberInfo = memberInfo;
				}
			}

			public TypeInfo TypeInfo { get; }

			public MemberInfo MemberInfo { get; }

			public XmlDocAssembly XmlDocAssembly { get; }

			public IReadOnlyDictionary<string, MemberInfo> MembersByXmlDocName { get; }

			public string AssemblyFileName { get; }
		}

		static readonly HashSet<string> s_keywords = new HashSet<string>
		{
			"abstract",
			"as",
			"base",
			"bool",
			"break",
			"byte",
			"case",
			"catch",
			"char",
			"checked",
			"class",
			"const",
			"continue",
			"decimal",
			"default",
			"delegate",
			"do",
			"double",
			"else",
			"enum",
			"event",
			"explicit",
			"extern",
			"false",
			"finally",
			"fixed",
			"float",
			"for",
			"foreach",
			"goto",
			"if",
			"implicit",
			"in",
			"int",
			"interface",
			"internal",
			"is",
			"lock",
			"long",
			"namespace",
			"new",
			"null",
			"object",
			"operator",
			"out",
			"override",
			"params",
			"private",
			"protected",
			"public",
			"readonly",
			"ref",
			"return",
			"sbyte",
			"sealed",
			"short",
			"sizeof",
			"stackalloc",
			"static",
			"string",
			"struct",
			"switch",
			"this",
			"throw",
			"true",
			"try",
			"typeof",
			"uint",
			"ulong",
			"unchecked",
			"unsafe",
			"ushort",
			"using",
			"virtual",
			"void",
			"volatile",
			"while",
		};
	}
}
