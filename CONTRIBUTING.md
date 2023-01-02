# Contributing

## If the change affects the API

New documentation will be required, so run

* `.\build.ps1 generate-docs`

as appropriate, and add the updated documents to your branch.

## Publishing

* To publish the library, update the `<VersionPrefix>` in [`Directory.Build.props`](Directory.Build.props), add a corresponding section to the top of [`ReleaseNotes.md`](ReleaseNotes.md), commit, and push.

## Template

* This repository uses the [`faithlife-build`](https://github.com/Faithlife/CSharpTemplate/tree/faithlife-build) template of [`Faithlife/CSharpTemplate`](https://github.com/Faithlife/CSharpTemplate).
