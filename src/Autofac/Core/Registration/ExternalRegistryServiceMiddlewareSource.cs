// This software is part of the Autofac IoC container
// Copyright © 2011 Autofac Contributors
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

using Autofac.Core.Resolving.Pipeline;

namespace Autofac.Core.Registration
{
    /// <summary>
    /// Provides middleware from a parent component registry.
    /// </summary>
    internal class ExternalRegistryServiceMiddlewareSource : IServiceMiddlewareSource
    {
        private readonly IComponentRegistry _componentRegistry;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExternalRegistryServiceMiddlewareSource"/> class.
        /// </summary>
        /// <param name="componentRegistry">The component registry to retrieve middleware from.</param>
        public ExternalRegistryServiceMiddlewareSource(IComponentRegistry componentRegistry)
        {
            _componentRegistry = componentRegistry ?? throw new System.ArgumentNullException(nameof(componentRegistry));
        }

        /// <inheritdoc/>
        public void ProvideMiddleware(Service service, IComponentRegistryServices availableServices, IResolvePipelineBuilder pipelineBuilder)
        {
            pipelineBuilder.UseRange(_componentRegistry.ServiceMiddlewareFor(service));
        }
    }
}
