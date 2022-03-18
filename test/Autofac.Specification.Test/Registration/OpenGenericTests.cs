// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac.Test.Scenarios.Graph1.GenericContraints;

namespace Autofac.Specification.Test.Registration;

public class OpenGenericTests
{
    private interface IImplementedInterface<T>
    {
    }

    [Fact]
    public void AsImplementedInterfacesOnOpenGeneric()
    {
        var builder = new ContainerBuilder();
        builder.RegisterGeneric(typeof(SelfComponent<>)).AsImplementedInterfaces();
        var context = builder.Build();
        context.Resolve<IImplementedInterface<object>>();
    }

    [Fact]
    public void AsSelfOnOpenGeneric()
    {
        var builder = new ContainerBuilder();
        builder.RegisterGeneric(typeof(SelfComponent<>)).AsSelf();
        var context = builder.Build();
        context.Resolve<SelfComponent<object>>();
    }

    [Fact]
    public void ResolveWithMultipleCandidatesLimitedByGenericConstraintsShouldSucceed()
    {
        var containerBuilder = new ContainerBuilder();
        containerBuilder.RegisterType<A>().As<IA>();
        containerBuilder.RegisterGeneric(typeof(Unrelated<>)).As(typeof(IB<>));
        containerBuilder.RegisterType<Required>().As<IB<ClassWithParameterlessButNotPublicConstructor>>();

        var container = containerBuilder.Build();
        var resolved = container.Resolve<IA>();
        Assert.NotNull(resolved);
    }

    // Issue #1315: Class services fail to resolve if the names on the type
    // arguments are changed.
    [Fact]
    public void ResolveClassWithRenamedTypeArguments()
    {
        var containerBuilder = new ContainerBuilder();
        containerBuilder.RegisterGeneric(typeof(DerivedRepository<,>)).As(typeof(BaseRepository<,>));

        var container = containerBuilder.Build();
        var resolved = container.Resolve<BaseRepository<string, int>>();
        Assert.IsType<DerivedRepository<int, string>>(resolved);
    }

    private class SelfComponent<T> : IImplementedInterface<T>
    {
    }

    private class BaseRepository<T1, T2>
    {
    }

    // Issue #1315: Class services fail to resolve if the names on the type
    // arguments are changed.
    private class DerivedRepository<TSecond, TFirst> : BaseRepository<TFirst, TSecond>
    {
    }
}
