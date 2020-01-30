using System;

namespace XmlDocMarkdown.Core
{
	internal sealed class NamedText
	{
		public NamedText(string name, string title, string text)
		{
			Name = name ?? throw new ArgumentNullException(nameof(name));
			Title = title ?? throw new ArgumentNullException(nameof(title));
			Text = text ?? throw new ArgumentNullException(nameof(text));
		}

		public string Name { get; }

		public string Title { get; }

		public string Text { get; }
	}
}
