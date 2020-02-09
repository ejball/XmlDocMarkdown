using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;

namespace XmlDocMarkdown.Core
{
	/// <summary>
	/// This class builds a .yml table of contents along the lines of what you see here:
	/// https://jekyllrb.com/tutorials/navigation/#scenario-4-three-level-navigation-list
	/// </summary>
	internal sealed class XmlDocToc
	{
		public string Prefix { get; set; }

		public string Path { get; set; }

		public string Title { get; set; }

		public List<XmlDocToc> Children { get; set; }

		public void AddChild(string path, string parent, string title)
		{
			if (parent == null || parent == Path)
			{
				GetOrCreate(path, title);
				return;
			}

			var parentItem = FindParent(parent);
			if (parentItem == null)
			{
				throw new Exception(string.Format("Parent '{0}' not found?", parent));
			}			

			parentItem.GetOrCreate(path, title);
		}

		private XmlDocToc FindParent(string parent)
		{
			if (Children == null)
			{
				return null;
			}

			foreach (var item in Children)
			{
				if (item.Path == parent)
				{
					return item;
				}
				var result = item.FindParent(parent);
				if (result != null)
				{
					return result;
				}
			}
			return null;
		}

		private XmlDocToc GetOrCreate(string path, string title)
		{
			if (Children == null)
			{
				Children = new List<XmlDocToc>();
			}
			var item = (from i in Children where i.Path == path select i).FirstOrDefault();
			if (item == null)
			{
				item = new XmlDocToc() { Path = path, Title = title, Prefix = Prefix };
				Children.Add(item);
			}
			return item;
		}

		internal void Save(string tocPath)
		{
			Directory.CreateDirectory(System.IO.Path.GetDirectoryName(tocPath));
			using (StreamWriter writer = new StreamWriter(tocPath, false, Encoding.UTF8))
			{
				writer.WriteLine("toc:");
				Save(writer, "  ");
			}
		}

		internal void Save(StreamWriter writer, string indent)
		{
			/* 
			toc:
			- title: ...
			  link: relative link to the permalink
			  subfolderitems:
				- name: ...
				  link: ...
			*/

			string p = Path;
			if (p.EndsWith(".md", StringComparison.Ordinal))
			{
				int pos = p.LastIndexOf('.');
				if (pos > 0)
				{
					p = p.Substring(0, pos);
				}
			}

			writer.Write(indent);
			writer.WriteLine("- name: {0}", Title);
			writer.Write(indent);
			if (!string.IsNullOrEmpty(Prefix))
			{
				writer.WriteLine("  link: {0}/{1}", Prefix, p);
			}
			else
			{
				writer.WriteLine("  link: {0}", p);
			}
			if (Children != null && Children.Count > 0)
			{
				writer.Write(indent);
				writer.WriteLine("  subfolderitems:");
				foreach (var item in Children)
				{
					item.Save(writer, indent + "  ");
				}
			}
		}
	}
}
