using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Autofac.Builder
{
    /// <summary>
    /// Registers an already-constructed component registration with the container.
    /// </summary>
    class RegistrationRegistrar : IModule
    {
        IComponentRegistration _registration;

        /// <summary>
        /// Initializes a new instance of the <see cref="RegistrationRegistrar"/> class.
        /// </summary>
        /// <param name="registration">The registration.</param>
        public RegistrationRegistrar(IComponentRegistration registration)
        {
            Enforce.ArgumentNotNull(registration, "registration");
            _registration = registration;
        }

        #region IComponentRegistrar Members

        /// <summary>
        /// Registers the component.
        /// </summary>
        /// <param name="container">The container.</param>
        public void Configure(Container container)
        {
            Enforce.ArgumentNotNull(container, "container");
            container.RegisterComponent(_registration);
        }

        #endregion
    }
}
