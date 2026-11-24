// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac.Core;
using Autofac.Core.Activators.Delegate;
using Autofac.Core.Lifetime;
using Autofac.Core.Registration;
using Autofac.Features.Decorators;
using Autofac.Test.Scenarios.RegistrationSources;
using Autofac.Test.Util;

namespace Autofac.Test.Core.Lifetime;

public class LifetimeScopeTests
{
    private interface IDecoratedService
    {
    }

    [Fact]
    public void QualifiedSharedInstance_TryGetBeforeCreation()
    {
        // #1508: the qualified store is created on first use, so a lookup before then must find nothing.
        using var container = new ContainerBuilder().Build();
        using var scope = container.BeginLifetimeScope();
        var sharingScope = (ISharingLifetimeScope)scope;
        var primaryId = Guid.NewGuid();
        var qualifyingId = Guid.NewGuid();
        var instance = new object();

        // No qualified instance has been created in this scope yet.
        Assert.False(sharingScope.TryGetSharedInstance(primaryId, qualifyingId, out _));

        Assert.Same(instance, sharingScope.CreateSharedInstance(primaryId, qualifyingId, () => instance));
        Assert.Same(instance, sharingScope.CreateSharedInstance(primaryId, qualifyingId, () => new object()));

        Assert.True(sharingScope.TryGetSharedInstance(primaryId, qualifyingId, out var found));
        Assert.Same(instance, found);
        Assert.False(sharingScope.TryGetSharedInstance(primaryId, out _));
    }

    [Fact]
    public void SharedInstances_BeyondInitialCapacity()
    {
        // #1508: the stores now have one lock and the default capacity, so they must keep every instance as they grow.
        using var container = new ContainerBuilder().Build();
        using var scope = container.BeginLifetimeScope();
        var sharingScope = (ISharingLifetimeScope)scope;
        var ids = Enumerable.Range(0, 100).Select(_ => Guid.NewGuid()).ToArray();
        var qualifyingId = Guid.NewGuid();

        foreach (var id in ids)
        {
            sharingScope.CreateSharedInstance(id, () => id);
            sharingScope.CreateSharedInstance(id, qualifyingId, () => id);
        }

        foreach (var id in ids)
        {
            Assert.True(sharingScope.TryGetSharedInstance(id, out var instance));
            Assert.Equal(id, instance);
            Assert.True(sharingScope.TryGetSharedInstance(id, qualifyingId, out var qualifiedInstance));
            Assert.Equal(id, qualifiedInstance);
        }
    }

    [Fact]
    public async Task DecoratedSharedInstance_ConcurrentFirstResolve()
    {
        // #1508: the qualified store is created on first use under _synchRoot, so threads racing to create it must
        // still share one instance.
        const int ThreadCount = 8;
        var created = 0;
        var builder = new ContainerBuilder();
        builder.Register(_ =>
        {
            Interlocked.Increment(ref created);
            return new DecoratedServiceImplementation();
        }).As<IDecoratedService>().InstancePerLifetimeScope();
        builder.RegisterDecorator<DecoratedServiceDecorator, IDecoratedService>();
        using var container = builder.Build();

        for (var iteration = 0; iteration < 200; iteration++)
        {
            created = 0;
            using var scope = container.BeginLifetimeScope();
            using var barrier = new Barrier(ThreadCount);

            // Each resolve blocks on the barrier, so each gets its own thread rather than waiting for the thread pool,
            // whose minimum is the processor count, to add threads.
            var resolves = Enumerable.Range(0, ThreadCount).Select(_ => Task.Factory.StartNew(
                () =>
                {
                    barrier.SignalAndWait();
                    return scope.Resolve<IDecoratedService>();
                },
                CancellationToken.None,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default)).ToArray();

            var results = await Task.WhenAll(resolves);

            Assert.Equal(1, created);
            Assert.IsType<DecoratedServiceDecorator>(results[0]);
            Assert.All(results, result => Assert.Same(results[0], result));
        }
    }

