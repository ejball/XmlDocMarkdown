using System;

namespace XmlDocMarkdown.Core
{
	public sealed class NamedText
	{
		public NamedText(string name, string text)
		{
			if (name == null)
				throw new ArgumentNullException(nameof(name));
			if (text == null)
				throw new ArgumentNullException(nameof(text));

			Name = name;
			Text = text;
		}

		public string Name { get; }

		public string Text { get; }
	}
}
