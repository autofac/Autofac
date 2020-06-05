using System.Collections.Generic;
using Autofac.Core;
using Autofac.Core.Pipeline;
using Autofac.Core.Resolving.Pipeline;

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
        public ResolveRequest(Service service, ServiceRegistration implementation, IEnumerable<Parameter> parameters)
            : this(service, implementation, parameters, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResolveRequest"/> class.
        /// </summary>
        /// <param name="service">The service being resolved.</param>
        /// <param name="registration">The component registration for the service.</param>
        /// <param name="parameters">The parameters used when resolving the service.</param>
        /// <param name="decoratorTarget">The target component to be decorated.</param>
        public ResolveRequest(Service service, ServiceRegistration implementation, IEnumerable<Parameter> parameters, IComponentRegistration? decoratorTarget = null)
        {
            Service = service;
            Registration = implementation.Registration;
            ResolvePipeline = implementation.Pipeline;
            Parameters = parameters;
            DecoratorTarget = decoratorTarget;
        }

        /// <summary>
        /// Gets the service being resolved.
        /// </summary>
        public Service Service { get; }

        /// <summary>
        /// Gets the component registration for the service being resolved. This may be null if a service is being supplied without registrations.
        /// </summary>
        public IComponentRegistration Registration { get; }

        public IResolvePipeline ResolvePipeline { get; }

        /// <summary>
        /// Gets the parameters used when resolving the service.
        /// </summary>
        public IEnumerable<Parameter> Parameters { get; }

        /// <summary>
        /// Gets the component registration for the decorator target if configured.
        /// </summary>
        public IComponentRegistration? DecoratorTarget { get; }
    }
}
