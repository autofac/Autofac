﻿// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Globalization;
using Autofac.Core.Resolving.Middleware;
using Autofac.Core.Resolving.Pipeline;
using Autofac.Util;

namespace Autofac.Core.Registration;

/// <summary>
/// Describes a logical component within the container.
/// </summary>
[SuppressMessage("Microsoft.ApiDesignGuidelines", "CA2213", Justification = "The target registration, if provided, is disposed elsewhere.")]
public class ComponentRegistration : Disposable, IComponentRegistration
{
    private readonly IComponentRegistration? _target;
    private readonly IResolvePipelineBuilder _lateBuildPipeline;

    private EventHandler<IResolvePipelineBuilder>? _pipelineBuildEvent;
    private IResolvePipeline? _builtComponentPipeline;

    /// <summary>
    /// Defines the options copied from a target registration onto this one.
    /// </summary>
    private const RegistrationOptions OptionsCopiedFromTargetRegistration = RegistrationOptions.Fixed |
                                                                            RegistrationOptions.ExcludeFromCollections |
                                                                            RegistrationOptions.DisableDecoration;

    /// <summary>
    /// Initializes a new instance of the <see cref="ComponentRegistration"/> class.
    /// </summary>
    /// <param name="id">The registration id.</param>
    /// <param name="activator">The component activator.</param>
    /// <param name="lifetime">The lifetime for activated instances.</param>
    /// <param name="sharing">The sharing setting for the registration.</param>
    /// <param name="ownership">The ownership setting for the registration.</param>
    /// <param name="services">The set of services provided by the registration.</param>
    /// <param name="metadata">Any metadata associated with the registration.</param>
    /// <param name="target">The target/inner registration.</param>
    /// <param name="options">Contains options for the registration.</param>
    public ComponentRegistration(
       Guid id,
       IInstanceActivator activator,
       IComponentLifetime lifetime,
       InstanceSharing sharing,
       InstanceOwnership ownership,
       IEnumerable<Service> services,
       IDictionary<string, object?> metadata,
       IComponentRegistration target,
       RegistrationOptions options = RegistrationOptions.None)
        : this(id, activator, lifetime, sharing, ownership, new ResolvePipelineBuilder(PipelineType.Registration), services, metadata, target, options)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ComponentRegistration"/> class.
    /// </summary>
    /// <param name="id">Unique identifier for the component.</param>
    /// <param name="activator">Activator used to activate instances.</param>
    /// <param name="lifetime">Determines how the component will be associated with its lifetime.</param>
    /// <param name="sharing">Whether the component is shared within its lifetime scope.</param>
    /// <param name="ownership">Whether the component instances are disposed at the end of their lifetimes.</param>
    /// <param name="services">Services the component provides.</param>
    /// <param name="metadata">Data associated with the component.</param>
    /// <param name="options">Contains options for the registration.</param>
    public ComponentRegistration(
       Guid id,
       IInstanceActivator activator,
       IComponentLifetime lifetime,
       InstanceSharing sharing,
       InstanceOwnership ownership,
       IEnumerable<Service> services,
       IDictionary<string, object?> metadata,
       RegistrationOptions options = RegistrationOptions.None)
        : this(id, activator, lifetime, sharing, ownership, new ResolvePipelineBuilder(PipelineType.Registration), services, metadata, options)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ComponentRegistration"/> class.
    /// </summary>
    /// <param name="id">Unique identifier for the component.</param>
    /// <param name="activator">Activator used to activate instances.</param>
    /// <param name="lifetime">Determines how the component will be associated with its lifetime.</param>
    /// <param name="sharing">Whether the component is shared within its lifetime scope.</param>
    /// <param name="ownership">Whether the component instances are disposed at the end of their lifetimes.</param>
    /// <param name="pipelineBuilder">The resolve pipeline builder for the registration.</param>
    /// <param name="services">Services the component provides.</param>
    /// <param name="metadata">Data associated with the component.</param>
    /// <param name="options">The additional registration options.</param>
    public ComponentRegistration(
        Guid id,
        IInstanceActivator activator,
        IComponentLifetime lifetime,
        InstanceSharing sharing,
        InstanceOwnership ownership,
        IResolvePipelineBuilder pipelineBuilder,
        IEnumerable<Service> services,
        IDictionary<string, object?> metadata,
        RegistrationOptions options = RegistrationOptions.None)
    {
        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        Id = id;
        Activator = activator ?? throw new ArgumentNullException(nameof(activator));
        Lifetime = lifetime ?? throw new ArgumentNullException(nameof(lifetime));
        Sharing = sharing;
        Ownership = ownership;

        _lateBuildPipeline = pipelineBuilder;

        Services = Enforce.ArgumentElementNotNull(services, nameof(services));
        Metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
        Options = options;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ComponentRegistration"/> class.
    /// </summary>
    /// <param name="id">Unique identifier for the component.</param>
    /// <param name="activator">Activator used to activate instances.</param>
    /// <param name="lifetime">Determines how the component will be associated with its lifetime.</param>
    /// <param name="sharing">Whether the component is shared within its lifetime scope.</param>
    /// <param name="ownership">Whether the component instances are disposed at the end of their lifetimes.</param>
    /// <param name="pipelineBuilder">The resolve pipeline builder for the registration.</param>
    /// <param name="services">Services the component provides.</param>
    /// <param name="metadata">Data associated with the component.</param>
    /// <param name="target">The component registration upon which this registration is based.</param>
    /// <param name="options">Registration options.</param>
    public ComponentRegistration(
        Guid id,
        IInstanceActivator activator,
        IComponentLifetime lifetime,
        InstanceSharing sharing,
        InstanceOwnership ownership,
        IResolvePipelineBuilder pipelineBuilder,
        IEnumerable<Service> services,
        IDictionary<string, object?> metadata,
        IComponentRegistration target,
        RegistrationOptions options = RegistrationOptions.None)
        : this(id, activator, lifetime, sharing, ownership, pipelineBuilder, services, metadata, options)
    {
        _target = target ?? throw new ArgumentNullException(nameof(target));

        // Certain flags carry over from the target.
        Options = options | (_target.Options & OptionsCopiedFromTargetRegistration);
    }

    /// <summary>
    /// Gets the component registration upon which this registration is based.
    /// If this registration was created directly by the user, returns this.
    /// </summary>
    public IComponentRegistration Target => _target ?? this;

    /// <summary>
    /// Gets a unique identifier for this component (shared in all sub-contexts.)
    /// This value also appears in Services.
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// Gets the activator for the registration.
    /// </summary>
    public IInstanceActivator Activator { get; private set; }

    /// <summary>
    /// Gets the lifetime associated with the component.
    /// </summary>
    public IComponentLifetime Lifetime { get; }

    /// <summary>
    /// Gets information about whether the component instances are shared or not.
    /// </summary>
    public InstanceSharing Sharing { get; }

    /// <summary>
    /// Gets information about whether the instances of the component should be disposed by the container.
    /// </summary>
    public InstanceOwnership Ownership { get; }

    /// <summary>
    /// Gets the services provided by the component.
    /// </summary>
    public IEnumerable<Service> Services { get; }

    /// <summary>
    /// Gets additional data associated with the component.
    /// </summary>
    public IDictionary<string, object?> Metadata { get; }

    /// <summary>
    /// Gets the options for the registration.
    /// </summary>
    public RegistrationOptions Options { get; }

    /// <inheritdoc />
    public event EventHandler<IResolvePipelineBuilder>? PipelineBuilding
    {
        add
        {
            if (_builtComponentPipeline is object)
            {
                throw new InvalidOperationException(ComponentRegistrationResources.PipelineAlreadyBuilt);
            }

            _pipelineBuildEvent += value;
        }

        remove
        {
            _pipelineBuildEvent -= value;
        }
    }

    /// <inheritdoc />
    public IResolvePipeline ResolvePipeline
    {
        get => _builtComponentPipeline ?? throw new InvalidOperationException(ComponentRegistrationResources.ComponentPipelineHasNotBeenBuilt);
        protected set => _builtComponentPipeline = value;
    }

    /// <inheritdoc />
    public void BuildResolvePipeline(IComponentRegistryServices registryServices)
    {
        if (_builtComponentPipeline is object)
        {
            // Nothing to do.
            return;
        }

        if (_pipelineBuildEvent is not null)
        {
            _pipelineBuildEvent.Invoke(this, _lateBuildPipeline);

            // Reset the PipelineBuilding event so we don't accidentally retain
            // references we don't need to.
            _pipelineBuildEvent = null;
        }

        ResolvePipeline = BuildResolvePipeline(registryServices, _lateBuildPipeline);
    }

    /// <inheritdoc />
    public void ReplaceActivator(IInstanceActivator activator)
    {
        if (activator is null)
        {
            throw new ArgumentNullException(nameof(activator));
        }

        if (_builtComponentPipeline is not null)
        {
            throw new InvalidOperationException(ComponentRegistrationResources.ReplaceActivatorPipelineAlreadyBuilt);
        }

        if (Target != this)
        {
            throw new InvalidOperationException(ComponentRegistrationResources.ReplaceActivatorTargetingRegistration);
        }

        var limitType = activator.LimitType;
        if (limitType != typeof(object))
        {
            foreach (var ts in Services)
            {
                if (ts is not IServiceWithType asServiceWithType)
                {
                    continue;
                }

                if (!asServiceWithType.ServiceType.IsAssignableFrom(limitType))
                {
                    throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, ComponentRegistrationResources.ReplaceActivatorComponentDoesNotSupportService, limitType, ts));
                }
            }
        }

