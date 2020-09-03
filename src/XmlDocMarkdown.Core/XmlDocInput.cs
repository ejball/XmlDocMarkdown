using System.Reflection;

namespace XmlDocMarkdown.Core
{
	/// <summary>
	/// The input for generating Markdown from .NET XML documentation comments.
	/// </summary>
	public sealed class XmlDocInput
	{
		/// <summary>
		/// The path of the assembly to load.
		/// </summary>
		/// <remarks>Optional; uses <c>Assembly</c> if omitted.</remarks>
		public string AssemblyPath { get; set; }

		/// <summary>
		/// An already-loaded assembly.
		/// </summary>
		/// <remarks>Optional; uses <c>AssemblyPath</c> if omitted.</remarks>
		public Assembly Assembly { get; set; }

		/// <summary>
		/// The path of the XML documentation for the assembly.
		/// </summary>
		/// <remarks>Optional; changes the extension of <c>AssemblyPath</c> or <c>Assembly.Location</c> if omitted.</remarks>
		public string XmlDocPath { get; set; }
	}
}
