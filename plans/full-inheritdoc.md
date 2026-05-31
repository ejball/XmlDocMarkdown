# Full Inheritdoc Support Plan

Snapshot: May 30, 2026.

## Current State

- `ReflectionFacts.ResolveXmlMember` resolves top-level `<inheritdoc>` only against the current `XmlDocXmlFile`.
- Explicit `cref` inheritance works only when the referenced member is in the same XML file.
- Implicit inheritance searches base definitions and interface maps for documented members already present in the same assembly XML.
- `path` filtering is supported after a source XML member is found.
- Documentation from referenced assemblies is not loaded or indexed unless that assembly is also one of the assemblies being documented.

## Goals

- Inherit documentation from base classes, interfaces, and explicit `cref` targets in assemblies that are referenced but not being documented.
- Keep external documentation available for inheritance without forcing those external assemblies to become output nodes.
- Support deterministic resolution, caching, cycle protection, and useful failure behavior when referenced XML documentation cannot be found.

## Plan

- Introduce a documentation index that can contain primary documented assemblies and external documentation-only assemblies.
- Store XML members by `XmlDocRef` across all indexed XML files, with deterministic collision handling and enough assembly identity to diagnose ambiguous matches.
- Add an external XML documentation source API, for example a resolver callback or settings collection that maps `AssemblyName`/`Assembly` to an `XDocument`.
- Provide a default resolver that looks for XML documentation next to referenced assembly files when the assembly location is available.
- Load external docs lazily from the documented assemblies' referenced assemblies, using caching so repeated inheritdoc resolution is cheap.
- Move inheritdoc resolution out of single-file lookup and into the shared documentation index.
- Resolve explicit `cref` by searching the full index, then applying `path` filtering.
- Resolve implicit type inheritance through base types and interfaces, including external interfaces.
- Resolve implicit method inheritance through override chains, interface maps, explicit interface implementations, generic method definitions, and overload signatures.
- Resolve property and event inheritance through accessor methods, then map the inherited accessor back to the owning property or event.
- Add cycle detection for inherited documentation chains and return the local member unchanged when a cycle or unresolved reference is found.
- Define merge semantics for full support: top-level `<inheritdoc>` replaces missing documentation sections, and section-level `<inheritdoc>` can inherit individual summaries, remarks, params, type params, returns, values, exceptions, examples, and see-also blocks.
- Preserve explicit local documentation when merging inherited sections unless the XML directive is the whole section being resolved.
- Add fixtures with a separate external base assembly and XML file covering base class inheritance, external interface inheritance, explicit `cref`, `path`, generic overloads, properties, events, and unresolved/cyclic inheritdoc cases.
- Add tests proving external docs are used for inheritance but do not create output pages unless the external assembly is also a primary input.
- Document the resolver API and the default adjacent-XML behavior in samples and checked-in docs.