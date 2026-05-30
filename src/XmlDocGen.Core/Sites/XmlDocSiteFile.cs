using XmlDocGen.Core.Nodes;
using XmlDocGen.Core.Pages;

namespace XmlDocGen.Core.Sites;

/// <summary>A single generated output file.</summary>
public sealed record XmlDocSiteFile(string Path, string Text);