    [Fact]
    public void QualifiedSharedInstance_SelfConstructing()
    {
        // #1508: the self-constructing check relies on the nested call using the same qualified store, which is now
        // created on first use.
        using var container = new ContainerBuilder().Build();
        using var scope = container.BeginLifetimeScope();
        var sharingScope = (ISharingLifetimeScope)scope;
        var primaryId = Guid.NewGuid();
        var qualifyingId = Guid.NewGuid();

        Assert.Throws<DependencyResolutionException>(() =>
            sharingScope.CreateSharedInstance(primaryId, qualifyingId, () => sharingScope.CreateSharedInstance(primaryId, qualifyingId, () => new object())));
    }

    [Fact]
    public void SharedInstance_SelfConstructing()
    {
        // #1508: the same check for the unqualified store, which now has one lock.
        using var container = new ContainerBuilder().Build();
        using var scope = container.BeginLifetimeScope();
        var sharingScope = (ISharingLifetimeScope)scope;
        var id = Guid.NewGuid();

        Assert.Throws<DependencyResolutionException>(() =>
            sharingScope.CreateSharedInstance(id, () => sharingScope.CreateSharedInstance(id, () => new object())));
    }

    [Fact]
    public void AdaptersInNestedScopeOverrideAdaptersInParent()
    {
        const string ParentInstance = "p";
        const string ChildInstance = "c";

        var builder = new ContainerBuilder();
        builder.ComponentRegistryBuilder.AddRegistrationSource(new ObjectRegistrationSource(ParentInstance));

        var parent = builder.Build();
        var child = parent.BeginLifetimeScope(lifetimeScopeBuilder =>
                lifetimeScopeBuilder.RegisterSource(new ObjectRegistrationSource(ChildInstance)));
        var fromChild = child.Resolve<object>();
        Assert.Same(ChildInstance, fromChild);
    }

    [Fact]
    public void NestedComponentRegistryIsProperlyDisposed()
    {
        var builder = new ContainerBuilder();
        var container = builder.Build();
        using var nestedRegistration = Mocks.GetComponentRegistration();
        var child = container.BeginLifetimeScope(b => b.RegisterComponent(nestedRegistration));
        child.Dispose();
        Assert.True(nestedRegistration.IsDisposed);
    }

    [Fact]
    public void NestedComponentRegistryIsProperlyDisposedEvenWhenRegistryUpdatedLater()
    {
        var builder = new ContainerBuilder();
        var container = builder.Build();
        using var nestedRegistration = Mocks.GetComponentRegistration();
        var child = container.BeginLifetimeScope(x => x.ComponentRegistryBuilder.Register(nestedRegistration));
        child.Dispose();
        Assert.True(nestedRegistration.IsDisposed);
    }

    [Fact]
    public void NestedLifetimeScopesMaintainServiceLimitTypes()
    {
        // Issue #365
        var cb = new ContainerBuilder();
        cb.RegisterType<Person>();
        var container = cb.Build();
        var service = new TypedService(typeof(Person));
        using (var unconfigured = container.BeginLifetimeScope())
        {
            Assert.True(unconfigured.ComponentRegistry.TryGetRegistration(service, out var reg), "The registration should have been found in the unconfigured scope.");
            Assert.Equal(typeof(Person), reg.Activator.LimitType);
        }

        using (var configured = container.BeginLifetimeScope(b => { }))
        {
            Assert.True(configured.ComponentRegistry.TryGetRegistration(service, out var reg), "The registration should have been found in the configured scope.");
            Assert.Equal(typeof(Person), reg.Activator.LimitType);
        }
    }

