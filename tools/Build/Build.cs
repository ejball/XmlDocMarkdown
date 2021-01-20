using System;
using Faithlife.Build;

return BuildRunner.Execute(args, build => build.AddDotNetTargets(
	new DotNetBuildSettings
	{
		NuGetApiKey = Environment.GetEnvironmentVariable("NUGET_API_KEY"),
		DocsSettings = new DotNetDocsSettings
		{
			GitLogin = new GitLoginInfo("ejball", Environment.GetEnvironmentVariable("BUILD_BOT_PASSWORD") ?? ""),
			GitAuthor = new GitAuthorInfo("ejball", "ejball@gmail.com"),
			SourceCodeUrl = "https://github.com/ejball/RepoName/tree/master/src",
		},
	}));
