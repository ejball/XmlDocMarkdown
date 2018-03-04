namespace XmlDocMarkdown.Core
{
	/// <summary>
	/// The minimum visibility for documented types and members.
	/// </summary>
	public enum XmlDocVisibilityLevel
	{
		/// <summary>
		/// All types and members are documented.
		/// </summary>
		Private,

		/// <summary>
		/// Only public, protected, and internal types and members are documented.
		/// </summary>
		Internal,

		/// <summary>
		/// Only public and protected types and members are documented.
		/// </summary>
		Protected,

		/// <summary>
		/// Reserved for internal use.
		/// </summary>
		ProtectedInternal,

		/// <summary>
		/// Only public types and members are documented.
		/// </summary>
		Public,
	}
}
