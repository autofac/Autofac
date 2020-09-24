// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Autofac.Core;
using Autofac.Core.Resolving.Pipeline;

namespace Autofac.Builder
{
    /// <summary>
    /// Data structure used to construct registrations.
    /// </summary>
    /// <typeparam name="TLimit">The most specific type to which instances of the registration
    /// can be cast.</typeparam>
    /// <typeparam name="TActivatorData">Activator builder type.</typeparam>
    /// <typeparam name="TRegistrationStyle">Registration style type.</typeparam>
    public interface IRegistrationBuilder<out TLimit, out TActivatorData, out TRegistrationStyle>
    {
        /// <summary>
        /// Gets the registration data.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        RegistrationData RegistrationData { get; }

        /// <summary>
        /// Gets the activator data.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        TActivatorData ActivatorData { get; }

        /// <summary>
        /// Gets the registration style.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        TRegistrationStyle RegistrationStyle { get; }

        /// <summary>
        /// Gets the resolve pipeline for this registration.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        IResolvePipelineBuilder ResolvePipeline { get; }

        /// <summary>
        /// Configure the component so that instances are never disposed by the container.
        /// </summary>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> ExternallyOwned();

        /// <summary>
        /// Configure the component so that instances that support IDisposable are
        /// disposed by the container (default).
        /// </summary>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> OwnedByLifetimeScope();

        /// <summary>
        /// Configure the component so that every dependent component or call to Resolve()
        /// gets a new, unique instance (default).
        /// </summary>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> InstancePerDependency();

        /// <summary>
        /// Configure the component so that every dependent component or call to Resolve()
        /// gets the same, shared instance.
        /// </summary>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> SingleInstance();

        /// <summary>
        /// Configure the component so that every dependent component or call to Resolve()
        /// within a single ILifetimeScope gets the same, shared instance. Dependent components in
        /// different lifetime scopes will get different instances.
        /// </summary>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> InstancePerLifetimeScope();

        /// <summary>
        /// Configure the component so that every dependent component or call to Resolve() within
        /// a ILifetimeScope tagged with any of the provided tags value gets the same, shared instance.
        /// Dependent components in lifetime scopes that are children of the tagged scope will
        /// share the parent's instance. If no appropriately tagged scope can be found in the
        /// hierarchy an <see cref="DependencyResolutionException"/> is thrown.
        /// </summary>
        /// <param name="lifetimeScopeTag">Tag applied to matching lifetime scopes.</param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> InstancePerMatchingLifetimeScope(params object[] lifetimeScopeTag);

        /// <summary>
        /// Configure the component so that every dependent component or call to Resolve()
        /// within a ILifetimeScope created by an owned instance gets the same, shared instance.
        /// Dependent components in lifetime scopes that are children of the owned instance scope will
        /// share the parent's instance. If no appropriate owned instance scope can be found in the
        /// hierarchy an <see cref="DependencyResolutionException"/> is thrown.
        /// </summary>
        /// <typeparam name="TService">Service type.</typeparam>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> InstancePerOwned<TService>();

        /// <summary>
        /// Configure the component so that every dependent component or call to Resolve()
        /// within a ILifetimeScope created by an owned instance gets the same, shared instance.
        /// Dependent components in lifetime scopes that are children of the owned instance scope will
        /// share the parent's instance. If no appropriate owned instance scope can be found in the
        /// hierarchy an <see cref="DependencyResolutionException"/> is thrown.
        /// </summary>
        /// <param name="serviceType">Service type.</param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> InstancePerOwned(Type serviceType);

        /// <summary>
        /// Configure the component so that every dependent component or call to Resolve()
        /// within a ILifetimeScope created by an owned instance gets the same, shared instance.
        /// Dependent components in lifetime scopes that are children of the owned instance scope will
        /// share the parent's instance. If no appropriate owned instance scope can be found in the
        /// hierarchy an <see cref="DependencyResolutionException"/> is thrown.
        /// </summary>
        /// <typeparam name="TService">The service type provided by the component.</typeparam>
        /// <param name="serviceKey">Key to associate with the component.</param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> InstancePerOwned<TService>(object serviceKey);

        /// <summary>
        /// Configure the component so that every dependent component or call to Resolve()
        /// within a ILifetimeScope created by an owned instance gets the same, shared instance.
        /// Dependent components in lifetime scopes that are children of the owned instance scope will
        /// share the parent's instance. If no appropriate owned instance scope can be found in the
        /// hierarchy an <see cref="DependencyResolutionException"/> is thrown.
        /// </summary>
        /// <typeparam name="TService">The service type provided by the component.</typeparam>
        /// <param name="serviceKeys">Keys to associate with the component.</param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> InstancePerOwned<TService>(params object[] serviceKeys);

        /// <summary>
        /// Configure the component so that every dependent component or call to Resolve()
        /// within a ILifetimeScope created by an owned instance gets the same, shared instance.
        /// Dependent components in lifetime scopes that are children of the owned instance scope will
        /// share the parent's instance. If no appropriate owned instance scope can be found in the
        /// hierarchy an <see cref="DependencyResolutionException"/> is thrown.
        /// </summary>
        /// <param name="serviceKey">Key to associate with the component.</param>
        /// <param name="serviceType">The service type provided by the component.</param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> InstancePerOwned(object serviceKey, Type serviceType);

        /// <summary>
        /// Configure the services that the component will provide. The generic parameter(s) to As()
        /// will be exposed as TypedService instances.
        /// </summary>
        /// <typeparam name="TService">Service type.</typeparam>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> As<TService>()
            where TService : notnull;

