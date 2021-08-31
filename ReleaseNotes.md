# Release Notes

## 2.7.0

* Support C# 9 records. (Thanks [bigtlb](https://github.com/bigtlb)!)

## 2.6.0

* Use `./` with relative links (for [GitLab wikis](https://community.opengroup.org/examples/wiki/-/wikis/Basics/Linking#internal-wiki-pages)).

## 2.5.2

* Fix tuple rendering bug.

## 2.5.1

* Fix nullable references bugs.

## 2.5.0

* Support nullable references. (Thanks [bgrainger](https://github.com/bgrainger)!)

## 2.4.0

* Support tuple syntax, including element names.

## 2.3.2

* Add .NET 5 target to `xmldocmd`. (Thanks [qmfrederik](https://github.com/qmfrederik)!)

## 2.3.1

* Allow .NET Core 3.1 build to run on .NET 5.

## 2.3.0

* Make command-line implementation available from `XmlDocMarkdown.Core`.
* Allow command-line input to be the name of the assembly.

## 2.2.0

* Support generating markdown from a loaded assembly.

## 2.1.0

* Add `--skip-unbrowsable`, `--namespace-pages`, `--front-matter`, `--permalink`, `--toc`, and `--toc-prefix`.
* Prevent NRE for F# compiler generated implicit parameters. (Thanks [SteveGilham](https://github.com/SteveGilham)!)

## 2.0.2

* `xmldocmd` can also support .NET Core 2.1 and .NET Core 2.2. (Thanks [sungam3r](https://github.com/sungam3r)!)

## 2.0.1

* Update `xmldocmd` to .NET Core 3.0 so it can load .NET Standard 2.1 libraries.

## 2.0.0

* **Breaking:** Drop support for `--settings` to avoid potentially conflicting Json.NET dependency.

## 1.5.3

* Support `<see langword="__" />`. (Thanks [sungam3r](https://github.com/sungam3r)!)
* Update `Newtonsoft.Json` to `12.0.2`. (Thanks [sungam3r](https://github.com/sungam3r)!)

## 1.5.2

* Prevent crash (though the logic is still wrong).

## 1.5.1

* Drop `ArgsReading` dependency so that we can be used to document it.

## 1.5.0

* Add `params` support to method signature.
* Add `Caller` attributes to method signature.
* Support hyperlinks to URLs.

## 1.4.3

* Fix missing documentation for `out` and `ref` parameters.

## 1.4.2

* Add .NET Core Global Tool: `xmldocmd`

## 1.4.1

* Fixes for Cake addin audit (issue #9).

## 1.4.0

* Target .NET Standard 2.0 and/or .NET Framework 4.7.

## 1.3.1

* Use Cake contrib icon.

## 1.3.0

* Add `--settings` command-line option.
* Add minimal external documentation support (same repository).

## 1.2.1

* Remove external dependencies from `Cake.XmlDocMarkdown`.

## 1.2.0

* Move Cake addin into `Cake.XmlDocMarkdown`.

## 1.1.0

* Publish Cake addin and class library `XmlDocMarkdown.Core`.
* Improve full signature line wrapping.

## 1.0.1

* Escapes code within backticks properly.

## 1.0.0

* Initial release.
