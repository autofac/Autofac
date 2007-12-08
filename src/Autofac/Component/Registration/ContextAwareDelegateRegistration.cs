using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autofac.Component.Activation;
using Autofac.Component;
using Autofac.Component.Scope;

namespace Autofac.Component.Registration
{
	/// <summary>
	/// Register a ComponentActivator that implements a strongly-typed delegate signature.
	/// </summary>
	/// <remarks>
	/// Note - the objects returned from the activator will not be disposed by the context.
	/// </remarks>
	class ContextAwareDelegateRegistration : ComponentRegistration
	{
		/// <summary>
		/// Create a new instance.
		/// </summary>
		/// <param name="creator">A delegate type that will be exposed as a service.</param>
		/// <param name="activator">An activator that will be passed the named parameters
		/// from the delegate and which should return an object compatible with the return
		/// type of <paramref name="creator"/>.</param>
		public ContextAwareDelegateRegistration(
			Type creator,
			ComponentActivator activator)
			: base(
				new[]{ new TypedService(creator) },
				new InterceptingDelegateActivator(creator, activator),
				new ContainerScope(),
				InstanceOwnership.Container)
		{
		}
	}
}
