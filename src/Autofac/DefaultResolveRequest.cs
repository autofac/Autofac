using System.Collections.Generic;
using Autofac.Core;
using Autofac.Core.Pipeline;
using Autofac.Core.Resolving.Pipeline;

namespace Autofac
{
    /// <inheritdoc />
    internal sealed class DefaultResolveRequest : ResolveRequest
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultResolveRequest"/> class.
        /// </summary>
        /// <param name="service">The service being resolved.</param>
        /// <param name="serviceRegistration">The component registration for the service.</param>
        /// <param name="parameters">The parameters used when resolving the service.</param>
        /// <param name="decoratorTarget">The target component to be decorated.</param>
        public DefaultResolveRequest(Service service, ServiceRegistration serviceRegistration, IEnumerable<Parameter> parameters, IComponentRegistration? decoratorTarget = null)
        {
            Service = service;
            Registration = serviceRegistration.Registration;
            ResolvePipeline = serviceRegistration.Pipeline;
            Parameters = parameters;
            DecoratorTarget = decoratorTarget;
        }

        /// <inheritdoc/>
        public override Service Service { get; }

        /// <inheritdoc/>
        public override IComponentRegistration Registration { get; }

        /// <inheritdoc/>
        public override IResolvePipeline ResolvePipeline { get; }

        /// <inheritdoc/>
        public override IEnumerable<Parameter> Parameters { get; }

        /// <inheritdoc/>
        public override IComponentRegistration? DecoratorTarget { get; }
    }
}
