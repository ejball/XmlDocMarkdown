return BuildRunner.Execute(args, build =>
{
	var gitLogin = new GitLoginInfo("faithlifebuildbot", Environment.GetEnvironmentVariable("BUILD_BOT_PASSWORD") ?? "");

	build.AddDotNetTargets(
		new DotNetBuildSettings
		{
			NuGetApiKey = Environment.GetEnvironmentVariable("NUGET_API_KEY"),
			DocsSettings = new DotNetDocsSettings
			{
				GitLogin = gitLogin,
				GitAuthor = new GitAuthorInfo("Faithlife Build Bot", "faithlifebuildbot@users.noreply.github.com"),
				SourceCodeUrl = "https://github.com/Faithlife/RepoName/tree/master/src",
			},
			PackageSettings = new DotNetPackageSettings
			{
				GitLogin = gitLogin,
				PushTagOnPublish = x => $"v{x.Version}",
			},
		});
});
