using XmlDocMarkdown.Core;

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

		public static XmlDocVisibilityLevel? ReadVisibilityOption(this ArgsReader args)
		{
			string visibility = args.ReadOption("visibility");
			switch (visibility)
			{
			case "public":
				return XmlDocVisibilityLevel.Public;
			case "protected":
				return XmlDocVisibilityLevel.Protected;
			case "internal":
				return XmlDocVisibilityLevel.Internal;
			case "private":
				return XmlDocVisibilityLevel.Private;
			case null:
				return null;
			default:
				throw new ArgsReaderException($"Unknown visibility option: {visibility}");
			}
		}

		public static bool ReadObsoleteFlag(this ArgsReader args)
		{
			return args.ReadFlag("obsolete");
		}

		public static bool ReadSkipUnbrowsableFlag(this ArgsReader args)
		{
			return args.ReadFlag("skip-unbrowsable");
		}

		public static string ReadExternalOption(this ArgsReader args)
		{
			return args.ReadOption("external");
		}

		public static bool ReadCleanFlag(this ArgsReader args)
		{
			return args.ReadFlag("clean");
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

		public static bool ReadNamespacePageFlag(this ArgsReader args)
		{
			return args.ReadFlag("namespace-pages");
		}

		public static string ReadFrontMatter(this ArgsReader args)
		{
			return args.ReadOption("front-matter");
		}

		public static string ReadPermalinkStyle(this ArgsReader args)
		{
			return args.ReadOption("permalink");
		}

		public static bool ReadVerifyFlag(this ArgsReader args)
		{
			return args.ReadFlag("verify");
		}

		public static bool ReadGeneratyeTocFlag(this ArgsReader args)
		{
			return args.ReadFlag("toc");
		}

		public static string ReadTocPrefix(this ArgsReader args)
		{
			return args.ReadOption("toc-prefix");
		}

		public static string ReadBlackList(this ArgsReader args)
		{
			return args.ReadOption("blacklist");
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
