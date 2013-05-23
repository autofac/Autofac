using System;
using System.Collections.Generic;
using System.Linq;
using Autofac.Builder;

namespace Autofac.Features.ResolveAnything
{
    /// <summary>
    /// Extension methods for configuring the <see cref="Autofac.Features.ResolveAnything.AnyConcreteTypeNotAlreadyRegisteredSource"/>.
    /// </summary>
    public static class AnyConcreteTypeNotAlreadyRegisteredSourceExtensions
    {
        /// <summary>
        /// Fluent method for setting the registration configuration on <see cref="Autofac.Features.ResolveAnything.AnyConcreteTypeNotAlreadyRegisteredSource"/>.
        /// </summary>
        /// <param name="source">The registration source to configure.</param>
        /// <param name="configurationAction">A configuration action that will run on any registration provided by the source.</param>
        /// <returns>
        /// The <paramref name="source" /> with the registration configuration set.
        /// </returns>
        public static AnyConcreteTypeNotAlreadyRegisteredSource WithRegistrationsAs(this AnyConcreteTypeNotAlreadyRegisteredSource source, Action<IRegistrationBuilder<object, ConcreteReflectionActivatorData, SingleRegistrationStyle>> configurationAction)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            source.RegistrationConfiguration = configurationAction;
            return source;
        }
    }
}
