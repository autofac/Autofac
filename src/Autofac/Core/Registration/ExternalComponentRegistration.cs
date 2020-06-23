// This software is part of the Autofac IoC container
// Copyright © 2019 Autofac Contributors
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
    ///  A wrapper component registration created only to distinguish it from other adapted registrations.
    /// </summary>
    internal class ExternalComponentRegistration : ComponentRegistration
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExternalComponentRegistration"/> class.
        /// </summary>
        /// <param name="service">The service to register for.</param>
        /// <param name="target">The target registration ID.</param>
        public ExternalComponentRegistration(Service service, IComponentRegistration target)
            : base(target.Id, new NoOpActivator(target.Activator.LimitType), target.Lifetime, target.Sharing, target.Ownership, new[] { service }, target.Metadata, target)
        {
        }

        /// <inheritdoc/>
        protected override IResolvePipeline BuildResolvePipeline(IComponentRegistryServices registryServices, IResolvePipelineBuilder pipelineBuilder)
        {
            // Just use the external pipeline.
            return Target.ResolvePipeline;
        }

        private class NoOpActivator : IInstanceActivator
        {
            public NoOpActivator(Type limitType)
            {
                LimitType = limitType;
            }

            public Type LimitType { get; }

            public void ConfigurePipeline(IComponentRegistryServices componentRegistryServices, IResolvePipelineBuilder pipelineBuilder)
            {
                // Should never be invoked.
                throw new InvalidOperationException();
            }

            public void Dispose()
            {
                // Do not do anything here.
            }
        }
    }
}
