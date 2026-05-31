using System.Reflection;
using System.Xml.Linq;
using XmlDocGen.Core.IO;
using XmlDocGen.Core.Markdown;
using XmlDocGen.Core.Nodes;
using XmlDocGen.Core.Sites;
using XmlDocGen.Core.Xml;

if (args.Length != 2)
{
	Console.Error.WriteLine("Usage: Samples.LibraryApi <assembly-name> <output-dir>");
	return 2;
}

var assembly = Assembly.Load(args[0]);
var xml = new XmlDocXmlFile(XDocument.Load(Path.ChangeExtension(assembly.Location, ".xml")));
var tree = XmlDocTree.Create([(assembly, xml)]);
var site = new MarkdownSiteBuilder(new XmlDocSiteBuilderSettings { NewLine = "\n" }).Build(tree);
new XmlDocSiteWriter(new XmlDocSiteWriterSettings { ShouldClean = true }).Write(site, args[1]);
return 0;
