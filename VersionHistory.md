# Version History

## Pending

Describe changes here when they're committed to the `master` branch. Move them to **Released** when the project version number is updated in preparation for publishing an updated NuGet package.

Prefix the description of the change with `[major]`, `[minor]`, or `[patch]` in accordance with [Semantic Versioning](https://semver.org/).

* [major] Drop support for `--settings` to avoid potentially conflicting Json.NET dependency.

## Released

### 1.5.3

* [patch] Support `<see langword="__" />`. (Thanks @sungam3r!)
* [patch] Update `Newtonsoft.Json` to `12.0.2`. (Thanks @sungam3r!)

### 1.5.2

* [patch] Prevent crash (though the logic is still wrong).

### 1.5.1

* [patch] Drop `ArgsReading` dependency so that we can be used to document it.

### 1.5.0

* [patch] Add `params` support to method signature.
* [minor] Add `Caller` attributes to method signature.
* [minor] Support hyperlinks to URLs.

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
