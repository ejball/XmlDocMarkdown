namespace XmlDocMarkdown.Core;

internal sealed class ArgsReader(IEnumerable<string> args)
{
	public bool ReadFlag(string name)
	{
		ArgumentNullException.ThrowIfNull(name);
		if (name.Length == 0)
			throw new ArgumentException("Flag name must not be empty.", nameof(name));

		var names = name.Split('|');
		if (names.Length > 1)
			return names.Any(ReadFlag);

		var index = FindOptionArgumentIndex(name);
		if (index == -1)
			return false;

		m_args.RemoveAt(index);
		return true;
	}

	public string? ReadOption(string name)
	{
		ArgumentNullException.ThrowIfNull(name);
		if (name.Length == 0)
			throw new ArgumentException("Option name must not be empty.", nameof(name));

		var names = name.Split('|');
		if (names.Length > 1)
			return names.Select(ReadOption).FirstOrDefault(x => x != null);

		var index = FindOptionArgumentIndex(name);
		if (index == -1)
			return null;

		var value = index + 1 < m_args.Count ? m_args[index + 1] : null;
		if (value == null || IsOption(value))
			throw new ArgsReaderException($"Missing value after '{RenderOption(name)}'.");

		m_args.RemoveAt(index);
		m_args.RemoveAt(index);
		return value;
	}

	public string? ReadArgument()
	{
		if (m_args.Count == 0)
			return null;

		var value = m_args[0];

		if (IsOption(value))
			throw new ArgsReaderException($"Unexpected option '{value}'.");

		m_args.RemoveAt(0);
		return value;
	}

	public void VerifyComplete()
	{
		if (m_args.Count != 0)
			throw new ArgsReaderException($"Unexpected {(IsOption(m_args[0]) ? "option" : "argument")} '{m_args[0]}'.");
	}

	private static bool IsOption(string value) => value.Length >= 2 && value[0] == '-' && value != "--";

	private static string RenderOption(string name) => name.Length == 1 ? $"-{name}" : $"--{name}";

	private static bool IsOptionArgument(string optionName, string argument) => string.Equals(argument, RenderOption(optionName), StringComparison.Ordinal);

	private int FindOptionArgumentIndex(string optionName)
	{
		for (var index = 0; index < m_args.Count; index++)
		{
			var arg = m_args[index];
			if (IsOptionArgument(optionName, arg))
				return index;
		}

		return -1;
	}

	private readonly List<string> m_args = (args ?? throw new ArgumentNullException(nameof(args))).ToList();
}