    [Fact]
    public async Task AsyncDisposeLifetimeScopeDisposesRegistrationsAsync()
    {
        var cb = new ContainerBuilder();

        cb.RegisterType<DisposeTracker>().InstancePerLifetimeScope().AsSelf();
        cb.RegisterType<AsyncDisposeTracker>().InstancePerLifetimeScope().AsSelf();
        cb.RegisterType<AsyncOnlyDisposeTracker>().InstancePerLifetimeScope().AsSelf();

        var container = cb.Build();

        DisposeTracker tracker;
        AsyncDisposeTracker asyncTracker;
        AsyncOnlyDisposeTracker asyncOnlyTracker;

        await using (var scope = container.BeginLifetimeScope())
        {
            tracker = scope.Resolve<DisposeTracker>();
            asyncTracker = scope.Resolve<AsyncDisposeTracker>();
            asyncOnlyTracker = scope.Resolve<AsyncOnlyDisposeTracker>();

            Assert.False(tracker.IsDisposed);
            Assert.False(asyncTracker.IsSyncDisposed);
            Assert.False(asyncTracker.IsAsyncDisposed);
            Assert.False(asyncOnlyTracker.IsAsyncDisposed);
        }

        Assert.True(tracker.IsDisposed);
        Assert.True(asyncTracker.IsAsyncDisposed);
        Assert.True(asyncOnlyTracker.IsAsyncDisposed);
        Assert.False(asyncTracker.IsSyncDisposed);
    }

    [Fact]
    public void DisposeLifetimeScopeDisposesRegistrationsThatAreAsyncAndSyncDispose()
    {
        var cb = new ContainerBuilder();

        cb.RegisterType<DisposeTracker>().InstancePerLifetimeScope().AsSelf();
        cb.RegisterType<AsyncDisposeTracker>().InstancePerLifetimeScope().AsSelf();

        var container = cb.Build();

        DisposeTracker tracker;
        AsyncDisposeTracker asyncTracker;

        using (var scope = container.BeginLifetimeScope())
        {
            tracker = scope.Resolve<DisposeTracker>();
            asyncTracker = scope.Resolve<AsyncDisposeTracker>();

            Assert.False(tracker.IsDisposed);
            Assert.False(asyncTracker.IsSyncDisposed);
            Assert.False(asyncTracker.IsAsyncDisposed);
        }

        Assert.True(tracker.IsDisposed);
        Assert.False(asyncTracker.IsAsyncDisposed);
        Assert.True(asyncTracker.IsSyncDisposed);
    }

    [Fact]
    public void RegistrationSourcesAreIsolatedToLifetimeScopes()
    {
        // Issue #1071 - Custom registration source implementations need to account for DecoratorService.
        var container = new ContainerBuilder().Build();

        var scope1RegisteredInstance = new Test();
        var scope2RegisteredInstance = new Test();

        using var scope1 = container.BeginLifetimeScope(builder => builder.RegisterSource(new SimplifiedRegistrationSource(scope1RegisteredInstance)));
        var scope1ResolvedInstance = scope1.Resolve<ITest>();
        Assert.Same(scope1RegisteredInstance, scope1ResolvedInstance);

        using var scope2 = scope1.BeginLifetimeScope(builder => builder.RegisterSource(new SimplifiedRegistrationSource(scope2RegisteredInstance)));
        var scope2ResolvedInstance = scope2.Resolve<ITest>();
        Assert.Same(scope2RegisteredInstance, scope2ResolvedInstance);
    }

    [Fact]
    public void ResolvingCachedSharedInstancesDoesNotAllocate()
    {
        var builder = new ContainerBuilder();
        builder.RegisterType<Test>().As<ITest>().SingleInstance();
        builder.RegisterType<Person>().InstancePerLifetimeScope();
        using var container = builder.Build();
        using var scope = container.BeginLifetimeScope();

        // Warm up so the instances are cached and first-run allocations happen before we measure.
        var singleton = container.Resolve<ITest>();
        var scoped = scope.Resolve<Person>();
        Assert.Same(singleton, scope.Resolve<ITest>());

        var before = GC.GetAllocatedBytesForCurrentThread();
        for (var i = 0; i < 100; i++)
        {
            container.Resolve<ITest>();
            scope.Resolve<ITest>();
            scope.Resolve<Person>();
        }

        var allocated = GC.GetAllocatedBytesForCurrentThread() - before;

        Assert.Equal(0, allocated);
        Assert.Same(scoped, scope.Resolve<Person>());
    }

