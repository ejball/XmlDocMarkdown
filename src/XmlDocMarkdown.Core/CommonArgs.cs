namespace XmlDocMarkdown.Core
{
	internal static class CommonArgs
	{
		public static bool ReadCleanFlag(this ArgsReader args) => args.ReadFlag("clean");

		public static bool ReadDryRunFlag(this ArgsReader args) => args.ReadFlag("dryrun");

		public static bool ReadHelpFlag(this ArgsReader args) => args.ReadFlag("help|h|?");

		public static bool ReadQuietFlag(this ArgsReader args) => args.ReadFlag("quiet");

		public static bool ReadVerifyFlag(this ArgsReader args) => args.ReadFlag("verify");
	}
}
