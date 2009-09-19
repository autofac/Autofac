using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autofac.Core.Activators;
using Autofac.Core;

namespace Autofac.Builder
{
    /// <summary>
    /// Generates activators in an IRegistrationSource.
    /// </summary>
    /// <typeparam name="TActivatorData">Data associated with the specific kind of activator.</typeparam>
    public interface IActivatorGenerator<TActivatorData>
    {
        /// <summary>
        /// Given a requested service and registration data, attempt to generate an
        /// activator for the service.
        /// </summary>
        /// <param name="service">Service that was requested.</param>
        /// <param name="configuredServices">Services associated with the activator generator.</param>
        /// <param name="reflectionActivatorData">Data specific to this kind of activator.</param>
        /// <param name="activator">Resulting activator.</param>
        /// <param name="services">Services provided by the activator.</param>
        /// <returns>True if an activator could be generated.</returns>
        bool TryGenerateActivator(
            Service service,
            IEnumerable<Service> configuredServices,
            TActivatorData reflectionActivatorData,
            out IInstanceActivator activator,
            out IEnumerable<Service> services);
    }
}
