// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Concurrent;
using System.Reflection;
using Autofac.Core;
using Autofac.Core.Activators.Delegate;
using Autofac.Core.Lifetime;
using Autofac.Core.Registration;

namespace Autofac.Test.Core.Registration;

/// <summary>
/// Covers the interaction between holding a source implementation for a second service and the
/// per-scope source skip performed for scope-isolated services, which is the one path where the
/// service never drains the source the implementation was held against.
/// </summary>
public sealed class DeferredSourceImplementationIsolationTests
{
    private interface IFirstService
    {
    }

    private interface ISecondService
    {
    }

    private class BothServices : IFirstService, ISecondService
    {
    }

    [Fact]
    public void HeldImplementationIsReleasedWhenAPerScopeSourceIsSkippedForAnIsolatedService()
    {
        using var tracker = new DefaultRegisteredServicesTracker();
        using var builder = new ComponentRegistryBuilder(tracker, new Dictionary<string, object?>());
        builder.AddRegistrationSource(new MultiServicePerScopeSource());

        var registry = builder.Build();

        // Querying the first service drains the source, which also provides the second service, so
        // the implementation is held against that source for the second service.
        Assert.NotEmpty(registry.RegistrationsFor(new TypedService(typeof(IFirstService))));
        Assert.Equal(1, HeldCount(tracker));

        // The second service is now queried as isolated. The per-scope source is skipped, so the
        // held implementation is never applied - and must not be left behind either.
        registry.RegistrationsFor(new ScopeIsolatedService(new TypedService(typeof(ISecondService))));

        Assert.Equal(0, HeldCount(tracker));
    }

    [Fact]
    public void HeldImplementationIsAppliedWhenTheServiceIsNotIsolated()
    {
        using var tracker = new DefaultRegisteredServicesTracker();
        using var builder = new ComponentRegistryBuilder(tracker, new Dictionary<string, object?>());
        builder.AddRegistrationSource(new MultiServicePerScopeSource());

        var registry = builder.Build();

        Assert.NotEmpty(registry.RegistrationsFor(new TypedService(typeof(IFirstService))));
        Assert.Equal(1, HeldCount(tracker));

        // The ordinary path drains the source and takes the held implementation rather than querying
        // the source a second time, so both services share the one component.
        var first = Assert.Single(registry.RegistrationsFor(new TypedService(typeof(IFirstService))));
        var second = Assert.Single(registry.RegistrationsFor(new TypedService(typeof(ISecondService))));

        Assert.Same(first, second);
        Assert.Equal(0, HeldCount(tracker));
    }

    private static int HeldCount(DefaultRegisteredServicesTracker tracker)
    {
        var field = typeof(DefaultRegisteredServicesTracker)
            .GetField("_deferredSourceImplementations", BindingFlags.NonPublic | BindingFlags.Instance);

        Assert.NotNull(field);

        var value = field.GetValue(tracker);

        return value is null ? 0 : ((System.Collections.ICollection)value).Count;
    }

    /// <summary>
    /// A per-scope source in the shape a consumer can produce by adding a service to
    /// AnyConcreteTypeNotAlreadyRegisteredSource through its registration configuration: one
    /// component exposing two services, from a source that is skipped for isolated queries.
    /// </summary>
    private sealed class MultiServicePerScopeSource : IRegistrationSource, IPerScopeRegistrationSource
    {
        public bool IsAdapterForIndividualComponents => false;

        public IEnumerable<IComponentRegistration> RegistrationsFor(Service service, Func<Service, IEnumerable<ServiceRegistration>> registrationAccessor)
        {
            if (service is not IServiceWithType swt ||
                (swt.ServiceType != typeof(IFirstService) && swt.ServiceType != typeof(ISecondService)))
            {
                yield break;
            }

#pragma warning disable CA2000 // Activator lifetime is controlled by the registry.
            yield return new ComponentRegistration(
                Guid.NewGuid(),
                new DelegateActivator(typeof(BothServices), (_, _) => new BothServices()),
                new CurrentScopeLifetime(),
                InstanceSharing.None,
                InstanceOwnership.OwnedByLifetimeScope,
                new Service[]
                {
                    new TypedService(typeof(IFirstService)),
                    new TypedService(typeof(ISecondService)),
                },
                new Dictionary<string, object?>());
#pragma warning restore CA2000
        }
    }
}
