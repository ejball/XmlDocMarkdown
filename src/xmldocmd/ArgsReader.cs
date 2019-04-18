using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace ArgsReading
{
	/// <summary>
	/// Helps process command-line arguments.
	/// </summary>
	/// <remarks>To use this class, construct an <c>ArgsReader</c> with the command-line arguments from <c>Main</c>,
	/// read the supported options one at a time with <see cref="ReadFlag" /> and <see cref="ReadOption"/>,
	/// read any normal arguments with <see cref="ReadArgument"/>, and finally call <see cref="VerifyComplete"/>,
	/// which throws an <see cref="ArgsReaderException"/> if any unsupported options or arguments haven't been read.</remarks>
	public sealed class ArgsReader
	{
		/// <summary>
		/// Creates a reader for the specified command-line arguments.
		/// </summary>
		/// <param name="args">The command-line arguments from <c>Main</c>.</param>
		/// <exception cref="ArgumentNullException"><c>args</c> is <c>null</c>.</exception>
		public ArgsReader(IEnumerable<string> args)
		{
			m_args = args?.ToList() ?? throw new ArgumentNullException(nameof(args));
		}

		/// <summary>
		/// True if short options (e.g. <c>-h</c>) should ignore case. (Default false.)
		/// </summary>
		public bool ShortOptionIgnoreCase { get; set; }

		/// <summary>
		/// True if long options (e.g. <c>--help</c>) should ignore case. (Default false.)
		/// </summary>
		public bool LongOptionIgnoreCase { get; set; }

		/// <summary>
		/// True if long options (e.g. <c>--dry-run</c>) should ignore "kebab case", i.e. allow <c>--dryrun</c>. (Default false.)
		/// </summary>
		public bool LongOptionIgnoreKebabCase { get; set; }

		/// <summary>
		/// Reads the specified flag, returning true if it is found.
		/// </summary>
		/// <param name="name">The name of the specified flag.</param>
		/// <returns>True if the specified flag was found on the command line.</returns>
		/// <remarks><para>If the flag is found, the method returns <c>true</c> and the flag is
		/// removed. If <c>ReadFlag</c> is called with the same name, it will return <c>false</c>,
		/// unless the same flag appears twice on the command line.</para>
		/// <para>To support multiple names for the same flag, use a <c>|</c> to separate them,
		/// e.g. use <c>help|h|?</c> to support three different names for a help flag.</para>
		/// <para>Single-character names use a single hyphen, e.g. <c>-h</c>, and are matched
		/// case-sensitively. Longer names use a double hyphen, e.g. <c>--help</c>, and are
		/// matched case-insensitively.</para></remarks>
		/// <exception cref="ArgumentNullException"><c>name</c> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException">One of the names is empty.</exception>
		public bool ReadFlag(string name)
		{
			if (name == null)
				throw new ArgumentNullException(nameof(name));
			if (name.Length == 0)
				throw new ArgumentException("Flag name must not be empty.", nameof(name));

			var names = name.Split('|');
			if (names.Length > 1)
				return names.Any(ReadFlag);

			int index = m_args.FindIndex(x => IsOptionArgument(name, x));
			if (index == -1)
				return false;

			m_args.RemoveAt(index);
			return true;
		}

		/// <summary>
		/// Reads the value of the specified option, if any.
		/// </summary>
		/// <param name="name">The name of the specified option.</param>
		/// <returns>The specified option if it was found on the command line; <c>null</c> otherwise.</returns>
		/// <remarks><para>If the option is found, the method returns the command-line argument
		/// after the option and both arguments are removed. If <c>ReadOption</c> is called with the
		/// same name, it will return <c>null</c>, unless the same option appears twice on the command line.</para>
		/// <para>To support multiple names for the same option, use a vertical bar (<c>|</c>) to separate them,
		/// e.g. use <c>n|name</c> to support two different names for a module option.</para>
		/// <para>Single-character names use a single hyphen, e.g. <c>-n example</c>, and are matched
		/// case-sensitively. Longer names use a double hyphen, e.g. <c>--name example</c>, and are
		/// matched case-insensitively.</para></remarks>
		/// <exception cref="ArgumentNullException"><c>name</c> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException">One of the names is empty.</exception>
		/// <exception cref="ArgsReaderException">The argument that must follow the option is missing.</exception>
		public string ReadOption(string name)
		{
			if (name == null)
				throw new ArgumentNullException(nameof(name));
			if (name.Length == 0)
				throw new ArgumentException("Option name must not be empty.", nameof(name));

			var names = name.Split('|');
			if (names.Length > 1)
				return names.Select(ReadOption).FirstOrDefault(x => x != null);

			int index = m_args.FindIndex(x => IsOptionArgument(name, x));
			if (index == -1)
				return null;

			string value = index + 1 < m_args.Count ? m_args[index + 1] : null;
			if (value == null || IsOption(value))
				throw new ArgsReaderException($"Missing value after '{RenderOption(name)}'.");

			m_args.RemoveAt(index);
			m_args.RemoveAt(index);
			return value;
		}

		/// <summary>
		/// Reads the next non-option argument.
		/// </summary>
		/// <returns>The next non-option argument, or null if none remain.</returns>
		/// <remarks><para>If the next argument is an option, this method throws an exception.
		/// If options can appear before normal arguments, be sure to read all options before reading
		/// any normal arguments.</para></remarks>
		/// <exception cref="ArgsReaderException">The next argument is an option.</exception>
		public string ReadArgument()
		{
			if (m_args.Count == 0)
				return null;

			string value = m_args[0];
			if (IsOption(value))
				throw new ArgsReaderException($"Unexpected option '{value}'.");

			m_args.RemoveAt(0);
			return value;
		}

		/// <summary>
		/// Reads any remaining non-option arguments.
		/// </summary>
		/// <returns>The remaining non-option arguments, if any.</returns>
		/// <remarks><para>If any remaining arguments are options, this method throws an exception.
		/// If options can appear before normal arguments, be sure to read all options before reading
		/// any normal arguments.</para></remarks>
		/// <exception cref="ArgsReaderException">A remaining argument is an option.</exception>
		public IReadOnlyList<string> ReadArguments()
		{
			var arguments = new List<string>();
			while (true)
			{
				string argument = ReadArgument();
				if (argument == null)
					return arguments;
				arguments.Add(argument);
			}
		}

		/// <summary>
		/// Confirms that all arguments were processed.
		/// </summary>
		/// <exception cref="ArgsReaderException">A command-line argument was not read.</exception>
		public void VerifyComplete()
		{
			if (m_args.Count != 0)
				throw new ArgsReaderException($"Unexpected {(IsOption(m_args[0]) ? "option" : "argument")} '{m_args[0]}'.");
		}

		private static bool IsOption(string value) => value.Length >= 2 && value[0] == '-' && value != "--";

		private static string RenderOption(string name) => name.Length == 1 ? $"-{name}" : $"--{name}";

		private bool IsOptionArgument(string optionName, string argument)
		{
			string renderedOption = RenderOption(optionName);
			if (optionName.Length == 1)
			{
				return string.Equals(argument, renderedOption, ShortOptionIgnoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
			}
			else
			{
				if (LongOptionIgnoreKebabCase)
				{
					argument = Regex.Replace(argument, @"\b-\b", "");
					renderedOption = Regex.Replace(renderedOption, @"\b-\b", "");
				}

				return string.Equals(argument, renderedOption, LongOptionIgnoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
			}
		}

		readonly List<string> m_args;
	}
}
