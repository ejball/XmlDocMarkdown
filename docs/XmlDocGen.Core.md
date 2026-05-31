# XmlDocGen.Core assembly



## XmlDocGen.Core namespace


| public type | description |
| --- | --- |
| class [XmlDocArgsReader](./XmlDocGen.Core/XmlDocArgsReader.md) | Reads command-line arguments. |
| class [XmlDocArgsReaderException](./XmlDocGen.Core/XmlDocArgsReaderException.md) | An exception thrown for invalid command-line arguments. |
| class [XmlDocGenApp](./XmlDocGen.Core/XmlDocGenApp.md) | Command-line entry point for documentation generation. |
| class [XmlDocGenAppContext](./XmlDocGen.Core/XmlDocGenAppContext.md) | Configuration context passed to host tools. |

## XmlDocGen.Core.CSharp namespace


| public type | description |
| --- | --- |
| class [CSharpSignature](./XmlDocGen.Core.CSharp/CSharpSignature.md) | A C# signature and its token stream. |
| abstract class [CSharpSignatureBuilder](./XmlDocGen.Core.CSharp/CSharpSignatureBuilder.md) | Builds structured C# signatures from documentation nodes. |
| enum [CSharpTokenKind](./XmlDocGen.Core.CSharp/CSharpTokenKind.md) | Kinds of C# signature tokens. |
| record [CSharpToken](./XmlDocGen.Core.CSharp/CSharpToken.md) | A token in a C# signature. |

## XmlDocGen.Core.IO namespace


| public type | description |
| --- | --- |
| class [XmlDocSiteWriter](./XmlDocGen.Core.IO/XmlDocSiteWriter.md) | Writes generated documentation sites to disk. |
| class [XmlDocSiteWriteResult](./XmlDocGen.Core.IO/XmlDocSiteWriteResult.md) | The result of writing a generated site. |
| class [XmlDocSiteWriterSettings](./XmlDocGen.Core.IO/XmlDocSiteWriterSettings.md) | Settings for writing a generated site. |
| enum [XmlDocNewLineComparison](./XmlDocGen.Core.IO/XmlDocNewLineComparison.md) | Controls newline comparison when diffing files. |
| interface [IXmlDocFileSystem](./XmlDocGen.Core.IO/IXmlDocFileSystem.md) | Abstracts file-system operations. |

## XmlDocGen.Core.Markdown namespace


| public type | description |
| --- | --- |
| class [MarkdownPageRenderer](./XmlDocGen.Core.Markdown/MarkdownPageRenderer.md) | A page renderer that emits Markdown. |
| class [MarkdownRenderer](./XmlDocGen.Core.Markdown/MarkdownRenderer.md) | Renders node and XML content as Markdown building blocks. |
| class [MarkdownSiteBuilder](./XmlDocGen.Core.Markdown/MarkdownSiteBuilder.md) | Convenience builder for Markdown sites. |
| class [MarkdownWriter](./XmlDocGen.Core.Markdown/MarkdownWriter.md) | Low-level Markdown emit helpers. |

## XmlDocGen.Core.Nodes namespace


| public type | description |
| --- | --- |
| static class [ReflectionFacts](./XmlDocGen.Core.Nodes/ReflectionFacts.md) | Shared reflection helpers for node construction and rendering. |
| class [XmlDocAssemblyNode](./XmlDocGen.Core.Nodes/XmlDocAssemblyNode.md) | An assembly documentation node. |
| class [XmlDocMemberNode](./XmlDocGen.Core.Nodes/XmlDocMemberNode.md) | A member documentation node. |
| class [XmlDocNamespaceNode](./XmlDocGen.Core.Nodes/XmlDocNamespaceNode.md) | A namespace documentation node. |
| abstract class [XmlDocNode](./XmlDocGen.Core.Nodes/XmlDocNode.md) | Base class for an assembly, namespace, type, or member documentation node. |
| abstract class [XmlDocNodeVisibility](./XmlDocGen.Core.Nodes/XmlDocNodeVisibility.md) | A composable node-visibility filter. |
| class [XmlDocTree](./XmlDocGen.Core.Nodes/XmlDocTree.md) | A documentation tree built from one or more assemblies. |
| class [XmlDocTypeNode](./XmlDocGen.Core.Nodes/XmlDocTypeNode.md) | A type documentation node. |
| enum [XmlDocMemberKind](./XmlDocGen.Core.Nodes/XmlDocMemberKind.md) | Kinds of documented members. |
| enum [XmlDocTypeKind](./XmlDocGen.Core.Nodes/XmlDocTypeKind.md) | Kinds of documented types. |
| enum [XmlDocVisibility](./XmlDocGen.Core.Nodes/XmlDocVisibility.md) | The exact visibility of a documentation node. |

