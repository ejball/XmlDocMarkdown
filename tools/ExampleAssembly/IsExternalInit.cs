using System.ComponentModel;

#pragma warning disable IDE0130
namespace System.Runtime.CompilerServices
#pragma warning restore IDE0130
{
	/// <summary>
	/// Bug fix for C# 9 record when not using .NET 5.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	internal class IsExternalInit
	{
	}
}
