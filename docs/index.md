# XmlDocMarkdown

**XmlDocMarkdown** generates Markdown from [.NET XML documentation comments](https://msdn.microsoft.com/en-us/library/b2s063f7.aspx).

It is distributed as a .NET Core Global Tool, console app, Cake addin, and class library.

For example output, see the [Markdown documents](https://github.com/ejball/XmlDocMarkdown/blob/master/docs/ExampleAssembly.md) for the [documentation](ExampleAssembly.md) of the [ExampleAssembly](https://github.com/ejball/XmlDocMarkdown/tree/master/tools/ExampleAssembly) class library.

The goal of this tool is to generate Markdown documentation for .NET class libraries that are simple enough to be read and understood in [raw form](https://raw.githubusercontent.com/ejball/XmlDocMarkdown/master/docs/ExampleAssembly/ExampleClass.md), as [rendered in GitHub](https://github.com/ejball/XmlDocMarkdown/blob/master/docs/ExampleAssembly/ExampleClass.md), or when used to generate [web pages](https://ejball.com/XmlDocMarkdown/ExampleAssembly/ExampleClass.html) using [Jekyll](https://jekyllrb.com/) and [GitHub Pages](https://pages.github.com/). To that end, it generates standard [GitHub Flavored Markdown](https://github.github.com/gfm/) without relying on raw HTML tags.

For a more full-featured documentation generation tool, check out [DocFX](https://dotnet.github.io/docfx/) or [Sandcastle](https://github.com/EWSoftware/SHFB).

## xmldocmd (.NET Core Global Tool)

[![NuGet](https://img.shields.io/nuget/v/xmldocmd.svg)](https://www.nuget.org/packages/xmldocmd)

To install `xmldocmd`: `dotnet tool install xmldocmd -g`

### Usage

The `xmldocmd` command-line tool accepts the path to the input assembly, the path to the output directory, and a number of options.

The XML documentation file should be in the same directory as the input assembly.

The output directory will be created if necessary.

For example, `xmldocmd MyLibrary.dll docs` generates Markdown documentation in the `docs` directory for the `MyLibrary.dll` assembly. The compiler-generated `MyLibrary.xml` file should be in the same directory as `MyLibrary.dll`.

### Options

* `--source <url>`: The URL (absolute or relative) of the folder containing the source code of the assembly, e.g. at GitHub. Required to generate source code links in the See Also sections for types.
* `--namespace <ns>`: The root namespace of the input assembly. Used to generate source code links in the See Also sections for types. If omitted, the tool guesses the root namespace from the exported types.
* `--visibility (public|protected|internal|private)`: The minimum visibility for documented types and members. If `public`, only public types and members are documented. If `protected`, only public and protected types and members are documented. Similarly for `internal` and `private`. Defaults to `protected`.
* `--obsolete`: Generates documentation for obsolete types and members, which are not documented by default.
* `--external <ns>`: Generates links to external documentation for the specified namespace.
* `--clean`: Delete previously generated files that are no longer used.
* `--verify`: Executes the tool without making changes to the file system, but exits with error code 1 if changes would be made. Typically used in build scripts to ensure that any changes have been reflected in the generated code.
* `--dryrun`: Executes the tool without making changes to the file system.
* `--quiet`: Suppresses normal console output.
* `--newline (auto|lf|crlf)`: Indicates the newline used in the output. Defaults to `auto`, which uses CRLF or LF, depending on the platform.

## XmlDocMarkdown (console app)

[![NuGet](https://img.shields.io/nuget/v/XmlDocMarkdown.svg)](https://www.nuget.org/packages/XmlDocMarkdown)

`nuget install XmlDocMarkdown -excludeversion` will download the latest version of `XmlDocMarkdown.exe` into `XmlDocMarkdown/tools`.

On Mac or Linux, use [Mono](http://www.mono-project.com/) to run `nuget` as well as the command-line tool itself.

The command-line arguments and options are the same as `xmldocmd` above.

## Cake.XmlDocMarkdown (Cake addin)

[![NuGet](https://img.shields.io/nuget/v/Cake.XmlDocMarkdown.svg)](https://www.nuget.org/packages/Cake.XmlDocMarkdown)

To use the addin, include it at the top of your [Cake](https://cakebuild.net/) 0.26.1+ script:

```
#addin Cake.XmlDocMarkdown
```

From your script, call [`XmlDocMarkdownGenerate`](Cake.XmlDocMarkdown/XmlDocCakeAddin/XmlDocMarkdownGenerate) with the desired input path, output path, and [`XmlDocMarkdownSettings`](XmlDocMarkdown.Core/XmlDocMarkdownSettings).

## XmlDocMarkdown.Core (class library)

[![NuGet](https://img.shields.io/nuget/v/XmlDocMarkdown.Core.svg)](https://www.nuget.org/packages/XmlDocMarkdown.Core)

Call [`XmlDocMarkdownGenerator.Generate`](XmlDocMarkdown.Core/XmlDocMarkdownGenerator/Generate) with the desired input path, output path, and [`XmlDocMarkdownSettings`](XmlDocMarkdown.Core/XmlDocMarkdownSettings).
