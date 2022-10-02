﻿// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac.Builder;
using Autofac.Core;
using Autofac.Test.Util;

namespace Autofac.Test.Core;

public class ComponentRegistrationTests
{
    [Fact]
    public void Constructor_DetectsNullsAmongServices()
    {
        Assert.Throws<ArgumentException>(() =>
        {
            var services = new Service[] { new TypedService(typeof(object)), null };
            Factory.CreateSingletonRegistration(services, Factory.CreateProvidedInstanceActivator(new object()));
        });
    }

    [Fact]
    public void ShouldHaveRegistrationOrderMetadataKey()
    {
        var services = new Service[] { new TypedService(typeof(object)) };

        var registration = Factory.CreateSingletonRegistration(services, Factory.CreateProvidedInstanceActivator(new object()));

        Assert.Contains(MetadataKeys.RegistrationOrderMetadataKey, registration.Metadata.Keys);
    }

    [Fact]
    public async Task AsyncDisposeComponentRegistrationAsyncDisposesActivator()
    {
        var services = new Service[] { new TypedService(typeof(object)) };

        var disposable = new AsyncDisposeTracker();

        var activator = Factory.CreateProvidedInstanceActivator(disposable);
        activator.DisposeInstance = true;

        var registration = Factory.CreateSingletonRegistration(services, activator);

        await registration.DisposeAsync();

        Assert.True(disposable.IsAsyncDisposed);
    }

    [Fact]
    public async Task AsyncDisposeComponentRegistrationCanDisposeSyncActivator()
    {
        var services = new Service[] { new TypedService(typeof(object)) };

        var registration = Factory.CreateSingletonRegistration(services, Factory.CreateReflectionActivator(typeof(object)));

        await registration.DisposeAsync();
    }

    [Fact]
    public void ShouldHaveAscendingRegistrationOrderMetadataValue()
    {
        var services = new Service[] { new TypedService(typeof(string)) };
        var registration1 = Factory.CreateSingletonRegistration(services, Factory.CreateProvidedInstanceActivator("s1"));
        var registration2 = Factory.CreateSingletonRegistration(services, Factory.CreateProvidedInstanceActivator("s2"));
        var registration3 = Factory.CreateSingletonRegistration(services, Factory.CreateProvidedInstanceActivator("s3"));
        var registrations = new List<IComponentRegistration> { registration2, registration3, registration1 };

        var orderedRegistrations = registrations.OrderBy(cr => cr.GetRegistrationOrder()).ToArray();

        Assert.Same(registration1, orderedRegistrations[0]);
        Assert.Same(registration2, orderedRegistrations[1]);
        Assert.Same(registration3, orderedRegistrations[2]);
    }

    [Fact]
    public void AttachingToPipelineBuildingShouldFailIfAlreadyBuilt()
    {
        var services = new Service[] { new TypedService(typeof(object)) };

        var registration = Factory.CreateSingletonRegistration(services, Factory.CreateProvidedInstanceActivator(new object()));

        var builder = Factory.CreateEmptyComponentRegistryBuilder();
        builder.Register(registration);

        builder.Build();

        Assert.Throws<InvalidOperationException>(() => registration.PipelineBuilding += (s, p) => { });
    }

    [Fact]
    public void CanReplaceActivatorBeforePipelineHasBeenBuilt()
    {
        var services = new Service[] { new TypedService(typeof(object)) };

        var registration = Factory.CreateSingletonRegistration(services, Factory.CreateProvidedInstanceActivator(new object()));

        var newActivator = Factory.CreateProvidedInstanceActivator(new object());

        registration.ReplaceActivator(newActivator);

        Assert.Same(registration.Activator, newActivator);
    }

    [Fact]
    public void CannotReplaceActivatorAfterRegistrationHasBeenBuilt()
    {
        var services = new Service[] { new TypedService(typeof(object)) };

        var registration = Factory.CreateSingletonRegistration(services, Factory.CreateProvidedInstanceActivator(new object()));

        var builder = Factory.CreateEmptyComponentRegistryBuilder();
        builder.Register(registration);

        builder.Build();

        Assert.Throws<InvalidOperationException>(() => registration.ReplaceActivator(Factory.CreateProvidedInstanceActivator(new object())));
    }

    [Fact]
    public void CannotReplaceActivatorIfTargetingAnotherRegistration()
    {
        var services = new Service[] { new TypedService(typeof(object)) };

        var registration = Factory.CreateSingletonRegistration(services, Factory.CreateProvidedInstanceActivator(new object()));

        var targetingRegistration = Factory.CreateTargetingRegistration(registration);

        Assert.Throws<InvalidOperationException>(() => targetingRegistration.ReplaceActivator(Factory.CreateProvidedInstanceActivator(new object())));
    }

    [Fact]
    public void ReplacementActivatorLimitTypeMustSatisfyAllServices()
    {
        var services = new Service[] { new TypedService(typeof(IServiceA)), new TypedService(typeof(IServiceB)) };

        var registration = Factory.CreateSingletonRegistration(services, Factory.CreateProvidedInstanceActivator(new ComponentAB()));

        Assert.Throws<ArgumentException>(() => registration.ReplaceActivator(Factory.CreateProvidedInstanceActivator(new ComponentA())));
    }

    private interface IServiceA
    {
    }

    private interface IServiceB
    {
    }

    public class ComponentA : IServiceA
    {
    }

    public class ComponentAB : IServiceA, IServiceB
    {
    }
}
