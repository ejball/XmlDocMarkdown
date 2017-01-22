using System.Globalization;
using ArgsReading;

namespace XmlDocMarkdown
{
	internal static class CommonArgs
	{
		public static bool ReadDryRunFlag(this ArgsReader args)
		{
			return args.ReadFlag("dryrun");
		}

		public static bool ReadHelpFlag(this ArgsReader args)
		{
			return args.ReadFlag("help|h|?");
		}

		public static bool ReadQuietFlag(this ArgsReader args)
		{
			return args.ReadFlag("quiet");
		}

		public static bool ReadVerifyFlag(this ArgsReader args)
		{
			return args.ReadFlag("verify");
		}

		public static string ReadIndentOption(this ArgsReader args)
		{
			string value = args.ReadOption("indent");
			if (value == null)
				return null;

			if (value == "tab")
				return "\t";

			int spaceCount;
			if (int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out spaceCount) && spaceCount >= 1 && spaceCount <= 8)
				return new string(' ', spaceCount);

			throw new ArgsReaderException($"Invalid indent '{value}'. (Should be 'tab' or the number of spaces.)");
		}

		public static string ReadNewLineOption(this ArgsReader args)
		{
			string value = args.ReadOption("newline");
			if (value == null)
				return null;

			switch (value)
			{
			case "auto":
				return null;
			case "lf":
				return "\n";
			case "crlf":
				return "\r\n";
			default:
				throw new ArgsReaderException($"Invalid new line '{value}'. (Should be 'auto', 'lf', or 'crlf'.)");
			}
		}
	}
}
