using System;
using System.Collections.Generic;
using System.Text;

namespace Autofac
{
	/// <summary>
	/// Determines when instances are created and how they are shared.
	/// </summary>
	public enum InstanceScope
	{
		/// <summary>
		/// Each request for an instance will return the same object, regardless
		/// of any subcontexts created.
		/// </summary>
		Singleton,

		/// <summary>
		/// Each request for an instance in the same container will return the same object.
		/// </summary>
		Container,

		/// <summary>
		/// Each request for an instance will return a new object.
		/// </summary>
		Factory
	}
}
