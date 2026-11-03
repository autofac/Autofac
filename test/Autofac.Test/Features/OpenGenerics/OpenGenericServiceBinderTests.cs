// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac.Core;
using Autofac.Features.OpenGenerics;

namespace Autofac.Test.Features.OpenGenerics;

public class OpenGenericServiceBinderTests
{
    [Fact]
    public void TypedServicesAreClosedInConfiguredOrder()
    {
        var configured = new Service[]
        {
            new TypedService(typeof(IFirst<>)),
            new KeyedService("key", typeof(IFirst<>)),
            new TypedService(typeof(ISecond<>)),
        };

        Assert.True(OpenGenericServiceBinder.TryBindOpenGenericTypedService(
            new TypedService(typeof(ISecond<int>)), configured, typeof(FirstAndSecond<>), out var implementationType, out var services));

        Assert.Equal(typeof(FirstAndSecond<int>), implementationType);
        Assert.Equal(
            new Service[]
            {
                new TypedService(typeof(IFirst<int>)),
                new KeyedService("key", typeof(IFirst<int>)),
                new TypedService(typeof(ISecond<int>)),
            },
            services);
    }

    [Fact]
    public void TypedServicesTheImplementationDoesNotImplementAreLeftOut()
    {
        var configured = new Service[] { new TypedService(typeof(IFirst<>)), new TypedService(typeof(ISecond<>)) };

        Assert.True(OpenGenericServiceBinder.TryBindOpenGenericTypedService(
            new TypedService(typeof(IFirst<int>)), configured, typeof(FirstAndSecondOfEnumerable<>), out _, out var services));

        Assert.Equal(new Service[] { new TypedService(typeof(IFirst<int>)) }, services);
    }

    [Fact]
    public void TypedServicesWithADifferentNumberOfArgumentsAreLeftOut()
    {
        var configured = new Service[] { new TypedService(typeof(IFirst<>)), new TypedService(typeof(IPair<,>)) };

        Assert.True(OpenGenericServiceBinder.TryBindOpenGenericTypedService(
            new TypedService(typeof(IFirst<int>)), configured, typeof(FirstAndPair<>), out _, out var firstServices));
        Assert.True(OpenGenericServiceBinder.TryBindOpenGenericTypedService(
            new TypedService(typeof(IPair<int, int>)), configured, typeof(FirstAndPair<>), out _, out var pairServices));

        Assert.Equal(new Service[] { new TypedService(typeof(IFirst<int>)) }, firstServices);
        Assert.Equal(new Service[] { new TypedService(typeof(IPair<int, int>)) }, pairServices);
    }

    [Fact]
    public void TypedServiceIsNotBoundWhenNoConfiguredServiceMatches()
    {
        var configured = new Service[] { new KeyedService("key", typeof(IFirst<>)) };

        Assert.False(OpenGenericServiceBinder.TryBindOpenGenericTypedService(
            new TypedService(typeof(IFirst<int>)), configured, typeof(FirstAndSecond<>), out var implementationType, out var services));
        Assert.False(OpenGenericServiceBinder.TryBindOpenGenericTypedService(
            new KeyedService("other", typeof(IFirst<int>)), configured, typeof(FirstAndSecond<>), out _, out _));
        Assert.False(OpenGenericServiceBinder.TryBindOpenGenericTypedService(
            new KeyedService("key", typeof(ISecond<int>)), configured, typeof(FirstAndSecond<>), out _, out _));

        Assert.Null(implementationType);
        Assert.Null(services);
    }

    [Fact]
    public void DelegateServicesAreClosedInConfiguredOrderWhenTheyHaveTheSameNumberOfArguments()
    {
        var configured = new Service[]
        {
            new TypedService(typeof(IFirst<>)),
            new TypedService(typeof(IPair<,>)),
            new KeyedService("key", typeof(ISecond<>)),
        };

        Assert.True(OpenGenericServiceBinder.TryBindOpenGenericDelegateService(
            new KeyedService("key", typeof(ISecond<int>)), configured, (context, types, parameters) => types, out var factory, out var services));

        Assert.Equal(
            new Service[] { new TypedService(typeof(IFirst<int>)), new KeyedService("key", typeof(ISecond<int>)) },
            services);
        Assert.Equal(new[] { typeof(int) }, (Type[])factory(null!, Array.Empty<Parameter>()));
    }

