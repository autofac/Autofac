// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Globalization;
using Autofac.Core;

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
            var startables = ToList(componentRegistry.ServiceRegistrationsFor(_startableService));
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

            var autoActivated = ToList(componentRegistry.ServiceRegistrationsFor(_autoActivateService));
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
    /// Gets registrations from a registry as a list, without copying the list the registry returns.
    /// </summary>
    /// <param name="registrations">The registrations the registry returned.</param>
    /// <returns>The registrations.</returns>
    internal static IReadOnlyList<ServiceRegistration> ToList(IEnumerable<ServiceRegistration> registrations)
        => registrations as IReadOnlyList<ServiceRegistration> ?? new List<ServiceRegistration>(registrations);

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
