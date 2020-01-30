using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;

namespace XmlDocMarkdown.Core
{
	internal sealed class XmlDocToc
	{
		public string Path { get; set; }

		public string Title { get; set; }

		public List<XmlDocToc> Children { get; set; }

		public void AddChild(string path, string title)
		{
			string[] parts = path.Split('/');

			XmlDocToc parent = this;
			string relativePath = Path;

			for (int i = 0, n = parts.Length; i < n; i++)
			{
				string part = parts[i];
				string childTitle = part;
				if (i == parts.Length - 1)
				{
					childTitle = Title;
				}
				XmlDocToc child = parent.GetOrCreate(relativePath, childTitle);
				parent = child;
				relativePath += "/" + part;
			}
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
				item = new XmlDocToc() { Path = path, Title = title };
				Children.Add(item);
			}
			return item;
		}

		internal void Save(string tocPath)
		{
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
			- title: Programming
			  link: /learn/programming-models
			  subfolderitems:
				- name: Asynchronous Tasks
				  link: /learn/programming-models/async/overview
				  tertiaryitems:
					- name: Overview
					  link: /learn/programming-models/async/overview
				- name: Asynchronous Actors
				  link: /learn/programming-models/actors/overview
				  tertiaryitems:
					- name: Overview
					  link: /learn/programming-models/actors/overview
					- name: State Machines
					  link: /learn/programming-models/actors/state-machin
			*/
			writer.Write(indent);
			writer.WriteLine("- name: {0}", Title);
			writer.Write(indent);
			writer.WriteLine("  link: {0}", Path);
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
