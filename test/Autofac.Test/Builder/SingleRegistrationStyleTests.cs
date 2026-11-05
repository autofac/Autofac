// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac.Builder;
using Autofac.Core;

namespace Autofac.Test.Builder;

public class SingleRegistrationStyleTests
{
    [Fact]
    public void RegisteredHandlersKeepTheHandlersAdded()
    {
        var style = new SingleRegistrationStyle();
        EventHandler<ComponentRegisteredEventArgs> handler = (s, e) => { };
        Assert.False(style.HasRegisteredHandlers);

        style.RegisteredHandlers.Add(handler);

        Assert.Same(style.RegisteredHandlers, style.RegisteredHandlers);
        Assert.Equal(new[] { handler }, style.RegisteredHandlers);
        Assert.True(style.HasRegisteredHandlers);
    }

    [Fact]
    public void HandlerAddedToARegistrationRunsWhenItIsRegistered()
    {
        var builder = new ContainerBuilder();
        IComponentRegistration? registered = null;
        builder.RegisterType<object>().OnRegistered(e => registered = e.ComponentRegistration);

        using var container = builder.Build();

        Assert.NotNull(registered);
        Assert.Same(registered, container.ComponentRegistry.RegistrationsFor(new TypedService(typeof(object))).Single());
    }
}
