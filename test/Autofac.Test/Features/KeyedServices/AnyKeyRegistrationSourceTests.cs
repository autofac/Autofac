// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac.Builder;
using Autofac.Core;
using Autofac.Core.Resolving.Pipeline;
using Autofac.Features.KeyedServices;

namespace Autofac.Test.Features.KeyedServices;

public class AnyKeyRegistrationSourceTests
{
    private readonly AnyKeyRegistrationSource _source = new();

    [Fact]
    public void RegistrationsFor_NullService()
    {
        Assert.Throws<ArgumentNullException>(
            () => _source.RegistrationsFor(
                service: null!,
                registrationAccessor: _ => Enumerable.Empty<ServiceRegistration>()));
    }

    [Fact]
    public void RegistrationsFor_NullRegistrationAccessor()
    {
        var service = new KeyedService("key", typeof(object));

        Assert.Throws<ArgumentNullException>(
            () => _source.RegistrationsFor(service, null!));
    }

    [Fact]
    public void RegistrationsFor_NonKeyedService()
    {
        var registrations = _source.RegistrationsFor(
            new TypedService(typeof(object)),
            _ => Enumerable.Empty<ServiceRegistration>());

        Assert.Empty(registrations);
    }

    [Fact]
    public void RegistrationsFor_ServiceKeyIsAnyKey()
    {
        var registrations = _source.RegistrationsFor(
            new KeyedService(KeyedService.AnyKey, typeof(object)),
            _ => Enumerable.Empty<ServiceRegistration>());

        Assert.Empty(registrations);
    }

    [Fact]
    public void RegistrationsFor_ServiceTypeIsCollection()
    {
        var registrations = _source.RegistrationsFor(
            new KeyedService("key", typeof(IEnumerable<DummyService>)),
            _ => Enumerable.Empty<ServiceRegistration>());

        Assert.Empty(registrations);
    }

    [Fact]
    public void RegistrationsFor_ServiceHasSpecificRegistration()
    {
        var service = new KeyedService("key", typeof(DummyService));

        using var registration = CreateComponentRegistration<DummyService>();
        var serviceRegistration = CreateServiceRegistration(registration);

        var registrations = _source.RegistrationsFor(
            service,
            requested =>
            {
                if (requested.Equals(service))
                {
                    return new[] { serviceRegistration };
                }

                return Array.Empty<ServiceRegistration>();
            });

        Assert.Empty(registrations);
    }

    [Fact]
    public void RegistrationsFor_NoAnyKeyRegistrations()
    {
        var service = new KeyedService("key", typeof(DummyService));

        var registrations = _source.RegistrationsFor(
            service,
            requested => Enumerable.Empty<ServiceRegistration>());

        Assert.Empty(registrations);
    }

    [Fact]
    public void RegistrationsFor_AnyKeyRegistrationCreatesAdapter()
    {
        var service = new KeyedService("key", typeof(DummyService));
        var anyKeyService = new KeyedService(KeyedService.AnyKey, typeof(DummyService));

        using var registration = CreateComponentRegistration<DummyService>();
        var serviceRegistration = CreateServiceRegistration(registration);

        var registrations = _source.RegistrationsFor(
            service,
            requested =>
            {
                if (requested.Equals(anyKeyService))
                {
                    return new[] { serviceRegistration };
                }

                return Enumerable.Empty<ServiceRegistration>();
            }).ToArray();

        var adapter = Assert.Single(registrations);
        Assert.Contains(service, adapter.Services);
        Assert.True(
            adapter.Metadata.TryGetValue(MetadataKeys.AnyKeyAdapter, out var marker) &&
            marker is true);
        Assert.Same(registration, adapter.Target);
    }

    [Fact]
    public void ResolveKeyed_AdapterNotSharedAcrossScopeRegistries()
    {
        var builder = new ContainerBuilder();
        builder.RegisterType<DummyService>().Keyed<DummyService>(KeyedService.AnyKey);
        using var container = builder.Build();
        using var scope = container.BeginLifetimeScope(b => { });

        container.ResolveKeyed<DummyService>("key");
        scope.ResolveKeyed<DummyService>("key");

        var service = new KeyedService("key", typeof(DummyService));
        Assert.True(container.ComponentRegistry.TryGetServiceRegistration(service, out var fromContainer));
        Assert.True(scope.ComponentRegistry.TryGetServiceRegistration(service, out var fromScope));
        Assert.NotSame(fromContainer.Registration, fromScope.Registration);
    }

    // Issue #1497: disposing the scope that first resolved the key used to dispose
    // the adapter every other scope was handed.
    [Fact]
    public void ResolveKeyed_AdapterUsableAfterAnotherScopeIsDisposed()
    {
        var builder = new ContainerBuilder();
        builder.RegisterType<DummyService>().Keyed<DummyService>(KeyedService.AnyKey);
        using var container = builder.Build();

        using (var first = container.BeginLifetimeScope(b => { }))
        {
            first.ResolveKeyed<DummyService>("key");
        }

        using var second = container.BeginLifetimeScope(b => { });

        Assert.NotNull(second.ResolveKeyed<DummyService>("key"));
    }

    // Issue #1497: a registry that adds middleware as components are registered
    // used to get an adapter whose pipeline another registry had already built.
    [Fact]
    public void ResolveKeyed_ScopeRegistryCanAddPipelineMiddleware()
    {
        var builder = new ContainerBuilder();
        builder.RegisterType<DummyService>().Keyed<DummyService>(KeyedService.AnyKey);
        using var container = builder.Build();
        container.ResolveKeyed<DummyService>("key");

        using var scope = container.BeginLifetimeScope(b =>
            b.ComponentRegistryBuilder.Registered += (sender, e) =>
                e.ComponentRegistration.PipelineBuilding += (sender, pipeline) => { });

        Assert.NotNull(scope.ResolveKeyed<DummyService>("key"));
    }

    [Fact]
    public void ResolveKeyed_SiblingScopesUseTheirOwnAnyKeyRegistration()
    {
        var builder = new ContainerBuilder();
        using var container = builder.Build();
        using var scope1 = container.BeginLifetimeScope(
            b => b.RegisterType<DummyService>().As<IDummyService>().Keyed<IDummyService>(KeyedService.AnyKey));
        using var scope2 = container.BeginLifetimeScope(
            b => b.RegisterType<OtherDummyService>().As<IDummyService>().Keyed<IDummyService>(KeyedService.AnyKey));

        Assert.IsType<DummyService>(scope1.ResolveKeyed<IDummyService>("key"));
        Assert.IsType<OtherDummyService>(scope2.ResolveKeyed<IDummyService>("key"));
    }

    private static IComponentRegistration CreateComponentRegistration<T>()
        where T : notnull
    {
        return RegistrationBuilder
            .ForType<T>()
            .CreateRegistration();
    }

    private static ServiceRegistration CreateServiceRegistration(IComponentRegistration registration)
    {
        return new ServiceRegistration(ServicePipelines.DefaultServicePipeline, registration);
    }

    private interface IDummyService
    {
    }

    private sealed class DummyService : IDummyService
    {
    }

    private sealed class OtherDummyService : IDummyService
    {
    }
}