        /// <summary>
        /// Configure the services that the component will provide. The generic parameter(s) to As()
        /// will be exposed as TypedService instances.
        /// </summary>
        /// <typeparam name="TService1">Service type.</typeparam>
        /// <typeparam name="TService2">Service type.</typeparam>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> As<TService1, TService2>()
            where TService1 : notnull
            where TService2 : notnull;

        /// <summary>
        /// Configure the services that the component will provide. The generic parameter(s) to As()
        /// will be exposed as TypedService instances.
        /// </summary>
        /// <typeparam name="TService1">Service type.</typeparam>
        /// <typeparam name="TService2">Service type.</typeparam>
        /// <typeparam name="TService3">Service type.</typeparam>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> As<TService1, TService2, TService3>()
            where TService1 : notnull
            where TService2 : notnull
            where TService3 : notnull;

        /// <summary>
        /// Configure the services that the component will provide.
        /// </summary>
        /// <param name="services">Service types to expose.</param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> As(params Type[] services);

        /// <summary>
        /// Configure the services that the component will provide.
        /// </summary>
        /// <param name="services">Services to expose.</param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> As(params Service[] services);

        /// <summary>
        /// Provide a textual name that can be used to retrieve the component.
        /// </summary>
        /// <param name="serviceName">Named service to associate with the component.</param>
        /// <param name="serviceType">The service type provided by the component.</param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> Named(string serviceName, Type serviceType);

        /// <summary>
        /// Provide a textual name that can be used to retrieve the component.
        /// </summary>
        /// <param name="serviceName">Named service to associate with the component.</param>
        /// <typeparam name="TService">The service type provided by the component.</typeparam>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> Named<TService>(string serviceName)
            where TService : notnull;

        /// <summary>
        /// Provide a key that can be used to retrieve the component.
        /// </summary>
        /// <param name="serviceKey">Key to associate with the component.</param>
        /// <param name="serviceType">The service type provided by the component.</param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> Keyed(object serviceKey, Type serviceType);

        /// <summary>
        /// Provide a key that can be used to retrieve the component.
        /// </summary>
        /// <param name="serviceKey">Key to associate with the component.</param>
        /// <typeparam name="TService">The service type provided by the component.</typeparam>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> Keyed<TService>(object serviceKey)
            where TService : notnull;

        /// <summary>
        /// Add a handler for the Preparing event. This event allows manipulating of the parameters
        /// that will be provided to the component.
        /// </summary>
        /// <param name="handler">The event handler.</param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> OnPreparing(Action<PreparingEventArgs> handler);

        /// <summary>
        /// Add an async handler for the Preparing event. This event allows manipulating of the parameters
        /// that will be provided to the component.
        /// </summary>
        /// <param name="handler">An event handler; the resolve process will not continue until the returned task completes.</param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> OnPreparing(Func<PreparingEventArgs, ValueTask> handler);

        /// <summary>
        /// Add a handler for the Activating event.
        /// </summary>
        /// <param name="handler">The event handler.</param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> OnActivating(Action<IActivatingEventArgs<TLimit>> handler);

        /// <summary>
        /// Add an async handler for the Activating event.
        /// </summary>
        /// <param name="handler">An event handler; the resolve process will not continue until the returned task completes.</param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> OnActivating(Func<IActivatingEventArgs<TLimit>, ValueTask> handler);

        /// <summary>
        /// Add a handler for the Activated event.
        /// </summary>
        /// <param name="handler">The event handler.</param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> OnActivated(Action<IActivatedEventArgs<TLimit>> handler);

        /// <summary>
        /// Add a handler for the Activated event.
        /// </summary>
        /// <param name="handler">An event handler; the resolve process will not continue until the returned task completes.</param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> OnActivated(Func<IActivatedEventArgs<TLimit>, ValueTask> handler);

        /// <summary>
        /// Configure the component so that any properties whose types are registered in the
        /// container and follow specific criteria will be wired to instances of the appropriate service.
        /// </summary>
        /// <param name="propertySelector">Selector to determine which properties should be injected.</param>
        /// <param name="allowCircularDependencies">Determine if circular dependencies should be allowed or not.</param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> PropertiesAutowired(IPropertySelector propertySelector, bool allowCircularDependencies = false);

        /// <summary>
        /// Associates data with the component.
        /// </summary>
        /// <param name="key">Key by which the data can be located.</param>
        /// <param name="value">The data value.</param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> WithMetadata(string key, object? value);

        /// <summary>
        /// Associates data with the component.
        /// </summary>
        /// <param name="properties">The extended properties to associate with the component.</param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> WithMetadata(IEnumerable<KeyValuePair<string, object?>> properties);

        /// <summary>
        /// Associates data with the component.
        /// </summary>
        /// <typeparam name="TMetadata">A type with properties whose names correspond to the
        /// property names to configure.</typeparam>
        /// <param name="configurationAction">
        /// The action used to configure the metadata.
        /// </param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> WithMetadata<TMetadata>(Action<MetadataConfiguration<TMetadata>> configurationAction);

        /// <summary>
        /// Provides access to the registration's pipeline builder, allowing custom middleware to be added.
        /// </summary>
        /// <param name="configurationAction">An action that can configure the registration's pipeline.</param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> ConfigurePipeline(Action<IResolvePipelineBuilder> configurationAction);
    }
}
