using System;
using System.Globalization;
using System.Linq;
using Autofac.Core;

namespace Autofac.Builder
{
    internal static class StartableManager
    {
        /// <summary>
        /// Executes the startable and auto-activate components in a context.
        /// </summary>
        /// <param name="componentContext">
        /// The <see cref="IComponentContext"/> in which startables should execute.
        /// </param>
        internal static void StartStartableComponents(IComponentContext componentContext)
        {
            var componentRegistry = componentContext.ComponentRegistry;
            try
            {
                componentRegistry.Properties[MetadataKeys.StartOnActivatePropertyKey] = true;

                // We track which registrations have already been auto-activated by adding
                // a metadata value. If the value is present, we won't re-activate. This helps
                // in the container update situation.
                var startableService = new TypedService(typeof(IStartable));
                foreach (var registration in componentRegistry.RegistrationsFor(startableService).Where(r => !r.Metadata.ContainsKey(MetadataKeys.AutoActivated)))
                {
                    try
                    {
                        componentContext.ResolveComponent(startableService, registration, Enumerable.Empty<Parameter>());
                    }
                    finally
                    {
                        registration.Metadata[MetadataKeys.AutoActivated] = true;
                    }
                }

                var autoActivateService = new AutoActivateService();
                foreach (var registration in componentRegistry.RegistrationsFor(autoActivateService).Where(r => !r.Metadata.ContainsKey(MetadataKeys.AutoActivated)))
                {
                    try
                    {
                        componentContext.ResolveComponent(autoActivateService, registration, Enumerable.Empty<Parameter>());
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
                componentRegistry.Properties.Remove(MetadataKeys.StartOnActivatePropertyKey);
            }
        }
    }
}