    [Fact]
    public void DelegateServiceIsNotBoundWhenNoConfiguredServiceMatches()
    {
        var configured = new Service[] { new TypedService(typeof(IFirst<>)) };

        Assert.False(OpenGenericServiceBinder.TryBindOpenGenericDelegateService(
            new TypedService(typeof(ISecond<int>)), configured, (context, types, parameters) => types, out var factory, out var services));

        Assert.Null(factory);
        Assert.Null(services);
    }

    [Fact]
    public void InterfaceThatMapsToTheServiceArgumentsIsPreferredOverAnEarlierCompleteMapping()
    {
        // PairAndReversedList<string, List<int>> and PairAndReversedList<int, string> both implement
        // IPair<string, List<int>>, the second through the interface that is declared first.
        Assert.True(OpenGenericServiceBinder.TryBindOpenGenericTypedService(
            new TypedService(typeof(IPair<string, List<int>>)),
            new Service[] { new TypedService(typeof(IPair<,>)) },
            typeof(PairAndReversedList<,>),
            out var implementationType,
            out _));

        Assert.Equal(typeof(PairAndReversedList<string, List<int>>), implementationType);
    }

    [Fact]
    public void ConstraintOnAnotherParameterChecksTheConcreteArgumentsOfTheMatchingInterface()
    {
        var builder = new ContainerBuilder();
        builder.RegisterGeneric(typeof(DictionaryHandler<,>)).As(typeof(IHandler<,>));
        var container = builder.Build();

        Assert.IsType<DictionaryHandler<Dictionary<string, Model>, Model>>(container.Resolve<IHandler<Dictionary<string, Model>, Model>>());
        Assert.False(container.IsRegistered<IHandler<Dictionary<int, Model>, Model>>());
    }

    [Fact]
    public void ConstraintOnAGenericBaseClassIsFoundFurtherUpTheHierarchy()
    {
        var builder = new ContainerBuilder();
        builder.RegisterGeneric(typeof(EntityHandler<,>)).As(typeof(IHandler<,>));
        var container = builder.Build();

        Assert.IsType<EntityHandler<DerivedEntity, Model>>(container.Resolve<IHandler<DerivedEntity, Model>>());
    }

    [Fact]
    public void ConstraintIgnoresInterfacesWhoseArgumentsCannotCloseTheConstraint()
    {
        // ValueSourceEntity also implements IReference<string>, which has the same number of arguments as
        // IValueSource<> but can't close it, since string isn't a value type.
        var builder = new ContainerBuilder();
        builder.RegisterGeneric(typeof(ValueSourceHandler<,>)).As(typeof(IHandler<,>));
        var container = builder.Build();

        Assert.IsType<ValueSourceHandler<ValueSourceEntity, int>>(container.Resolve<IHandler<ValueSourceEntity, int>>());
        Assert.False(container.IsRegistered<IHandler<ReferenceOnlyEntity, int>>());
    }

    private interface IFirst<T>
    {
    }

    private interface ISecond<T>
    {
    }

    private interface IPair<T1, T2>
    {
    }

    private interface IHandler<T1, T2>
    {
    }

    private interface IValueSource<T>
        where T : struct
    {
    }

    private interface IReference<T>
    {
    }

    private class FirstAndSecond<T> : IFirst<T>, ISecond<T>
    {
    }

    private class FirstAndSecondOfEnumerable<T> : IFirst<T>, ISecond<IEnumerable<T>>
    {
    }

    private class FirstAndPair<T> : IFirst<T>, IPair<T, T>
    {
    }

    private class PairAndReversedList<T1, T2> : IPair<T2, List<T1>>, IPair<T1, T2>
    {
    }

    private class Model
    {
    }

    private class DictionaryHandler<TDictionary, TValue> : IHandler<TDictionary, TValue>
        where TDictionary : IDictionary<string, TValue>
    {
    }

    private class Entity<TModel>
    {
    }

    private class ModelEntity : Entity<Model>
    {
    }

    private class DerivedEntity : ModelEntity
    {
    }

    private class EntityHandler<TEntity, TModel> : IHandler<TEntity, TModel>
        where TEntity : Entity<TModel>
    {
    }

    private class ValueSourceEntity : IReference<string>, IValueSource<int>
    {
    }

    private class ReferenceOnlyEntity : IReference<string>
    {
    }

    private class ValueSourceHandler<TEntity, TValue> : IHandler<TEntity, TValue>
        where TEntity : IValueSource<TValue>
        where TValue : struct
    {
    }
}
