// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Globalization;
using Autofac.Core;
using Autofac.Core.Registration;

namespace Autofac.Builder;

/// <summary>
/// Helper functions for starting 'startable' components.
/// </summary>
internal static class StartableManager
{
    private static readonly TypedService _startableService = new(typeof(IStartable));

    private static readonly AutoActivateService _autoActivateService = new();

    /// <summary>
    /// Executes the startable and auto-activate components in a context.
    /// </summary>
    /// <param name="properties">The set of properties used during component registration.</param>
    /// <param name="componentContext">
    /// The <see cref="IComponentContext"/> in which startable services should execute.
    /// </param>
    internal static void StartStartableComponents(IDictionary<string, object?> properties, IComponentContext componentContext)
    {
        var componentRegistry = componentContext.ComponentRegistry;
        var startOnActivateSet = false;
        try
        {
            // We track which registrations have already been auto-activated by adding
            // a metadata value. If the value is present, we won't re-activate. This helps
            // in the container update situation.
            var startables = ServiceRegistrationsFor(componentRegistry, _startableService);
            for (var i = 0; i < startables.Count; i++)
            {
                var registration = startables[i];
                if (registration.Metadata.ContainsKey(MetadataKeys.AutoActivated))
                {
                    continue;
                }

                SetStartOnActivate(properties, ref startOnActivateSet);
                try
                {
                    var request = new ResolveRequest(_startableService, registration, Enumerable.Empty<Parameter>());
                    componentContext.ResolveComponent(request);
                }
                finally
                {
                    registration.Metadata[MetadataKeys.AutoActivated] = true;
                }
            }

            var autoActivated = ServiceRegistrationsFor(componentRegistry, _autoActivateService);
            for (var i = 0; i < autoActivated.Count; i++)
            {
                var registration = autoActivated[i];
                if (registration.Metadata.ContainsKey(MetadataKeys.AutoActivated))
                {
                    continue;
                }

                SetStartOnActivate(properties, ref startOnActivateSet);
                try
                {
                    var request = new ResolveRequest(_autoActivateService, registration, Enumerable.Empty<Parameter>());
                    componentContext.ResolveComponent(request);
                }
                catch (DependencyResolutionException ex)
                {
                    throw new DependencyResolutionException(string.Format(CultureInfo.CurrentCulture, ContainerBuilderResources.ErrorAutoActivating, registration), ex);
                }
                finally
                {
                    registration.Metadata[MetadataKeys.AutoActivated] = true;
                }
            }
        }
        finally
        {
            if (startOnActivateSet)
            {
                properties.Remove(MetadataKeys.StartOnActivatePropertyKey);
            }
        }
    }

    /// <summary>
    /// Gets the registrations for a service as a list, which for an Autofac registry is shared when empty.
    /// </summary>
    /// <param name="componentRegistry">The registry.</param>
    /// <param name="service">The service for which registrations are sought.</param>
    /// <returns>The registrations.</returns>
    internal static IReadOnlyList<ServiceRegistration> ServiceRegistrationsFor(IComponentRegistry componentRegistry, Service service)
        => componentRegistry is ComponentRegistry registry
            ? registry.ServiceRegistrationsOrEmptyFor(service)
            : new List<ServiceRegistration>(componentRegistry.ServiceRegistrationsFor(service));

    private static void SetStartOnActivate(IDictionary<string, object?> properties, ref bool startOnActivateSet)
    {
        // The property is only read while components start, so a container or scope with nothing
        // to start doesn't write it into its properties.
        if (!startOnActivateSet)
        {
            properties[MetadataKeys.StartOnActivatePropertyKey] = true;
            startOnActivateSet = true;
        }
    }
}
