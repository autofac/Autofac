// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac.Core;
using Autofac.Test.Scenarios.Dependencies.Circularity;

namespace Autofac.Test;

public class CircularDependencyTests
{
    [Fact]
    public void IdentifiesCircularDependencyInExceptionMessage()
    {
        var builder = new ContainerBuilder();
        builder.RegisterType<D>().As<ID>();
        builder.RegisterType<A>().As<IA>();
        builder.RegisterType<BC>().As<IB, IC>();
        var container = builder.Build();
        var de = Assert.Throws<DependencyResolutionException>(() => container.Resolve<ID>());
        Assert.Contains("Autofac.Test.Scenarios.Dependencies.Circularity.D -> Autofac.Test.Scenarios.Dependencies.Circularity.A -> Autofac.Test.Scenarios.Dependencies.Circularity.BC -> Autofac.Test.Scenarios.Dependencies.Circularity.A", de.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public void DetectsSelfDependencyOnFirstRepeat()
    {
        var builder = new ContainerBuilder();
        builder.RegisterType<SelfDependent>();
        var container = builder.Build();
        var de = Assert.Throws<DependencyResolutionException>(() => container.Resolve<SelfDependent>());
        var name = typeof(SelfDependent).FullName;
        Assert.Contains($"{name} -> {name}", de.ToString(), StringComparison.Ordinal);
        Assert.DoesNotContain($"{name} -> {name} -> {name}", de.ToString(), StringComparison.Ordinal);
    }

    private sealed class SelfDependent
    {
        public SelfDependent(SelfDependent self)
        {
        }
    }
}
