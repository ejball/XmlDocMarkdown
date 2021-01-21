namespace XmlDocMarkdown.Core
{
	internal static class CommonArgs
	{
		public static string ReadSourceOption(this ArgsReader args) => args.ReadOption("source");

		public static string ReadNamespaceOption(this ArgsReader args) => args.ReadOption("namespace");

		public static XmlDocVisibilityLevel? ReadVisibilityOption(this ArgsReader args)
		{
			var visibility = args.ReadOption("visibility");
			return visibility switch
			{
				"public" => XmlDocVisibilityLevel.Public,
				"protected" => XmlDocVisibilityLevel.Protected,
				"internal" => XmlDocVisibilityLevel.Internal,
				"private" => XmlDocVisibilityLevel.Private,
				null => null,
				_ => throw new ArgsReaderException($"Unknown visibility option: {visibility}"),
			};
		}

		public static bool ReadObsoleteFlag(this ArgsReader args) => args.ReadFlag("obsolete");

		public static bool ReadSkipUnbrowsableFlag(this ArgsReader args) => args.ReadFlag("skip-unbrowsable");

		public static string ReadExternalOption(this ArgsReader args) => args.ReadOption("external");

		public static bool ReadCleanFlag(this ArgsReader args) => args.ReadFlag("clean");

		public static bool ReadDryRunFlag(this ArgsReader args) => args.ReadFlag("dryrun");

		public static bool ReadHelpFlag(this ArgsReader args) => args.ReadFlag("help|h|?");

		public static bool ReadQuietFlag(this ArgsReader args) => args.ReadFlag("quiet");

		public static bool ReadNamespacePagesFlag(this ArgsReader args) => args.ReadFlag("namespace-pages");

		public static string ReadFrontMatter(this ArgsReader args) => args.ReadOption("front-matter");

		public static string ReadPermalinkStyle(this ArgsReader args) => args.ReadOption("permalink");

		public static bool ReadVerifyFlag(this ArgsReader args) => args.ReadFlag("verify");

		public static bool ReadTocFlag(this ArgsReader args) => args.ReadFlag("toc");

		public static string ReadTocPrefix(this ArgsReader args) => args.ReadOption("toc-prefix");

		public static string ReadNewLineOption(this ArgsReader args)
		{
			var value = args.ReadOption("newline");
			return value switch
			{
				"auto" => null,
				"lf" => "\n",
				"crlf" => "\r\n",
				null => null,
				_ => throw new ArgsReaderException($"Invalid new line '{value}'. (Should be 'auto', 'lf', or 'crlf'.)"),
			};
		}
	}
}
