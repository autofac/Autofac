// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac.Builder;
using Autofac.Core;
using Autofac.Core.Activators.Delegate;
using Autofac.Core.Lifetime;
using Autofac.Core.Registration;
using Autofac.Test.Scenarios.RegistrationSources;

namespace Autofac.Test.Core.Registration;

[SuppressMessage("CA2000", "CA2000", Justification = "The tracker or container disposes of the registrations added to it.")]
public class DefaultRegisteredServicesTrackerTests
{
    [Fact]
    public void Registrations_AddedBeforeAndAfterComplete()
    {
        using var tracker = new DefaultRegisteredServicesTracker();
        var first = Registration();
        var second = Registration();
        var third = Registration();

        tracker.AddRegistration(first, preserveDefaults: false);
        tracker.AddRegistration(second, preserveDefaults: false);
        tracker.Complete();
        tracker.AddRegistration(third, preserveDefaults: false);

        Assert.Equal(new[] { first, second, third }, tracker.Registrations);
    }

    [Fact]
    public void Registrations_AddedWhileEnumerating()
    {
        using var tracker = new DefaultRegisteredServicesTracker();
        var first = Registration();
        var second = Registration();
        tracker.AddRegistration(first, preserveDefaults: false);

        var seen = EnumerateAdding(tracker, second);

        Assert.Equal(new[] { first }, seen);
        Assert.Equal(new[] { first, second }, tracker.Registrations);
    }

    [Fact]
    public void Registrations_AddedAfterCompleteWhileEnumerating()
    {
        // The registry is built this way: the tracker is completed, then its registrations are
        // enumerated, and building their pipelines can make a source add one.
        using var tracker = new DefaultRegisteredServicesTracker();
        var first = Registration();
        var second = Registration();
        tracker.AddRegistration(first, preserveDefaults: false);
        tracker.Complete();

        var seen = EnumerateAdding(tracker, second);

        Assert.Equal(new[] { first }, seen);
        Assert.Equal(new[] { first, second }, tracker.Registrations);
    }

    [Fact]
    public void Registrations_AddedAfterCompleteWhileEnumeratingWithQueuedRegistrations()
    {
        using var tracker = new DefaultRegisteredServicesTracker();
        var first = Registration();
        var second = Registration();
        var third = Registration();
        tracker.AddRegistration(first, preserveDefaults: false);
        tracker.Complete();
        tracker.AddRegistration(second, preserveDefaults: false);

        var seen = EnumerateAdding(tracker, third);

        Assert.Equal(new[] { first, second }, seen);
        Assert.Equal(new[] { first, second, third }, tracker.Registrations);
    }

    [Fact]
    public void Dispose_RegistrationsAddedBeforeAndAfterComplete()
    {
        var tracker = new DefaultRegisteredServicesTracker();
        var (before, after) = AddBeforeAndAfterComplete(tracker);

        tracker.Dispose();

        Assert.True(before.IsDisposed);
        Assert.True(after.IsDisposed);
    }

    [Fact]
    public async Task DisposeAsync_RegistrationsAddedBeforeAndAfterComplete()
    {
        var tracker = new DefaultRegisteredServicesTracker();
        var (before, after) = AddBeforeAndAfterComplete(tracker);

        await tracker.DisposeAsync();

        Assert.True(before.IsDisposed);
        Assert.True(after.IsDisposed);
    }

    [Fact]
    public void RegistrationsFor_ServiceWithNoRegistrations()
    {
        using var container = new ContainerBuilder().Build();
        var registry = container.ComponentRegistry;
        var service = new TypedService(typeof(string));

        var registrations = registry.RegistrationsFor(service);
        var serviceRegistrations = registry.ServiceRegistrationsFor(service);

        Assert.Empty(Assert.IsType<List<IComponentRegistration>>(registrations));
        Assert.NotSame(registrations, registry.RegistrationsFor(service));
        Assert.Empty(Assert.IsType<List<ServiceRegistration>>(serviceRegistrations));
        Assert.NotSame(serviceRegistrations, registry.ServiceRegistrationsFor(service));
    }

