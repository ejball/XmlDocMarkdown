using ArgsReading;

namespace XmlDocMarkdown
{
	internal static class CommonArgs
	{
		public static string ReadSourceOption(this ArgsReader args)
		{
			return args.ReadOption("source");
		}

		public static string ReadNamespaceOption(this ArgsReader args)
		{
			return args.ReadOption("namespace");
		}

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
