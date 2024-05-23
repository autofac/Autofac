// Copyright (c) Autofac Project. All rights reserved.
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

        using var activator = Factory.CreateProvidedInstanceActivator(new object());
        using var registration = Factory.CreateSingletonRegistration(services, activator);

        Assert.Contains(MetadataKeys.RegistrationOrderMetadataKey, registration.Metadata.Keys);
    }

    [Fact]
    public async Task AsyncDisposeComponentRegistrationAsyncDisposesActivator()
    {
        var services = new Service[] { new TypedService(typeof(object)) };

        await using var disposable = new AsyncDisposeTracker();

        using var activator = Factory.CreateProvidedInstanceActivator(disposable);
        activator.DisposeInstance = true;

        var registration = Factory.CreateSingletonRegistration(services, activator);

        await registration.DisposeAsync();

        Assert.True(disposable.IsAsyncDisposed);
    }

    [Fact]
    public async Task AsyncDisposeComponentRegistrationCanDisposeSyncActivator()
    {
        var services = new Service[] { new TypedService(typeof(object)) };

        using var activator = Factory.CreateProvidedInstanceActivator(new object());
        var registration = Factory.CreateSingletonRegistration(services, activator);

        await registration.DisposeAsync();
    }

    [Fact]
    public void ShouldHaveAscendingRegistrationOrderMetadataValue()
    {
        var services = new Service[] { new TypedService(typeof(string)) };
        using var activator1 = Factory.CreateProvidedInstanceActivator("s1");
        using var activator2 = Factory.CreateProvidedInstanceActivator("s2");
        using var activator3 = Factory.CreateProvidedInstanceActivator("s3");

        var registration1 = Factory.CreateSingletonRegistration(services, activator1);
        var registration2 = Factory.CreateSingletonRegistration(services, activator2);
        var registration3 = Factory.CreateSingletonRegistration(services, activator3);
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

        using var activator = Factory.CreateProvidedInstanceActivator(new object());
        using var registration = Factory.CreateSingletonRegistration(services, activator);

        using var builder = Factory.CreateEmptyComponentRegistryBuilder();
        builder.Register(registration);

        builder.Build();

        Assert.Throws<InvalidOperationException>(() => registration.PipelineBuilding += (s, p) => { });
    }
}
