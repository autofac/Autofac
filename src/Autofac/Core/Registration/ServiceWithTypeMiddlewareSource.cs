// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Autofac.Core.Resolving.Pipeline;

namespace Autofac.Core.Registration
{
    /// <summary>
    /// Defines a middleware source that adds a specific middleware to all services that expose a specific type; this allows middleware
    /// to be added to regular and keyed instances.
    /// </summary>
    internal class ServiceWithTypeMiddlewareSource : IServiceMiddlewareSource
    {
        private readonly Type _serviceType;
        private readonly IResolveMiddleware _middleware;
        private readonly MiddlewareInsertionMode _insertionMode;

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceWithTypeMiddlewareSource"/> class.
        /// </summary>
        /// <param name="serviceType">The service type.</param>
        /// <param name="middleware">The middleware to add.</param>
        /// <param name="insertionMode">The insertion mode of the middleware.</param>
        public ServiceWithTypeMiddlewareSource(Type serviceType, IResolveMiddleware middleware, MiddlewareInsertionMode insertionMode)
        {
            _serviceType = serviceType ?? throw new ArgumentNullException(nameof(serviceType));
            _middleware = middleware ?? throw new ArgumentNullException(nameof(middleware));
            _insertionMode = insertionMode;
        }

        /// <inheritdoc/>
        public void ProvideMiddleware(Service service, IComponentRegistryServices availableServices, IResolvePipelineBuilder pipelineBuilder)
        {
            if (service is IServiceWithType swt && swt.ServiceType == _serviceType)
            {
                // This is the right type, add the middleware.
                pipelineBuilder.Use(_middleware, _insertionMode);
            }
        }
    }
}