    [Fact]
    public void Source_AddedAfterServiceBeganInitializing()
    {
        using var tracker = new DefaultRegisteredServicesTracker();
        tracker.AddRegistrationSource(new ObjectRegistrationSource());
        Assert.False(tracker.IsRegistered(new TypedService(typeof(string))));

        tracker.AddRegistrationSource(new StringRegistrationSource());

        Assert.True(tracker.IsRegistered(new TypedService(typeof(string))));
    }

    [Fact]
    public void Source_ProvidesRegistrationWithListOfServices()
    {
        var first = new TypedService(typeof(IFirst));
        var second = new TypedService(typeof(ISecond));
        var builder = new ContainerBuilder();
        builder.RegisterSource(new ListServicesRegistrationSource(new List<Service> { first, second }));
        using var container = builder.Build();

        // The source provides a new registration each time it is asked, so the second service shares
        // the first one only if the registration is held for it rather than the source asked again.
        var forFirst = Assert.Single(container.ComponentRegistry.RegistrationsFor(first));
        var forSecond = Assert.Single(container.ComponentRegistry.RegistrationsFor(second));
        Assert.Same(forFirst, forSecond);
        Assert.Same(container.Resolve<IFirst>(), container.Resolve<ISecond>());
    }

    private static IComponentRegistration Registration()
        => RegistrationBuilder.ForType<object>().CreateRegistration();

    private static List<IComponentRegistration> EnumerateAdding(DefaultRegisteredServicesTracker tracker, IComponentRegistration added)
    {
        var seen = new List<IComponentRegistration>();
        foreach (var registration in tracker.Registrations)
        {
            seen.Add(registration);
            if (seen.Count == 1)
            {
                tracker.AddRegistration(added, preserveDefaults: false);
            }
        }

        return seen;
    }

    private static (Mocks.MockComponentRegistration Before, Mocks.MockComponentRegistration After) AddBeforeAndAfterComplete(DefaultRegisteredServicesTracker tracker)
    {
        var before = Mocks.GetComponentRegistration();
        var after = Mocks.GetComponentRegistration();
        tracker.AddRegistration(before, preserveDefaults: false);
        tracker.Complete();
        tracker.AddRegistration(after, preserveDefaults: false);
        return (before, after);
    }

    private interface IFirst
    {
    }

    private interface ISecond
    {
    }

    private sealed class Component : IFirst, ISecond
    {
    }

    private sealed class StringRegistrationSource : IRegistrationSource
    {
        public bool IsAdapterForIndividualComponents => false;

        public IEnumerable<IComponentRegistration> RegistrationsFor(Service service, Func<Service, IEnumerable<ServiceRegistration>> registrationAccessor)
        {
            if (service == new TypedService(typeof(string)))
            {
                yield return RegistrationBuilder.ForDelegate((c, p) => "value").CreateRegistration();
            }
        }
    }

    private sealed class ListServicesRegistrationSource : IRegistrationSource
    {
        private readonly List<Service> _services;

        public ListServicesRegistrationSource(List<Service> services)
        {
            _services = services;
        }

        public bool IsAdapterForIndividualComponents => false;

        public IEnumerable<IComponentRegistration> RegistrationsFor(Service service, Func<Service, IEnumerable<ServiceRegistration>> registrationAccessor)
        {
            if (!_services.Contains(service))
            {
                return Array.Empty<IComponentRegistration>();
            }

            // Registrations created by the registration builders hold an array of services; this one doesn't.
            return new[]
            {
                new ComponentRegistration(
                    Guid.NewGuid(),
                    new DelegateActivator(typeof(Component), (c, p) => new Component()),
                    RootScopeLifetime.Instance,
                    InstanceSharing.Shared,
                    InstanceOwnership.OwnedByLifetimeScope,
                    _services,
                    new Dictionary<string, object?>()),
            };
        }
    }
}
