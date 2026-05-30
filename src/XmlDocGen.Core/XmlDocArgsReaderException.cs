using System.Reflection;
using XmlDocGen.Core.IO;
using XmlDocGen.Core.Markdown;
using XmlDocGen.Core.Nodes;
using XmlDocGen.Core.Pages;
using XmlDocGen.Core.Sites;
using XmlDocGen.Core.Xml;

namespace XmlDocGen.Core;

/// <summary>An exception thrown for invalid command-line arguments.</summary>
public sealed class XmlDocArgsReaderException(string message) : Exception(message);
