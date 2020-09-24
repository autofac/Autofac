// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Autofac.Core.Registration;
using Autofac.Core.Resolving.Pipeline;

namespace Autofac.Core
{
    /// <summary>
    /// Describes a logical component within the container.
    /// </summary>
    public interface IComponentRegistration : IDisposable
    {
        /// <summary>
        /// Gets a unique identifier for this component (shared in all sub-contexts.)
        /// This value also appears in Services.
        /// </summary>
        Guid Id { get; }

        /// <summary>
        /// Gets the activator used to create instances.
        /// </summary>
        IInstanceActivator Activator { get; }

        /// <summary>
        /// Gets the lifetime associated with the component.
        /// </summary>
        IComponentLifetime Lifetime { get; }

        /// <summary>
        /// Gets a value indicating whether the component instances are shared or not.
        /// </summary>
        InstanceSharing Sharing { get; }

        /// <summary>
        /// Gets a value indicating whether the instances of the component should be disposed by the container.
        /// </summary>
        InstanceOwnership Ownership { get; }

        /// <summary>
        /// Gets the services provided by the component.
        /// </summary>
        IEnumerable<Service> Services { get; }

        /// <summary>
        /// Gets additional data associated with the component.
        /// </summary>
        IDictionary<string, object?> Metadata { get; }

        /// <summary>
        /// Gets the component registration upon which this registration is based.
        /// </summary>
        IComponentRegistration Target { get; }

        /// <summary>
        /// Gets the resolve pipeline for the component.
        /// </summary>
        IResolvePipeline ResolvePipeline { get; }

        /// <summary>
        /// Gets the options for the registration.
        /// </summary>
        RegistrationOptions Options { get; }

        /// <summary>
        /// Provides an event that will be invoked just before a pipeline is built, and can be used to add additional middleware
        /// at that point.
        /// </summary>
        public event EventHandler<IResolvePipelineBuilder> PipelineBuilding;

        /// <summary>
        /// Builds the resolve pipeline.
        /// </summary>
        /// <param name="registryServices">The available services.</param>
        void BuildResolvePipeline(IComponentRegistryServices registryServices);
    }
}
