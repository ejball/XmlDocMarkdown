using System.Reflection;

namespace XmlDocMarkdown.Core;

internal class JekyllPathBuilder : IPathBuilder
{
	private string? @namespace;
	private string? typeName;
	private string? memberName;
	private bool hasPermalinkPretty;
	private bool useTypeFolders;

	public IPathBuilder WithNamespace(string @namespace)
	{
		this.@namespace = @namespace;
		return this;
	}

	public IPathBuilder WithType(TypeInfo typeInfo)
	{
		typeName = typeInfo.Name;
		return this;
	}

	public IPathBuilder WithMemberName(string name)
	{
		memberName = name;
		return this;
	}

	public IPathBuilder WithPermalinkPretty()
	{
		hasPermalinkPretty = true;
		return this;
	}

	public IPathBuilder WithTypeFolders()
	{
		useTypeFolders = true;
		return this;
	}

	public string Build()
	{
		var relative = GetPermalink(typeName!);
		var safeRelative = GetSafeName(relative);

		if (hasPermalinkPretty)
			safeRelative += "Type.md";
		var path = $"{GetNamespaceUriName(@namespace)}/{safeRelative}";
		if (memberName is not null)
			path += $"/{memberName}";
		else if (useTypeFolders)
			path += $"/{safeRelative}";
		return $"{path}.md";
	}

	private string GetPermalink(string path)
	{
		if (hasPermalinkPretty)
		{
			// permalinks paths cannot end in .md
			var pos = path.LastIndexOf('.');
			if (pos > 0)
				path = path.Substring(0, pos);
		}
		return path.Replace("\\", "/");
	}

	private string GetSafeName(string name)
	{
		if (hasPermalinkPretty)
			return name.Replace(".", ""); // Jekyll can't handle dots in file name.
		return name;
	}

	private static string GetNamespaceUriName(string? namespaceName)
		=> namespaceName ?? "global";
}
