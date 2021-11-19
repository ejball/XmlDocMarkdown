using Faithlife.Build;

return BuildRunner.Execute(args, build =>
{
	var gitLogin = new GitLoginInfo("ejball", Environment.GetEnvironmentVariable("BUILD_BOT_PASSWORD") ?? "");

	build.AddDotNetTargets(
		new DotNetBuildSettings
		{
			NuGetApiKey = Environment.GetEnvironmentVariable("NUGET_API_KEY"),
			DocsSettings = new DotNetDocsSettings
			{
				GitLogin = gitLogin,
				GitAuthor = new GitAuthorInfo("ejball", "ejball@gmail.com"),
				SourceCodeUrl = "https://github.com/ejball/RepoName/tree/master/src",
			},
			PackageSettings = new DotNetPackageSettings
			{
				GitLogin = gitLogin,
				PushTagOnPublish = x => $"v{x.Version}",
			},
		});
});
