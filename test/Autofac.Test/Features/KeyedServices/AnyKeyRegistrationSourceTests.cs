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

    private static IComponentRegistration CreateComponentRegistration<T>()
    {
        return RegistrationBuilder
            .ForType<T>()
            .CreateRegistration();
    }

    private static ServiceRegistration CreateServiceRegistration(IComponentRegistration registration)
    {
        return new ServiceRegistration(ServicePipelines.DefaultServicePipeline, registration);
    }

    private sealed class DummyService
    {
    }
}
