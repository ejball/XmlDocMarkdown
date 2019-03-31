using System;
using Faithlife.Build;

internal static class Build
{
	public static int Main(string[] args) => BuildRunner.Execute(args, build =>
	{
		build.AddDotNetTargets(
			new DotNetBuildSettings
			{
				DocsSettings = new DotNetDocsSettings
				{
					GitLogin = new GitLoginInfo("ejball", Environment.GetEnvironmentVariable("BUILD_BOT_PASSWORD") ?? ""),
					GitAuthor = new GitAuthorInfo("ejball", "ejball@gmail.com"),
					SourceCodeUrl = "https://github.com/ejball/RepoName/tree/master/src",
				},
			});

		build.Target("default")
			.DependsOn("build");
	});
}
