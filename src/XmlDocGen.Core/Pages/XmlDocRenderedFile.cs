using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Text.Json;
using XmlDocGen.Core.Nodes;
using XmlDocGen.Core.Xml;

namespace XmlDocGen.Core.Pages;

/// <summary>A rendered page file.</summary>
public sealed record XmlDocRenderedFile(string Path, string Text);
