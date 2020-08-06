// This software is part of the Autofac IoC container
// Copyright © 2020 Autofac Contributors
// https://autofac.org
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.

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