## XmlDocGen.Core.Pages namespace


| public type | description |
| --- | --- |
| abstract class [XmlDocExternalLinkResolver](./XmlDocGen.Core.Pages/XmlDocExternalLinkResolver.md) | Resolves links to documentation outside the current tree. |
| class [XmlDocPage](./XmlDocGen.Core.Pages/XmlDocPage.md) | A generated documentation page before it is rendered. |
| static class [XmlDocPageBuilder](./XmlDocGen.Core.Pages/XmlDocPageBuilder.md) | Builds pages from a tree and page map. |
| class [XmlDocPageContext](./XmlDocGen.Core.Pages/XmlDocPageContext.md) | Context available while rendering a page. |
| abstract class [XmlDocPageMap](./XmlDocGen.Core.Pages/XmlDocPageMap.md) | Maps documentation nodes to extensionless page paths. |
| abstract class [XmlDocPageRenderer](./XmlDocGen.Core.Pages/XmlDocPageRenderer.md) | Renders a page to a file. |
| class [XmlDocSourceLinks](./XmlDocGen.Core.Pages/XmlDocSourceLinks.md) | Provides source-link URLs for reflected members. |
| abstract class [XmlDocUrlMapper](./XmlDocGen.Core.Pages/XmlDocUrlMapper.md) | Maps rendered pages and nodes to URLs. |
| record [XmlDocRenderedFile](./XmlDocGen.Core.Pages/XmlDocRenderedFile.md) | A rendered page file. |

## XmlDocGen.Core.Sites namespace


| public type | description |
| --- | --- |
| class [XmlDocSite](./XmlDocGen.Core.Sites/XmlDocSite.md) | A generated documentation site. |
| class [XmlDocSiteBuilder](./XmlDocGen.Core.Sites/XmlDocSiteBuilder.md) | Builds a generated documentation site. |
| class [XmlDocSiteBuilderSettings](./XmlDocGen.Core.Sites/XmlDocSiteBuilderSettings.md) | Settings for building a site. |
| record [XmlDocSiteFile](./XmlDocGen.Core.Sites/XmlDocSiteFile.md) | A single generated output file. |

## XmlDocGen.Core.Xml namespace


| public type | description |
| --- | --- |
| class [XmlDocXmlBlock](./XmlDocGen.Core.Xml/XmlDocXmlBlock.md) | A block of parsed XML documentation content. |
| class [XmlDocXmlException](./XmlDocGen.Core.Xml/XmlDocXmlException.md) | Parsed XML documentation for an exception. |
| class [XmlDocXmlFile](./XmlDocGen.Core.Xml/XmlDocXmlFile.md) | An in-memory representation of a compiler-generated XML documentation file. |
| class [XmlDocXmlInheritDoc](./XmlDocGen.Core.Xml/XmlDocXmlInheritDoc.md) | The raw XML inheritdoc directive. |
| class [XmlDocXmlInline](./XmlDocGen.Core.Xml/XmlDocXmlInline.md) | Parsed inline XML documentation content. |
| class [XmlDocXmlMember](./XmlDocGen.Core.Xml/XmlDocXmlMember.md) | The parsed XML documentation for a single member. |
| class [XmlDocXmlParameter](./XmlDocGen.Core.Xml/XmlDocXmlParameter.md) | Parsed XML documentation for a parameter. |
| class [XmlDocXmlSeeAlso](./XmlDocGen.Core.Xml/XmlDocXmlSeeAlso.md) | Parsed XML documentation for a see-also item. |
| enum [XmlDocXmlInlineKind](./XmlDocGen.Core.Xml/XmlDocXmlInlineKind.md) | Kinds of inline XML documentation content. |
| enum [XmlDocXmlListKind](./XmlDocGen.Core.Xml/XmlDocXmlListKind.md) | Kinds of XML documentation lists. |
| readonly struct [XmlDocRef](./XmlDocGen.Core.Xml/XmlDocRef.md) | An XML documentation identifier, e.g. `T:My.Type` or `M:My.Type.Method(System.Int32)`. |

<!-- DO NOT EDIT: generated by XmlDocGen for XmlDocGen.Core -->
