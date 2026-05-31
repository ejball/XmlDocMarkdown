using System.Reflection;

namespace XmlDocGen.Core.CSharp;

/// <summary>A token in a C# signature.</summary>
public sealed record CSharpToken(CSharpTokenKind Kind, string Text, MemberInfo? LinkTarget = null);
