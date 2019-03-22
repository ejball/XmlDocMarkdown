# Contributing to XmlDocMarkdown

## Prerequisites

* Install [Visual Studio 2017](https://visualstudio.microsoft.com/downloads/) or [Visual Studio Code](https://code.visualstudio.com/) with the [editorconfig extension](https://github.com/editorconfig/editorconfig-vscode).
* Install [.NET Core 2.x](https://dotnet.microsoft.com/download).

## Guidelines

* All new code **must** have complete unit tests.
* All public classes, methods, interfaces, enums, etc. **must** have correct XML documentation comments.
* Update [VersionHistory](VersionHistory.md) with a human-readable description of the change.

## How to Build

* `git clone https://github.com/ejball/XmlDocMarkdown.git`
* `cd XmlDocMarkdown`
* `dotnet test`
