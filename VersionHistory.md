# Version History

### 2.x.x 
* Add `--source-style` to extend the generation of source URLs for the `--source` option

### 2.1.0

* Add `--skip-unbrowsable`, `--namespace-pages`, `--front-matter`, `--permalink`, `--toc`, and `--toc-prefix`.
* Prevent NRE for F# compiler generated implicit parameters. (Thanks [SteveGilham](https://github.com/SteveGilham)!)

### 2.0.2

* `xmldocmd` can also support .NET Core 2.1 and .NET Core 2.2. (Thanks [sungam3r](https://github.com/sungam3r)!)

### 2.0.1

* Update `xmldocmd` to .NET Core 3.0 so it can load .NET Standard 2.1 libraries.

### 2.0.0

* **Breaking:** Drop support for `--settings` to avoid potentially conflicting Json.NET dependency.

### 1.5.3

* Support `<see langword="__" />`. (Thanks [sungam3r](https://github.com/sungam3r)!)
* Update `Newtonsoft.Json` to `12.0.2`. (Thanks [sungam3r](https://github.com/sungam3r)!)

### 1.5.2

* Prevent crash (though the logic is still wrong).

### 1.5.1

* Drop `ArgsReading` dependency so that we can be used to document it.

### 1.5.0

* Add `params` support to method signature.
* Add `Caller` attributes to method signature.
* Support hyperlinks to URLs.

### 1.4.3

* Fix missing documentation for `out` and `ref` parameters.

### 1.4.2

* Add .NET Core Global Tool: `xmldocmd`

### 1.4.1

* Fixes for Cake addin audit (issue #9).

### 1.4.0

* Target .NET Standard 2.0 and/or .NET Framework 4.7.

### 1.3.1

* Use Cake contrib icon.

### 1.3.0

* Add `--settings` command-line option.
* Add minimal external documentation support (same repository).

### 1.2.1

* Remove external dependencies from `Cake.XmlDocMarkdown`.

### 1.2.0

* Move Cake addin into `Cake.XmlDocMarkdown`.

### 1.1.0

* Publish Cake addin and class library `XmlDocMarkdown.Core`.
* Improve full signature line wrapping.

### 1.0.1

* Escapes code within backticks properly.

### 1.0.0

* Initial release.
