using System.ComponentModel;

namespace System.Runtime.CompilerServices
{
	/// <summary>
	/// Bug fix for C# 9 record when not using .NET 5.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	internal class IsExternalInit
	{
	}
}
