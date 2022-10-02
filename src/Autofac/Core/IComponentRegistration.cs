﻿// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac.Core.Registration;
using Autofac.Core.Resolving.Pipeline;

namespace Autofac.Core;

/// <summary>
/// Describes a logical component within the container.
/// </summary>
public interface IComponentRegistration : IDisposable, IAsyncDisposable
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
    /// <exception cref="InvalidOperationException">
    /// Attaching to this event after a component registration
    /// has already been built will throw an exception.
    /// </exception>
    [SuppressMessage("CA1003", "CA1003", Justification = "Breaking API change.")]
    event EventHandler<IResolvePipelineBuilder> PipelineBuilding;

    /// <summary>
    /// Builds the resolve pipeline.
    /// </summary>
    /// <param name="registryServices">The available services.</param>
    void BuildResolvePipeline(IComponentRegistryServices registryServices);

    /// <summary>
    /// Replaces the activator for the registration.
    /// </summary>
    /// <remarks>
    /// You can only invoke this method inside
    /// a <see cref="IComponentRegistryBuilder.Registered"/> handler.
    /// </remarks>
    /// <param name="activator">The new activator.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown if <see cref="ReplaceActivator"/> is invoked
    /// after the registration pipeline has been built, or if this registration targets a different one.
    /// </exception>
    void ReplaceActivator(IInstanceActivator activator);
}
