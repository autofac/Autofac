// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac.Core.Resolving.Pipeline;

namespace Autofac.Core.Registration;

/// <summary>
/// Provides middleware from a parent component registry.
/// </summary>
internal class ExternalRegistryServiceMiddlewareSource : IServiceMiddlewareSource
{
    private readonly IComponentRegistry _componentRegistry;
    private readonly bool _isolatedScope;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExternalRegistryServiceMiddlewareSource"/> class.
    /// </summary>
    /// <param name="componentRegistry">The component registry to retrieve middleware from.</param>
    /// <param name="isolatedScope">
    /// Indicates whether queries to the external registry should be wrapped with
    /// <see cref="ScopeIsolatedService"/>, to indicate that the destination
    /// registry should not hold on to type information that does not result in a registration.
    /// </param>
    public ExternalRegistryServiceMiddlewareSource(IComponentRegistry componentRegistry, bool isolatedScope)
    {
        _componentRegistry = componentRegistry ?? throw new ArgumentNullException(nameof(componentRegistry));
        _isolatedScope = isolatedScope;
    }

    /// <inheritdoc/>
    public void ProvideMiddleware(Service service, IComponentRegistryServices availableServices, IResolvePipelineBuilder pipelineBuilder)
    {
        var serviceForLookup = service;

        if (_isolatedScope)
        {
            // If we need to isolate services to a particular scope,
            // we wrap the service in ScopeIsolatedService to tell the parent
            // registry not to hold on to any types that don't result in implementations.
            serviceForLookup = new ScopeIsolatedService(service);
        }

        pipelineBuilder.UseRange(_componentRegistry.ServiceMiddlewareFor(serviceForLookup));
    }
}
