// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac.Builder;
using Autofac.Core;

namespace Autofac.Test.Builder;

public class GetRegistrationIdTests
{
    [Fact]
    public void GetRegistrationId_NullRegistration_ThrowsArgumentNullException()
    {
        // Issue #1327
        IRegistrationBuilder<object, SimpleActivatorData, SingleRegistrationStyle> registration = null;
        Assert.Throws<ArgumentNullException>(() => registration.GetRegistrationId());
    }

    [Fact]
    public void GetRegistrationId_RegisterType_MatchesBuiltComponentRegistrationId()
    {
        // Issue #1327 - The ID returned before build matches IComponentRegistration.Id after build.
        var cb = new ContainerBuilder();
        var rb = cb.RegisterType<MyComponent>();
        var capturedId = rb.GetRegistrationId();
        var container = cb.Build();

        Assert.True(container.ComponentRegistry.TryGetRegistration(new TypedService(typeof(MyComponent)), out var cr));
        Assert.Equal(capturedId, cr.Id);
    }

    [Fact]
    public void GetRegistrationId_RegisterDelegate_MatchesBuiltComponentRegistrationId()
    {
        // Issue #1327 - The ID returned before build matches IComponentRegistration.Id after build.
        var cb = new ContainerBuilder();
        var rb = cb.Register(_ => new MyComponent());
        var capturedId = rb.GetRegistrationId();
        var container = cb.Build();

        Assert.True(container.ComponentRegistry.TryGetRegistration(new TypedService(typeof(MyComponent)), out var cr));
        Assert.Equal(capturedId, cr.Id);
    }

    [Fact]
    public void GetRegistrationId_RegisterInstance_MatchesBuiltComponentRegistrationId()
    {
        // Issue #1327 - The ID returned before build matches IComponentRegistration.Id after build.
        var instance = new MyComponent();
        var cb = new ContainerBuilder();
        var rb = cb.RegisterInstance(instance);
        var capturedId = rb.GetRegistrationId();
        var container = cb.Build();

        Assert.True(container.ComponentRegistry.TryGetRegistration(new TypedService(typeof(MyComponent)), out var cr));
        Assert.Equal(capturedId, cr.Id);
    }

    [Fact]
    public void GetRegistrationId_CalledTwice_ReturnsSameGuid()
    {
        // Issue #1327 - The ID is stable across multiple calls on the same builder.
        var cb = new ContainerBuilder();
        var rb = cb.RegisterType<MyComponent>();

        var id1 = rb.GetRegistrationId();
        var id2 = rb.GetRegistrationId();

        Assert.Equal(id1, id2);
    }

    [Fact]
    public void GetRegistrationId_TwoDistinctRegistrations_ReturnDifferentGuids()
    {
        // Issue #1327 - Each registration gets its own unique ID.
        var cb = new ContainerBuilder();
        var rb1 = cb.RegisterType<MyComponent>();
        var rb2 = cb.Register(_ => new MyComponent());

        Assert.NotEqual(rb1.GetRegistrationId(), rb2.GetRegistrationId());
    }

    private class MyComponent
    {
    }
}
