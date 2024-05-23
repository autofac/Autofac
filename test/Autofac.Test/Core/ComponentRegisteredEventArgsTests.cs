// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac.Core;

namespace Autofac.Test.Core;

public class ComponentRegisteredEventArgsTests
{
    [Fact]
    public void ConstructorSetsProperties()
    {
        using var registry = Factory.CreateEmptyComponentRegistryBuilder();
        using var registration = Factory.CreateSingletonObjectRegistration();
        var args = new ComponentRegisteredEventArgs(registry, registration);
        Assert.Same(registry, args.ComponentRegistryBuilder);
        Assert.Same(registration, args.ComponentRegistration);
    }

    [Fact]
    public void NullContainerDetected()
    {
        using var registration = Factory.CreateSingletonObjectRegistration();
        Assert.Throws<ArgumentNullException>(() => new ComponentRegisteredEventArgs(null, registration));
    }

    [Fact]
    public void NullRegistrationDetected()
    {
        using var registry = Factory.CreateEmptyComponentRegistryBuilder();
        Assert.Throws<ArgumentNullException>(() => new ComponentRegisteredEventArgs(registry, null));
    }
}
