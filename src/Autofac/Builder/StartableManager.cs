// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Autofac.Core;

namespace Autofac.Builder
{
    /// <summary>
    /// Helper functions for starting 'startable' components.
    /// </summary>
    internal static class StartableManager
    {
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
            try
            {
                properties[MetadataKeys.StartOnActivatePropertyKey] = true;

                // We track which registrations have already been auto-activated by adding
                // a metadata value. If the value is present, we won't re-activate. This helps
                // in the container update situation.
                var startableService = new TypedService(typeof(IStartable));
                foreach (var registration in componentRegistry.ServiceRegistrationsFor(startableService).Where(r => !r.Metadata.ContainsKey(MetadataKeys.AutoActivated)))
                {
                    try
                    {
                        var request = new ResolveRequest(startableService, registration, Enumerable.Empty<Parameter>());
                        componentContext.ResolveComponent(request);
                    }
                    finally
                    {
                        registration.Metadata[MetadataKeys.AutoActivated] = true;
                    }
                }

                var autoActivateService = new AutoActivateService();
                foreach (var registration in componentRegistry.ServiceRegistrationsFor(autoActivateService).Where(r => !r.Metadata.ContainsKey(MetadataKeys.AutoActivated)))
                {
                    try
                    {
                        var request = new ResolveRequest(autoActivateService, registration, Enumerable.Empty<Parameter>());
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
                properties.Remove(MetadataKeys.StartOnActivatePropertyKey);
            }
        }
    }
}
