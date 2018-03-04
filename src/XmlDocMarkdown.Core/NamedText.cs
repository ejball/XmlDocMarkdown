using System;

namespace XmlDocMarkdown.Core
{
	internal sealed class NamedText
	{
		public NamedText(string name, string text)
		{
			Name = name ?? throw new ArgumentNullException(nameof(name));
			Text = text ?? throw new ArgumentNullException(nameof(text));
		}

		public string Name { get; }

		public string Text { get; }
	}
}
