// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

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

        /// <summary>
        /// Initializes a new instance of the <see cref="ComponentRegistrationLifetimeDecorator"/> class.
        /// </summary>
        /// <param name="inner">The inner registration.</param>
        /// <param name="lifetime">The enforced lifetime.</param>
        public ComponentRegistrationLifetimeDecorator(IComponentRegistration inner, IComponentLifetime lifetime)
        {
            _inner = inner ?? throw new ArgumentNullException(nameof(inner));
            Lifetime = lifetime ?? throw new ArgumentNullException(nameof(lifetime));
        }

        /// <inheritdoc/>
        public Guid Id => _inner.Id;

        /// <inheritdoc/>
        public IInstanceActivator Activator => _inner.Activator;

        /// <inheritdoc/>
        public IComponentLifetime Lifetime { get; }

        /// <inheritdoc/>
        public InstanceSharing Sharing => _inner.Sharing;

        /// <inheritdoc/>
        public InstanceOwnership Ownership => _inner.Ownership;

        /// <inheritdoc/>
        public IEnumerable<Service> Services => _inner.Services;

        /// <inheritdoc/>
        public IDictionary<string, object?> Metadata => _inner.Metadata;

        /// <inheritdoc/>
        public IComponentRegistration Target => _inner.IsAdapting() ? _inner.Target : this;

        /// <inheritdoc/>
        public IResolvePipeline ResolvePipeline => _inner.ResolvePipeline;

        /// <inheritdoc/>
        public RegistrationOptions Options => _inner.Options;

        /// <inheritdoc/>
        public event EventHandler<IResolvePipelineBuilder> PipelineBuilding
        {
            add => _inner.PipelineBuilding += value;
            remove => _inner.PipelineBuilding -= value;
        }

        /// <inheritdoc/>
        public void BuildResolvePipeline(IComponentRegistryServices registryServices)
        {
            _inner.BuildResolvePipeline(registryServices);
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            _inner.Dispose();
        }
    }
}
