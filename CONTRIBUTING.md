# Contributing to XmlDocMarkdown

## Prerequisites

* Install [Visual Studio 2019](https://visualstudio.microsoft.com/downloads/) or [Visual Studio Code](https://code.visualstudio.com/).
* Install [.NET Core 3.x](https://dotnet.microsoft.com/download).

## Guidelines

* All new code **must** have complete unit tests.
* All public classes, methods, interfaces, enums, etc. **must** have correct XML documentation comments.
* Update [VersionHistory](VersionHistory.md) with a human-readable description of the change.

## How to Build

* `git clone https://github.com/ejball/XmlDocMarkdown.git`
* `cd XmlDocMarkdown`
* `dotnet test`

### If the change affects the API

New documentation will be required, so run

* .\build.cmd generate-docs
or
* ./build.sh generate-docs

as appropriate, and add the updated documents to your branch
