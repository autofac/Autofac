using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autofac.Component.Registration;

namespace Autofac.Builder
{
    /// <summary>
    /// Creates generic component registrations that will be automatically bound to concrete
    /// types as they are requested.
    /// </summary>
    /// <remarks>
    /// The interface of this class and returned IRegistrar are non-generic as
    /// C# (or the CLR) does not allow partially-constructed generic types like IList&lt;&gt;
    /// to be used as generic arguments.
    /// </remarks>
	class GenericRegistrar : Registrar, IModule, IRegistrar
	{
		Type _implementor;

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericRegistrar"/> class.
        /// </summary>
        /// <param name="implementor">The implementor.</param>
		public GenericRegistrar(Type implementor)
		{
            Enforce.ArgumentNotNull(implementor, "implementor");

			_implementor = implementor;
			Services = new[] { implementor };
		}

		#region IComponentRegistrar Members

        /// <summary>
        /// Registers the component.
        /// </summary>
        /// <param name="container">The container.</param>
		public void Configure(Container container)
		{
            Enforce.ArgumentNotNull(container, "container");
			container.AddRegistrationSource(new GenericRegistrationHandler(
				Services,
				_implementor,
				Ownership,
				Scope,
                ActivatingHandlers,
                ActivatedHandlers));
		}

		#endregion
	}
}
