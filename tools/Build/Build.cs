using System;
using Faithlife.Build;

BuildRunner.Execute(args, build => build.AddDotNetTargets(
	new DotNetBuildSettings
	{
		NuGetApiKey = Environment.GetEnvironmentVariable("NUGET_API_KEY"),
		DocsSettings = new DotNetDocsSettings
		{
			GitLogin = new GitLoginInfo("faithlifebuildbot", Environment.GetEnvironmentVariable("BUILD_BOT_PASSWORD") ?? ""),
			GitAuthor = new GitAuthorInfo("Faithlife Build Bot", "faithlifebuildbot@users.noreply.github.com"),
			SourceCodeUrl = "https://github.com/Faithlife/RepoName/tree/master/src",
		},
	}));
