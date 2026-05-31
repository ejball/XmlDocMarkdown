using System.Reflection;
using XmlDocGen.Core.Xml;

namespace XmlDocGen.Core.Pages;

/// <summary>Resolves links to documentation outside the current tree.</summary>
public abstract class XmlDocExternalLinkResolver
{
	/// <summary>Gets a resolver for Microsoft Learn .NET API documentation.</summary>
	public static XmlDocExternalLinkResolver DotNetApi { get; } = new DotNetApiResolver();

	/// <summary>Creates a resolver from a URL format where <c>{ref}</c> is the XML documentation reference.</summary>
	public static XmlDocExternalLinkResolver UrlPattern(string urlFormat) => new PatternResolver(urlFormat);

	/// <summary>Combines resolvers, using the first non-null URL.</summary>
	public static XmlDocExternalLinkResolver Combine(params XmlDocExternalLinkResolver[] resolvers) => new CombinedResolver(resolvers);

	/// <summary>Tries to resolve a URL for an external reference.</summary>
	public abstract string? TryGetUrl(XmlDocRef reference, MemberInfo? member);

	private sealed class DotNetApiResolver : XmlDocExternalLinkResolver
	{
		public override string? TryGetUrl(XmlDocRef reference, MemberInfo? member)
		{
			var value = reference.Value;
			if (value.StartsWith("N:System", StringComparison.Ordinal))
				return "https://learn.microsoft.com/dotnet/api/" + value[2..].ToLowerInvariant();
			if (value.StartsWith("T:System", StringComparison.Ordinal))
				return "https://learn.microsoft.com/dotnet/api/" + value[2..].Replace('`', '-').ToLowerInvariant();
			return member?.DeclaringType?.Namespace?.StartsWith("System", StringComparison.Ordinal) == true ? "https://learn.microsoft.com/dotnet/api/" + member.DeclaringType.FullName?.ToLowerInvariant() : null;
		}
	}

	private sealed class PatternResolver(string urlFormat) : XmlDocExternalLinkResolver
	{
		public override string TryGetUrl(XmlDocRef reference, MemberInfo? member) => urlFormat.Replace("{ref}", Uri.EscapeDataString(reference.Value), StringComparison.Ordinal).Replace("{name}", Uri.EscapeDataString(reference.Value[2..]), StringComparison.Ordinal);
	}

	private sealed class CombinedResolver(IReadOnlyList<XmlDocExternalLinkResolver> resolvers) : XmlDocExternalLinkResolver
	{
		public override string? TryGetUrl(XmlDocRef reference, MemberInfo? member)
		{
			foreach (var resolver in resolvers)
			{
				if (resolver.TryGetUrl(reference, member) is { } url)
					return url;
			}
			return null;
		}
	}
}
