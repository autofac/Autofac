// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Autofac.Specification.Test.Registration;

public class TypeRegistrationTests
{
    private interface IA
    {
    }

    private interface IB
    {
    }

    private interface IC
    {
    }

    private interface IMyService
    {
    }

    public delegate void MyDelegateType();

    private abstract class MyAbstractClass
    {
    }

    private class MyOpenGeneric<T>
    {
    }

    private struct MyValueType
    {
        public MyValueType(IMyService service)
        {
            Service = service;
        }

        public IMyService Service
        {
            get;
        }
    }

    [Fact]
    public void AsImplementedInterfacesGeneric()
    {
        var builder = new ContainerBuilder();
        builder.RegisterType<Abc>().AsImplementedInterfaces();
        var context = builder.Build();

        var resultA = context.Resolve<IA>();
        var resultB = context.Resolve<IB>();
        var resultC = context.Resolve<IC>();
        Assert.NotNull(resultA);
        Assert.NotNull(resultB);
        Assert.NotNull(resultC);
    }

    [Fact]
    public void AsImplementedInterfacesNonGeneric()
    {
        var builder = new ContainerBuilder();
        builder.RegisterType(typeof(Abc)).AsImplementedInterfaces();
        var context = builder.Build();

        var resultA = context.Resolve<IA>();
        var resultB = context.Resolve<IB>();
        var resultC = context.Resolve<IC>();
        Assert.NotNull(resultA);
        Assert.NotNull(resultB);
        Assert.NotNull(resultC);
    }

    [Fact]
    public void AsSelfGeneric()
    {
        var builder = new ContainerBuilder();
        builder.RegisterType<Abc>().AsSelf();
        var context = builder.Build();

        var result = context.Resolve<Abc>();
        Assert.NotNull(result);
    }

    [Fact]
    public void AsSelfNonGeneric()
    {
        var builder = new ContainerBuilder();
        builder.RegisterType(typeof(Abc)).AsSelf();
        var context = builder.Build();

        var result = context.Resolve<Abc>();
        Assert.NotNull(result);
    }

    [Fact]
    public void OneTypeImplementMultipleInterfaces_OtherObjectsImplementingOneOfThoseInterfaces_CanBeResolved()
    {
        var builder = new ContainerBuilder();
        builder.RegisterType(typeof(Abc)).As(typeof(IA), typeof(IB));
        builder.RegisterType(typeof(A)).As(typeof(IA));

        var container = builder.Build();
        var lifetime = container.BeginLifetimeScope(cb => cb.Register(_ => new object()));
        Assert.NotNull(lifetime.Resolve<IB>());

        var allImplementationsOfServiceA = lifetime.Resolve<IEnumerable<IA>>();
        Assert.Equal(2, allImplementationsOfServiceA.Count());
    }

    [Fact]
    public void RegisterTypeAsSupportedAndUnsupportedService()
    {
        var builder = new ContainerBuilder();
        builder.RegisterType<A>().As<IA, IB>();
        Assert.Throws<ArgumentException>(() => builder.Build());
    }

    [Fact]
    public void RegisterTypeAsUnsupportedService()
    {
        var builder = new ContainerBuilder();
        builder.RegisterType<string>().As<IA>();
        Assert.Throws<ArgumentException>(() => builder.Build());
    }

    [Fact]
    public void RegisterTypeMustBeConcrete_Generic()
    {
        var builder = new ContainerBuilder();
        Assert.Throws<ArgumentException>(() => builder.RegisterType<IMyService>());
        Assert.Throws<ArgumentException>(() => builder.RegisterType<MyDelegateType>());
        Assert.Throws<ArgumentException>(() => builder.RegisterType<MyAbstractClass>());
        Assert.Throws<ArgumentException>(() => builder.RegisterType<MyValueType>());
    }