    [Fact]
    public void ResolvingCachedInstancesFromScopesNestedBelowTheirScopeDoesNotAllocate()
    {
        var builder = new ContainerBuilder();
        builder.RegisterType<Test>().As<ITest>().InstancePerMatchingLifetimeScope("request");
        using var container = builder.Build();
        using var request = container.BeginLifetimeScope("request");
        using var nested = request.BeginLifetimeScope();
        using var configured = container.BeginLifetimeScope(b => b.RegisterType<Person>().SingleInstance());
        using var configuredNested = configured.BeginLifetimeScope();

        // Warm up so the instances are cached and first-run allocations happen before we measure.
        var perRequest = request.Resolve<ITest>();
        var configuredSingleton = configured.Resolve<Person>();
        Assert.Same(perRequest, nested.Resolve<ITest>());
        Assert.Same(configuredSingleton, configuredNested.Resolve<Person>());

        var before = GC.GetAllocatedBytesForCurrentThread();
        for (var i = 0; i < 100; i++)
        {
            nested.Resolve<ITest>();
            configuredNested.Resolve<Person>();
        }

        var allocated = GC.GetAllocatedBytesForCurrentThread() - before;

        Assert.Equal(0, allocated);

        using var otherRequest = container.BeginLifetimeScope("request");
        using var otherNested = otherRequest.BeginLifetimeScope();
        Assert.NotSame(perRequest, otherNested.Resolve<ITest>());

        using var unmatched = container.BeginLifetimeScope();
        Assert.Throws<DependencyResolutionException>(() => unmatched.Resolve<ITest>());
    }

    [Fact]
    public void ResolveEventsAreRaisedForCachedSharedInstances()
    {
        var builder = new ContainerBuilder();
        builder.RegisterType<Test>().As<ITest>().SingleInstance();
        using var container = builder.Build();
        var singleton = container.Resolve<ITest>();
        using var scope = container.BeginLifetimeScope();

        var requestedServices = new List<Service>();
        scope.ResolveOperationBeginning += (sender, args) =>
            args.ResolveOperation.ResolveRequestBeginning += (operation, requestArgs) => requestedServices.Add(requestArgs.RequestContext.Service);

        Assert.Same(singleton, scope.Resolve<ITest>());
        Assert.Equal(new TypedService(typeof(ITest)), Assert.Single(requestedServices));
    }

    private interface ITest
    {
    }

    private class Test : ITest
    {
    }

    private class SimplifiedRegistrationSource : IRegistrationSource
    {
        private readonly ITest _instance;

        public bool IsAdapterForIndividualComponents
        {
            get;
        }

        public SimplifiedRegistrationSource(ITest instance) => _instance = instance;

        public IEnumerable<IComponentRegistration> RegistrationsFor(Service service, Func<Service, IEnumerable<ServiceRegistration>> registrationAccessor)
        {
            // Important that DecoratorService is not included here.
            if (service is not IServiceWithType serviceWithType || service is DecoratorService)
            {
                yield break;
            }

            if (IsTestType(serviceWithType.ServiceType))
            {
                yield return CreateRegistration(service, serviceWithType.ServiceType, (c, p) => _instance);
            }
        }

        private static bool IsTestType(Type serviceType) => typeof(ITest).IsAssignableFrom(serviceType);

        [SuppressMessage("CA2000", "CA2000", Justification = "The registration disposes of the activator automatically.")]
        private static ComponentRegistration CreateRegistration(Service service, Type serviceType, Func<IComponentContext, IEnumerable<Parameter>, object> factory)
            => new(
                Guid.NewGuid(),
                new DelegateActivator(serviceType, factory),
                CurrentScopeLifetime.Instance,
                InstanceSharing.None,
                InstanceOwnership.OwnedByLifetimeScope,
                new[] { service },
                new Dictionary<string, object?>());
    }

    internal class DependsOnRegisteredInstance
    {
        public DependsOnRegisteredInstance(object instance)
        {
            Instance = instance;
        }

        internal object Instance
        {
            get; set;
        }
    }

    private class Person
    {
    }

    private sealed class DecoratedServiceImplementation : IDecoratedService
    {
    }

    private sealed class DecoratedServiceDecorator : IDecoratedService
    {
        public DecoratedServiceDecorator(IDecoratedService decorated)
        {
            Decorated = decorated;
        }

        public IDecoratedService Decorated
        {
            get;
        }
    }
}
