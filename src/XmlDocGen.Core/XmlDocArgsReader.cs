namespace XmlDocGen.Core;

/// <summary>Reads command-line arguments.</summary>
public sealed class XmlDocArgsReader
{
	/// <summary>Initializes a new instance of the <see cref="XmlDocArgsReader"/> class.</summary>
	public XmlDocArgsReader(IEnumerable<string> args)
	{
		m_args = [.. args];
	}

	/// <summary>Reads a flag by long or short name.</summary>
	public bool ReadFlag(string name)
	{
		var names = name.Split('|');
		if (names.Length > 1)
			return names.Any(ReadFlag);

		var index = FindOptionArgumentIndex(name);
		if (index == -1)
			return false;
		m_args.RemoveAt(index);
		return true;
	}

	/// <summary>Reads an option value by long or short name.</summary>
	public string? ReadOption(string name)
	{
		var names = name.Split('|');
		if (names.Length > 1)
			return names.Select(ReadOption).FirstOrDefault(x => x is not null);

		var index = FindOptionArgumentIndex(name);
		if (index == -1)
			return null;
		var value = index + 1 < m_args.Count ? m_args[index + 1] : null;
		if (value is null || IsOption(value))
			throw new XmlDocArgsReaderException($"Missing value after '{RenderOption(name)}'.");
		m_args.RemoveAt(index);
		m_args.RemoveAt(index);
		return value;
	}

	/// <summary>Reads one positional argument.</summary>
	public string? ReadArgument()
	{
		if (m_args.Count == 0)
			return null;
		var value = m_args[0];
		if (IsOption(value))
			throw new XmlDocArgsReaderException($"Unexpected option '{value}'.");
		m_args.RemoveAt(0);
		return value;
	}

	/// <summary>Reads remaining positional arguments.</summary>
	public IReadOnlyList<string> ReadRemainingArguments()
	{
		var arguments = new List<string>();
		while (m_args.Count != 0 && !IsOption(m_args[0]))
			arguments.Add(ReadArgument()!);
		return arguments;
	}

	/// <summary>Verifies that no unread arguments remain.</summary>
	public void VerifyComplete()
	{
		if (m_args.Count != 0)
			throw new XmlDocArgsReaderException($"Unexpected {(IsOption(m_args[0]) ? "option" : "argument")} '{m_args[0]}'.");
	}

	private static bool IsOption(string value) => value.Length >= 2 && value[0] == '-' && value != "--";

	private static string RenderOption(string name) => name.Length == 1 ? $"-{name}" : $"--{name}";

	private static bool IsOptionArgument(string optionName, string argument) => argument == RenderOption(optionName);

	private int FindOptionArgumentIndex(string optionName)
	{
		for (var index = 0; index < m_args.Count; index++)
		{
			if (IsOptionArgument(optionName, m_args[index]))
				return index;
		}
		return -1;
	}

	private readonly List<string> m_args;
}
