using System;
using System.Collections.Generic;
using System.Text;

namespace Autofac
{
	/// <summary>
	/// Determines when instances supporting IDisposable are disposed.
	/// </summary>
	public enum InstanceOwnership
	{
		/// <summary>
		/// The container/context does not dispose the instances.
		/// </summary>
		External,

		/// <summary>
		/// The instances are disposed when the container/context is disposed.
		/// </summary>
		Container
	}
}