        Activator = activator;
    }

    /// <summary>
    /// Populates the resolve pipeline with middleware based on the registration, and builds the pipeline.
    /// </summary>
    /// <param name="registryServices">The known set of all services.</param>
    /// <param name="pipelineBuilder">The registration's pipeline builder (with user-added middleware already in it).</param>
    /// <returns>The built pipeline.</returns>
    /// <remarks>
    /// A derived implementation can use this to add additional middleware, or return a completely different pipeline if required.
    /// </remarks>
    protected virtual IResolvePipeline BuildResolvePipeline(IComponentRegistryServices registryServices, IResolvePipelineBuilder pipelineBuilder)
    {
        if (HasStartableService())
        {
            _lateBuildPipeline.Use(StartableMiddleware.Instance);
        }

        if (Ownership == InstanceOwnership.OwnedByLifetimeScope)
        {
            // Add the disposal tracking stage.
            _lateBuildPipeline.Use(DisposalTrackingMiddleware.Instance);
        }

        // Add activator error propagation (want it to run outer-most in the Activator phase).
        _lateBuildPipeline.Use(ActivatorErrorHandlingMiddleware.Instance, MiddlewareInsertionMode.StartOfPhase);

        // Allow the activator to configure the pipeline.
        Activator.ConfigurePipeline(registryServices, _lateBuildPipeline);

        return _lateBuildPipeline.Build();
    }

    private bool HasStartableService()
    {
        foreach (var service in Services)
        {
            if ((service is TypedService typed) && typed.ServiceType == typeof(IStartable))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Describes the component in a human-readable form.
    /// </summary>
    /// <returns>A description of the component.</returns>
    public override string ToString()
    {
        // Activator = {0}, Services = [{1}], Lifetime = {2}, Sharing = {3}, Ownership = {4}, Pipeline = {5}
        return string.Format(
            CultureInfo.CurrentCulture,
            ComponentRegistrationResources.ToStringFormat,
            Activator,
            Services.Select(s => s.Description).JoinWith(", "),
            Lifetime,
            Sharing,
            Ownership,
            _builtComponentPipeline is null ? ComponentRegistrationResources.PipelineNotBuilt : _builtComponentPipeline.ToString());
    }

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            Activator.Dispose();
        }

        base.Dispose(disposing);
    }

    /// <inheritdoc />
    protected override ValueTask DisposeAsync(bool disposing)
    {
        if (disposing)
        {
            // Check the Activator type so we don't have to force Activators to implement IAsyncDisposable if they don't need it.
            if (Activator is IAsyncDisposable asyncDispose)
            {
                var vt = asyncDispose.DisposeAsync();
                if (vt.IsCompletedSuccessfully)
                {
                    return vt;
                }

                static async ValueTask Awaiter(ValueTask vt) => await vt.ConfigureAwait(false);
                return Awaiter(vt);
            }
            else
            {
                Activator.Dispose();
            }
        }

        return default;

        // Do not call the base, otherwise the standard Dispose will fire.
    }
}