    [Theory]
    [InlineData(typeof(MyDelegateType))]
    [InlineData(typeof(IMyService))]
    [InlineData(typeof(MyAbstractClass))]
    [InlineData(typeof(MyOpenGeneric<>))]
    [InlineData(typeof(MyValueType))]
    public void RegisterTypeMustBeConcrete_Parameter(Type type)
    {
        var builder = new ContainerBuilder();
        Assert.Throws<ArgumentException>(() => builder.RegisterType(type));
    }

    [Fact]
    public void RegisterTypesCanBeFilteredByAssignableTo()
    {
        var container = new ContainerBuilder().Build().BeginLifetimeScope(b =>
            b.RegisterTypes(typeof(MyComponent), typeof(MyComponent2))
                .AssignableTo(typeof(IMyService)));

        Assert.Single(container.ComponentRegistry.Registrations);
        Assert.True(container.TryResolve(typeof(MyComponent), out var obj));
        Assert.False(container.TryResolve(typeof(MyComponent2), out obj));
    }

    [Fact]
    public void RegisterTypesIgnoresNonRegisterableTypes()
    {
        var container = new ContainerBuilder().Build().BeginLifetimeScope(b =>
            b.RegisterTypes(
                typeof(IMyService),
                typeof(MyDelegateType),
                typeof(MyAbstractClass),
                typeof(MyOpenGeneric<>),
                typeof(MyValueType),
                typeof(MyOpenGeneric<int>),
                typeof(MyComponent)));

        Assert.Equal(2, container.ComponentRegistry.Registrations.Count());
        Assert.True(container.TryResolve(typeof(MyComponent), out var _));
        Assert.True(container.TryResolve(typeof(MyOpenGeneric<int>), out var _));
        Assert.False(container.TryResolve(typeof(IMyService), out _));
        Assert.False(container.TryResolve(typeof(MyDelegateType), out _));
        Assert.False(container.TryResolve(typeof(MyAbstractClass), out _));
        Assert.False(container.TryResolve(typeof(MyValueType), out _));
    }

    [Fact]
    public void RegisterTypesIgnoresNullValues()
    {
        var container = new ContainerBuilder().Build().BeginLifetimeScope(b =>
            b.RegisterTypes(null, typeof(MyComponent), null));

        Assert.Single(container.ComponentRegistry.Registrations);
        Assert.True(container.TryResolve(typeof(MyComponent), out var _));
    }

    [Fact]
    public void TypeRegisteredOnlyWithServiceNotRegisteredAsSelf()
    {
        var cb = new ContainerBuilder();
        cb.RegisterType<A>().As<IA>();
        var c = cb.Build();
        Assert.False(c.IsRegistered<A>());
    }

    [Fact]
    public void TypeRegisteredWithMultipleServicesCanBeResolved()
    {
        var target = new ContainerBuilder();
        target.RegisterType<Abc>()
            .As<IA, IB, IC>()
            .SingleInstance();
        var container = target.Build();
        var a = container.Resolve<IA>();
        var b = container.Resolve<IB>();
        var c = container.Resolve<IC>();
        Assert.NotNull(a);
        Assert.Same(a, b);
        Assert.Same(b, c);
    }

    [Fact]
    public void TypeRegisteredWithoutServiceCanBeResolved()
    {
        var cb = new ContainerBuilder();
        cb.RegisterType<A>();
        var c = cb.Build();
        var a = c.Resolve<A>();
        Assert.NotNull(a);
        Assert.IsType<A>(a);
    }

    [Fact]
    public void TypeRegisteredWithSingleServiceCanBeResolved()
    {
        var cb = new ContainerBuilder();
        cb.RegisterType<A>().As<IA>();
        var c = cb.Build();
        var a = c.Resolve<IA>();
        Assert.NotNull(a);
        Assert.IsType<A>(a);
    }

    private class A : IA
    {
    }

    private class Abc : IA, IB, IC
    {
    }

    private sealed class MyComponent : IMyService
    {
    }

    private sealed class MyComponent2
    {
    }
}
