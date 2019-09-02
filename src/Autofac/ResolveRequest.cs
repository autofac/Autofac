using System.Collections.Generic;
using Autofac.Core;

namespace Autofac
{
    /// <summary>
    /// The details of an individual request to resolve a service.
    /// </summary>
    public class ResolveRequest
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ResolveRequest"/> class.
        /// </summary>
        /// <param name="service">The service being resolved.</param>
        /// <param name="registration">The component registration for the service.</param>
        /// <param name="parameters">The parameters used when resolving the service.</param>
        public ResolveRequest(Service service, IComponentRegistration registration, IEnumerable<Parameter> parameters)
        {
            Service = service;
            Registration = registration;
            Parameters = parameters;
        }

        /// <summary>
        /// Gets the service being resolved.
        /// </summary>
        public Service Service { get; }

        /// <summary>
        /// Gets the component registration for the service being resolved.
        /// </summary>
        public IComponentRegistration Registration { get; }

        /// <summary>
        /// Gets the parameters used when resolving the service.
        /// </summary>
        public IEnumerable<Parameter> Parameters { get; }
    }
}
