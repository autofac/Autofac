using System;
using System.Collections.Generic;
using System.Linq;

namespace Autofac.Integration.Wcf
{
    /// <summary>
    /// Defines a strategy for resolving which service type should be
    /// used for hosting vs. which type is the actual service implementation.
    /// </summary>
    public interface IServiceImplementationDataProvider
    {
        /// <summary>
        /// Gets data about a service implementation.
        /// </summary>
        /// <param name="value">
        /// The constructor string passed in to the service host factory
        /// that is used to determine which type to host/use as a service
        /// implementation.
        /// </param>
        /// <returns>
        /// A <see cref="Autofac.Integration.Wcf.ServiceImplementationData"/>
        /// object containing information about which type to use in
        /// the service host and how to resolve the implementation.
        /// </returns>
        ServiceImplementationData GetServiceImplementationData(string value);
    }
}
