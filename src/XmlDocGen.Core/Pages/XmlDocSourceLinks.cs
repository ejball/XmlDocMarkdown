using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Text.Json;
using XmlDocGen.Core.Nodes;
using XmlDocGen.Core.Xml;

namespace XmlDocGen.Core.Pages;

/// <summary>Provides source-link URLs for reflected members.</summary>
public sealed class XmlDocSourceLinks
{
	private XmlDocSourceLinks(Assembly assembly, IReadOnlyDictionary<int, string> urlsByMetadataToken)
	{
		Assembly = assembly;
		m_urlsByMetadataToken = urlsByMetadataToken;
	}

	/// <summary>Gets the assembly this source-link resolver was created for.</summary>
	public Assembly Assembly { get; }

	/// <summary>Attempts to create source links for an assembly.</summary>
	public static XmlDocSourceLinks? TryCreate(Assembly assembly)
	{
		ArgumentNullException.ThrowIfNull(assembly);
		if (string.IsNullOrEmpty(assembly.Location))
			return null;

		var pdbPath = Path.ChangeExtension(assembly.Location, ".pdb");
		if (!File.Exists(pdbPath))
			return TryCreateFromEmbeddedPdb(assembly);

		using var stream = File.OpenRead(pdbPath);
		using var provider = MetadataReaderProvider.FromPortablePdbStream(stream);
		return TryCreateFromReader(assembly, provider.GetMetadataReader());
	}

	/// <summary>Attempts to get a source URL for a member.</summary>
	public string? TryGetUrl(MemberInfo member)
	{
		if (member is TypeInfo type)
			member = type.DeclaredConstructors.FirstOrDefault(x => !x.IsStatic) ?? type.DeclaredMethods.FirstOrDefault() ?? member;
		if (member is PropertyInfo property)
			member = property.GetMethod ?? property.SetMethod ?? member;
		if (member is EventInfo @event)
			member = @event.AddMethod ?? @event.RemoveMethod ?? member;
		return m_urlsByMetadataToken.GetValueOrDefault(member.MetadataToken);
	}

	private static XmlDocSourceLinks? TryCreateFromEmbeddedPdb(Assembly assembly)
	{
		using var stream = File.OpenRead(assembly.Location);
		using var peReader = new PEReader(stream);
		foreach (var entry in peReader.ReadDebugDirectory())
		{
			if (entry.Type != DebugDirectoryEntryType.EmbeddedPortablePdb)
				continue;

			using var provider = peReader.ReadEmbeddedPortablePdbDebugDirectoryData(entry);
			return TryCreateFromReader(assembly, provider.GetMetadataReader());
		}
		return null;
	}

	private static XmlDocSourceLinks? TryCreateFromReader(Assembly assembly, MetadataReader reader)
	{
		var documents = ReadSourceLinkDocuments(reader);
		if (documents.Count == 0)
			return null;

		var urlsByMetadataToken = new Dictionary<int, string>();
		foreach (var handle in reader.MethodDebugInformation)
		{
			var methodDebugInfo = reader.GetMethodDebugInformation(handle);
			if (methodDebugInfo.Document.IsNil)
				continue;

			var firstSequencePoint = methodDebugInfo.GetSequencePoints().FirstOrDefault(x => !x.IsHidden);
			if (firstSequencePoint.Equals(default(SequencePoint)))
				continue;

			var documentName = reader.GetString(reader.GetDocument(methodDebugInfo.Document).Name);
			if (TryGetSourceUrl(documents, documentName, firstSequencePoint.StartLine) is { } sourceUrl)
			{
				var rowNumber = MetadataTokens.GetRowNumber(handle);
				urlsByMetadataToken[MetadataTokens.GetToken(MetadataTokens.MethodDefinitionHandle(rowNumber))] = sourceUrl;
			}
		}

		return urlsByMetadataToken.Count == 0 ? null : new XmlDocSourceLinks(assembly, urlsByMetadataToken);
	}

	private static IReadOnlyDictionary<string, string> ReadSourceLinkDocuments(MetadataReader reader)
	{
		foreach (var handle in reader.CustomDebugInformation)
		{
			var customDebugInformation = reader.GetCustomDebugInformation(handle);
			if (reader.GetGuid(customDebugInformation.Kind) != s_sourceLinkId)
				continue;

			var bytes = reader.GetBlobBytes(customDebugInformation.Value);
			using var document = JsonDocument.Parse(Encoding.UTF8.GetString(bytes));
			if (!document.RootElement.TryGetProperty("documents", out var documentsElement))
				return new Dictionary<string, string>();

			return documentsElement.EnumerateObject().ToDictionary(x => NormalizePath(x.Name), x => x.Value.GetString() ?? "", StringComparer.OrdinalIgnoreCase);
		}
		return new Dictionary<string, string>();
	}

	private static string? TryGetSourceUrl(IReadOnlyDictionary<string, string> documents, string documentName, int line)
	{
		var normalizedDocumentName = NormalizePath(documentName);
		foreach (var (pattern, urlPattern) in documents)
		{
			var starIndex = pattern.IndexOf('*', StringComparison.Ordinal);
			if (starIndex == -1)
			{
				if (string.Equals(pattern, normalizedDocumentName, StringComparison.OrdinalIgnoreCase))
					return urlPattern + "#L" + line;
				continue;
			}

			var prefix = pattern[..starIndex];
			var suffix = pattern[(starIndex + 1)..];
			if (normalizedDocumentName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) && normalizedDocumentName.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
			{
				var wildcard = normalizedDocumentName[prefix.Length..^suffix.Length];
				return urlPattern.Replace("*", wildcard, StringComparison.Ordinal) + "#L" + line;
			}
		}
		return null;
	}

	private static string NormalizePath(string path) => path.Replace('\\', '/');

	private static readonly Guid s_sourceLinkId = new("CC110556-A091-4D38-9FEC-25AB9A351A6A");
	private readonly IReadOnlyDictionary<int, string> m_urlsByMetadataToken;
}
