namespace ExampleAssembly
{
	/// <summary>
	/// An abstract class.
	/// </summary>
	public abstract class ExampleAbstractClass
	{
		/// <summary>
		/// A public abstract read-only property.
		/// </summary>
		public abstract int PublicProperty { get; }

		/// <summary>
		/// A public and protected virtual property.
		/// </summary>
		public virtual int PublicProtectedProperty { get; protected set; }

		/// <summary>
		/// A protected abstract property.
		/// </summary>
		protected abstract int ProtectedPropertyCore { get; set; }

		/// <summary>
		/// A protected and private virtual property.
		/// </summary>
		protected virtual int ProtectedPrivateProperty { private get; set; }

		/// <summary>
		/// A public abstract method.
		/// </summary>
		public abstract void PublicMethod();

		/// <summary>
		/// A protected abstract method.
		/// </summary>
		protected abstract void ProtectedMethodCore();

		/// <summary>
		/// An internal abstract method.
		/// </summary>
		internal abstract void InternalMethodCore();

		/// <summary>
		/// A protected internal abstract method.
		/// </summary>
		protected internal abstract void ProtectedInternalMethodCore();
	}
}
