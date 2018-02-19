# Contributing to ProjectName

## Prerequisites

* Install [Visual Studio 2017](https://www.visualstudio.com/downloads/) or [Visual Studio Code](https://code.visualstudio.com/) with the [editorconfig extension](https://github.com/editorconfig/editorconfig-vscode).
* Install [.NET Core 2.0](https://www.microsoft.com/net/core).

## Guidelines

* All new code **must** have complete unit tests.
* All public classes, methods, interfaces, enums, etc. **must** have correct XML documentation comments.
* Update [VersionHistory](VersionHistory.md) with a human-readable description of the change.

## How to Build

* Clone the repository: `git clone https://github.com/ejball/RepoName.git`
* `cd RepoName`
* `dotnet test tests/ProjectName.Tests`
