using ExampleAssembly;
using NUnit.Framework;
using XmlDocGen.Core;

namespace XmlDocGen.Tests.App;

internal sealed class XmlDocGenAppTests
{
	[Test]
	public void AppCanGenerateExampleAssemblyInDryRunMode()
	{
		var exitCode = XmlDocGenApp.Run(
			[
				typeof(ExampleClass).Assembly.GetName().Name!,
				Path.Combine(Path.GetTempPath(), "XmlDocGenAppTests"),
				"--dryrun",
				"--quiet",
			]);

		Assert.That(exitCode, Is.Zero);
	}

	[Test]
	public void HelpIncludesCustomHostLines()
	{
		using var output = new StringWriter();
		var oldOutput = Console.Out;
		try
		{
			Console.SetOut(output);

			var exitCode = XmlDocGenApp.Run(["--help"], ctx => ctx.HelpLines.Add("  --sample   Custom sample option."));

			Assert.That(exitCode, Is.Zero);
			Assert.That(output.ToString(), Does.Contain("--sample"));
		}
		finally
		{
			Console.SetOut(oldOutput);
		}
	}

	[Test]
	public void UnknownOptionsReturnCommandLineError()
	{
		using var error = new StringWriter();
		var oldError = Console.Error;
		try
		{
			Console.SetError(error);

			Assert.That(XmlDocGenApp.Run([typeof(ExampleClass).Assembly.GetName().Name!, "docs", "--unknown"]), Is.EqualTo(2));
			Assert.That(error.ToString(), Does.Contain("--unknown"));
		}
		finally
		{
			Console.SetError(oldError);
		}
	}
}
