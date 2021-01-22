# XmlDocMarkdown

**XmlDocMarkdown** generates Markdown from [.NET XML documentation comments](https://msdn.microsoft.com/en-us/library/b2s063f7.aspx).

It is distributed as a class library, .NET tool, .NET Framework console app, and Cake addin.

For example output, see the [Markdown documents](https://github.com/ejball/XmlDocMarkdown/blob/master/docs/ExampleAssembly.md) for the [documentation](ExampleAssembly.md) of the [ExampleAssembly](https://github.com/ejball/XmlDocMarkdown/tree/master/tools/ExampleAssembly) class library.

The goal of this tool is to generate Markdown documentation for .NET class libraries that are simple enough to be read and understood in [raw form](https://raw.githubusercontent.com/ejball/XmlDocMarkdown/master/docs/ExampleAssembly/ExampleClass.md), as [rendered in GitHub](https://github.com/ejball/XmlDocMarkdown/blob/master/docs/ExampleAssembly/ExampleClass.md), or when used to generate [web pages](https://ejball.com/XmlDocMarkdown/ExampleAssembly/ExampleClass.html) using [Jekyll](https://jekyllrb.com/) and [GitHub Pages](https://pages.github.com/). To that end, it generates standard [GitHub Flavored Markdown](https://github.github.com/gfm/) without relying on raw HTML tags.

For a more full-featured documentation generation tool, check out [DocFX](https://dotnet.github.io/docfx/) or [Sandcastle](https://github.com/EWSoftware/SHFB).

## Usage

The most reliable way to use **XmlDocMarkdown** is to build and run a command-line tool that references the **XmlDocMarkdown.Core** class library and the assembly that you want to document. This ensures that the assembly and all of its dependencies are loaded properly.

This is easier than it sounds, because the class library contains the full implementation of the command-line application via [XmlDocMarkdownApp.Run](https://ejball.com/XmlDocMarkdown/XmlDocMarkdown.Core/XmlDocMarkdownApp/Run.html). For example, here is how the `ArgsReading` library defines its documentation generation tool:

* [XmlDocGen.csproj](https://github.com/ejball/ArgsReading/blob/master/tools/XmlDocGen/XmlDocGen.csproj)
* [XmlDocGenApp.cs](https://github.com/ejball/ArgsReading/blob/master/tools/XmlDocGen/XmlDocGenApp.cs)

Depending on the type of console application that you build, you can run `XmlDocGen.exe` directly or run `dotnet path-to-XmlDocGen.dll`.

If your dependencies are simple enough, you can use the standard .NET tool (`xmldocmd`), console app (`XmlDocMarkdown`), or Cake addin (`Cake.XmlDocMarkdown`), all documented below.

### Arguments

The command-line tool accepts the name or path of the input assembly, the path of the output directory, and a number of options. (Use the name of the input assembly if you have built your own command-line tool, as recommended above.)

The XML documentation file should be in the same directory as the input assembly.

The output directory will be created if necessary.

For example, `xmldocmd MyLibrary.dll docs` generates Markdown documentation in the `docs` directory for the `MyLibrary.dll` assembly. The compiler-generated `MyLibrary.xml` file should be in the same directory as `MyLibrary.dll`.

### Options

* `--source <url>`: The URL (absolute or relative) of the folder containing the source code of the assembly, e.g. at GitHub. Required to generate source code links in the See Also sections for types. This assumes that each type is defined in a `.cs` file that matches its name.
* `--namespace <ns>`: The root namespace of the input assembly. Used to generate source code links in the See Also sections for types. If omitted, the tool guesses the root namespace from the exported types.
* `--visibility (public|protected|internal|private)`: The minimum visibility for documented types and members. If `public`, only public types and members are documented. If `protected`, only public and protected types and members are documented. Similarly for `internal` and `private`. Defaults to `protected`.
* `--obsolete`: Generates documentation for obsolete types and members, which are not documented by default.
* `--external <ns>`: Generates links to external documentation for the specified namespace, which must be documented in the same repository with similar options.
* `--clean`: Delete previously generated files that are no longer used.
* `--verify`: Executes the tool without making changes to the file system, but exits with error code 1 if changes would be made. Typically used in build scripts to ensure that any changes have been reflected in the generated code.
* `--dryrun`: Executes the tool without making changes to the file system.
* `--quiet`: Suppresses normal console output.
* `--newline (auto|lf|crlf)`: Indicates the newline used in the output. Defaults to `auto`, which uses CRLF or LF, depending on the platform.

### XmlDocMarkdown.Core (class library)

[![NuGet](https://img.shields.io/nuget/v/XmlDocMarkdown.Core.svg)](https://www.nuget.org/packages/XmlDocMarkdown.Core)

To implement a command-line tool, call [XmlDocMarkdownApp.Run](https://ejball.com/XmlDocMarkdown/XmlDocMarkdown.Core/XmlDocMarkdownApp/Run.html) with the command-line arguments.

To use the library directly, call [`XmlDocMarkdownGenerator.Generate`](XmlDocMarkdown.Core/XmlDocMarkdownGenerator/Generate) with the desired input path, output path, and [`XmlDocMarkdownSettings`](XmlDocMarkdown.Core/XmlDocMarkdownSettings).

### xmldocmd (.NET tool)

[![NuGet](https://img.shields.io/nuget/v/xmldocmd.svg)](https://www.nuget.org/packages/xmldocmd)

To install `xmldocmd`: `dotnet tool install xmldocmd -g`

### XmlDocMarkdown (.NET Framework console app)

[![NuGet](https://img.shields.io/nuget/v/XmlDocMarkdown.svg)](https://www.nuget.org/packages/XmlDocMarkdown)

`nuget install XmlDocMarkdown -excludeversion` will download the latest version of `XmlDocMarkdown.exe` into `XmlDocMarkdown/tools`.

On Mac or Linux, use [Mono](http://www.mono-project.com/) to run `nuget` as well as the command-line tool itself.

The command-line arguments and options are the same as `xmldocmd` above.

### Cake.XmlDocMarkdown (Cake addin)

[![NuGet](https://img.shields.io/nuget/v/Cake.XmlDocMarkdown.svg)](https://www.nuget.org/packages/Cake.XmlDocMarkdown)

See [https://cakebuild.net/extensions/cake-xmldocmarkdown/](https://cakebuild.net/extensions/cake-xmldocmarkdown/).
