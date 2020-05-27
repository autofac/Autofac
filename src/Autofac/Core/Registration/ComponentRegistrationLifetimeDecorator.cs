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

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Autofac.Core.Resolving.Pipeline;
using Autofac.Util;

namespace Autofac.Core.Registration
{
    /// <summary>
    /// Wraps a component registration, switching its lifetime.
    /// </summary>
    [SuppressMessage("Microsoft.ApiDesignGuidelines", "CA2215", Justification = "The creator of the inner registration is responsible for disposal.")]
    internal class ComponentRegistrationLifetimeDecorator : Disposable, IComponentRegistration
    {
        private readonly IComponentRegistration _inner;

        public ComponentRegistrationLifetimeDecorator(IComponentRegistration inner, IComponentLifetime lifetime)
        {
            _inner = inner ?? throw new ArgumentNullException(nameof(inner));
            Lifetime = lifetime ?? throw new ArgumentNullException(nameof(lifetime));
        }

        public Guid Id => _inner.Id;

        public IInstanceActivator Activator => _inner.Activator;

        public IComponentLifetime Lifetime { get; }

        public InstanceSharing Sharing => _inner.Sharing;

        public InstanceOwnership Ownership => _inner.Ownership;

        public IEnumerable<Service> Services => _inner.Services;

        public IDictionary<string, object?> Metadata => _inner.Metadata;

        public IComponentRegistration Target => _inner.IsAdapting() ? _inner.Target : this;

        public bool IsAdapterForIndividualComponent => _inner.IsAdapterForIndividualComponent;

        public IResolvePipeline ResolvePipeline => _inner.ResolvePipeline;

        public event EventHandler<IResolvePipelineBuilder> PipelineBuilding
        {
            add => _inner.PipelineBuilding += value;
            remove => _inner.PipelineBuilding -= value;
        }

        public void BuildResolvePipeline(IComponentRegistryServices registryServices)
        {
            _inner.BuildResolvePipeline(registryServices);
        }

        protected override void Dispose(bool disposing)
        {
            _inner.Dispose();
        }
    }
}
