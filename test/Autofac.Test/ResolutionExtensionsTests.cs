// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac.Core;
using Autofac.Core.Activators.ProvidedInstance;
using Autofac.Core.Registration;
using Autofac.Test.Scenarios.Parameterization;
using Autofac.Test.Scenarios.WithProperty;

namespace Autofac.Test;

public class ResolutionExtensionsTests
{
    [Fact]
    public void ResolvingUnregisteredService_ProvidesDescriptionInException()
    {
        using var target = Factory.CreateEmptyContainer();
        var ex = Assert.Throws<ComponentNotRegisteredException>(() => target.Resolve<object>());
        Assert.Contains("System.Object", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void WhenComponentIsRegistered_IsRegisteredReturnsTrueForAllServices()
    {
        using var activator = Factory.CreateProvidedInstanceActivator("Hello");
        using var registration = Factory.CreateSingletonRegistration(
            new[] { new TypedService(typeof(object)), new TypedService(typeof(string)) },
            activator);

        using var builder = Factory.CreateEmptyComponentRegistryBuilder();
        builder.Register(registration);
        var target = new ContainerBuilder(builder).Build();

        Assert.True(target.IsRegistered<object>());
        Assert.True(target.IsRegistered<string>());
    }

    [Fact]
    public void WhenServiceIsRegistered_ResolveOptionalReturnsAnInstance()
    {
        using var activator = new ProvidedInstanceActivator("Hello");
        using var builder = Factory.CreateEmptyComponentRegistryBuilder();
        using var registration = Factory.CreateSingletonRegistration(
            new[] { new TypedService(typeof(string)) },
            activator);
        builder.Register(registration);

        var target = new ContainerBuilder(builder).Build();

        var inst = target.ResolveOptional<string>();

        Assert.Equal("Hello", inst);
    }

    [Fact]
    public void WhenServiceNotRegistered_ResolveOptionalReturnsNull()
    {
        using var target = Factory.CreateEmptyContainer();
        var inst = target.ResolveOptional<string>();
        Assert.Null(inst);
    }

    [Fact]
    public void WhenParametersProvided_ResolveOptionalSuppliesThemToComponent()
    {
        var cb = new ContainerBuilder();
        cb.RegisterType<Parameterized>();
        var container = cb.Build();
        const string Param1 = "Hello";
        const int Param2 = 42;
        var result = container.ResolveOptional<Parameterized>(
            new NamedParameter("a", Param1),
            new NamedParameter("b", Param2));
        Assert.NotNull(result);
        Assert.Equal(Param1, result.A);
        Assert.Equal(Param2, result.B);
    }

    [Fact]
    public void WhenPredicateAndValueParameterSupplied_PassedToComponent()
    {
        const string A = "Hello";
        const int B = 42;
        var builder = new ContainerBuilder();

        builder.RegisterType<Parameterized>()
            .WithParameter(
                (pi, c) => pi.Name == "a",
                (pi, c) => A)
            .WithParameter(
                (pi, c) => pi.Name == "b",
                (pi, c) => B);

        var container = builder.Build();
        var result = container.Resolve<Parameterized>();

        Assert.Equal(A, result.A);
        Assert.Equal(B, result.B);
    }

    [Fact]
    public void RegisterPropertyWithExpression()
    {
        const string A = "Hello";
        const bool B = true;
        var builder = new ContainerBuilder();

        builder.RegisterType<WithProps>()
            .WithProperty(x => x.A, A)
            .WithProperty(x => x.B, B);

        var container = builder.Build();
        var result = container.Resolve<WithProps>();

        Assert.Equal(A, result.A);
        Assert.Equal(B, result.B);
    }

    [Fact]
    public void RegisterPropertyWithExpressionFieldExceptions()
    {
        const string A = "Hello";
        var builder = new ContainerBuilder();

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            builder.RegisterType<WithProps>().WithProperty(x => x._field, A));
    }

    [Fact]
    public void WhenServiceIsRegistered_TryResolveNamedReturnsTrue()
    {
        const string Name = "name";

        var cb = new ContainerBuilder();
        cb.RegisterType<object>().Named<object>(Name);

        var container = cb.Build();

        Assert.True(container.TryResolveNamed<object>(Name, out var o));
        Assert.NotNull(o);
    }

    [Fact]
    public void WhenServiceIsNotRegistered_TryResolveNamedReturnsFalse()
    {
        using var container = Factory.CreateEmptyContainer();

        Assert.False(container.TryResolveNamed<object>("name", out var o));
        Assert.Null(o);
    }

    [Fact]
    public void WhenServiceIsRegistered_TryResolveKeyedReturnsTrue()
    {
        var key = new object();

        var cb = new ContainerBuilder();
        cb.RegisterType<object>().Keyed<object>(key);

        var container = cb.Build();

        Assert.True(container.TryResolveKeyed<object>(key, out var o));
        Assert.NotNull(o);
    }

    [Fact]
    public void WhenServiceIsNotRegistered_TryResolveKeyedReturnsFalse()
    {
        using var container = Factory.CreateEmptyContainer();

        Assert.False(container.TryResolveKeyed<object>("name", out var o));
        Assert.Null(o);
    }

    [Fact]
    public void ResolveKeyed_AnyKeyNonCollection()
    {
        using var container = Factory.CreateEmptyContainer();

        Assert.Throws<DependencyResolutionException>(
            () => container.ResolveKeyed<object>(KeyedService.AnyKey));
    }

    [Fact]
    public void ResolveKeyed_AnyKeyEnumerable()
    {
        var builder = new ContainerBuilder();
        builder.RegisterType<AnyKeyService>().Keyed<AnyKeyService>("first");
        builder.RegisterType<AnyKeyService>().Keyed<AnyKeyService>("second");
        using var container = builder.Build();

        var services = container.ResolveKeyed<IEnumerable<AnyKeyService>>(KeyedService.AnyKey).ToList();

        Assert.Equal(2, services.Count);
    }

    [Fact]
    public void TryResolveKeyed_AnyKeyNonCollection()
    {
        using var container = Factory.CreateEmptyContainer();

        Assert.Throws<DependencyResolutionException>(
            () => container.TryResolveKeyed(KeyedService.AnyKey, typeof(object), out _));
    }

    [Fact]
    public void TryResolveKeyed_AnyKeyEnumerable()
    {
        var builder = new ContainerBuilder();
        builder.RegisterType<AnyKeyService>().Keyed<AnyKeyService>("only");
        using var container = builder.Build();

        var result = container.TryResolveKeyed<IEnumerable<AnyKeyService>>(KeyedService.AnyKey, out var services);

        Assert.True(result);
        Assert.NotNull(services);
        Assert.Single(services!);
    }

    private sealed class AnyKeyService
    {
    }
}
