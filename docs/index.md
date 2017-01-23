# XmlDocMarkdown

**XmlDocMarkdown** is a command-line tool that generates Markdown from [.NET XML documentation comments](https://msdn.microsoft.com/en-us/library/b2s063f7.aspx).

## Installation

Use [NuGet](https://www.nuget.org/) to install **XmlDocMarkdown** from its [NuGet package](https://www.nuget.org/packages/XmlDocMarkdown).

For example, `nuget install XmlDocMarkdown -excludeversion` will download the latest version of `XmlDocMarkdown.exe` into `XmlDocMarkdown/tools`.

On Mac or Linux, use [Mono](http://www.mono-project.com/) to run `NuGet.exe` as well as the Facility command-line tool itself.

## Usage

The `XmlDocMarkdown` command-line tool accepts the path to the input assembly, the path to the output directory, and a number of options.

The XML documentation file should be in the same directory as the input assembly.

The output directory will be created if necessary.

For example, `xmldocmarkdown MyLibrary.dll docs` generates Markdown documentation in the `docs` directory for the `MyLibrary.dll` assembly. The compiler-generated `MyLibrary.xml`file should be in the same directory as `MyLibrary.dll`.

## Options

* `--newlines (auto|lf|crlf)`: Indicates the newline used in the output. Defaults to `auto`, which uses CRLF or LF, depending on the platform.
* `--dryrun`: Executes the tool without making changes to the file system.
* `--verify`: Executes the tool without making changes to the file system, but exits with error code 1 if changes would be made. Typically used in build scripts to ensure that any changes have been reflected in the generated code.
* `--quiet`: Suppresses normal console output.
