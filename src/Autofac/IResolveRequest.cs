using System.Collections.Generic;
using Autofac.Core;
using Autofac.Core.Resolving.Pipeline;

namespace Autofac
{
    /// <summary>
    /// The details of an individual request to resolve a service.
    /// </summary>
    public interface IResolveRequest
    {
        /// <summary>
        /// Gets the service being resolved.
        /// </summary>
        Service Service { get; }

        /// <summary>
        /// Gets the component registration for the service being resolved. This may be null if a service is being supplied without registrations.
        /// </summary>
        IComponentRegistration Registration { get; }

        /// <summary>
        /// Gets the resolve pipeline for the request.
        /// </summary>
        IResolvePipeline ResolvePipeline { get; }

        /// <summary>
        /// Gets the parameters used when resolving the service.
        /// </summary>
        IEnumerable<Parameter> Parameters { get; }

        /// <summary>
        /// Gets the component registration for the decorator target if configured.
        /// </summary>
        IComponentRegistration? DecoratorTarget { get; }
    }
}
