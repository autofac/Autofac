using System.Collections.Generic;
using Autofac.Core;
using Autofac.Core.Resolving.Pipeline;

namespace Autofac
{
    /// <summary>
    /// The details of an individual request to resolve a service.
    /// </summary>
    public abstract class ResolveRequest
    {
        /// <summary>
        /// Gets the service being resolved.
        /// </summary>
        public abstract Service Service { get; }

        /// <summary>
        /// Gets the component registration for the service being resolved. This may be null if a service is being supplied without registrations.
        /// </summary>
        public abstract IComponentRegistration Registration { get; }

        /// <summary>
        /// Gets the resolve pipeline for the request.
        /// </summary>
        public abstract IResolvePipeline ResolvePipeline { get; }

        /// <summary>
        /// Gets the parameters used when resolving the service.
        /// </summary>
        public abstract IEnumerable<Parameter> Parameters { get; }

        /// <summary>
        /// Gets the component registration for the decorator target if configured.
        /// </summary>
        public abstract IComponentRegistration? DecoratorTarget { get; }
    }
}
