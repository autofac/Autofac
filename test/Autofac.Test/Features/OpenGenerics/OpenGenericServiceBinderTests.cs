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

    [Theory]
    [MemberData(nameof(RequestsForCustomServices))]
    public void TypedServicesSkipServicesWithoutATypeAndCloseCustomServicesInConfiguredOrder(Service request)
    {
        Assert.True(OpenGenericServiceBinder.TryBindOpenGenericTypedService(
            (IServiceWithType)request, ConfiguredWithCustomServices(), typeof(FirstAndSecond<>), out _, out var services));

        AssertClosedCustomServices(services);
    }

    [Theory]
    [MemberData(nameof(RequestsForCustomServices))]
    public void DelegateServicesSkipServicesWithoutATypeAndCloseCustomServicesInConfiguredOrder(Service request)
    {
        Assert.True(OpenGenericServiceBinder.TryBindOpenGenericDelegateService(
            (IServiceWithType)request, ConfiguredWithCustomServices(), (context, types, parameters) => types, out _, out var services));

        AssertClosedCustomServices(services);
    }

    public static IEnumerable<object[]> RequestsForCustomServices()
    {
        // Matches the configured typed service.
        yield return new object[] { new TypedService(typeof(ISecond<int>)) };

        // Matches the configured custom service, through its Equals.
        yield return new object[] { new TypedService(typeof(IFirst<int>)) };
        yield return new object[] { new CustomService(typeof(IFirst<int>)) };
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
    public void KeyedServiceWithAnEqualKeyIsClosedWithTheConfiguredKey()
    {
        object configuredKey = 1;
        object requestedKey = 1;

        Assert.True(OpenGenericServiceBinder.TryBindOpenGenericTypedService(
            new KeyedService(requestedKey, typeof(IFirst<int>)),
            new Service[] { new KeyedService(configuredKey, typeof(IFirst<>)) },
            typeof(FirstAndSecond<>),
            out _,
            out var services));

        Assert.Same(configuredKey, Assert.IsType<KeyedService>(Assert.Single(services)).ServiceKey);
    }

    [Fact]
    public void ImplementationParametersWithTheSameNameAreMappedByName()
    {
        // Both of Inner's parameters are named T, so both map by name to the service's first argument, and
        // Outer<int>.Inner<int> doesn't implement the requested Outer<int>.INested<string>.
        Assert.False(OpenGenericServiceBinder.TryBindOpenGenericTypedService(
            new TypedService(typeof(Outer<int>.INested<string>)),
            new Service[] { new TypedService(typeof(Outer<>.INested<>)) },
            typeof(Outer<>.Inner<>),
            out _,
            out _));
    }

    [Fact]
    public void ConstructorConstraintNeedsADeclaredPublicParameterlessConstructor()
    {
        var builder = new ContainerBuilder();
        builder.RegisterGeneric(typeof(NewHandler<>)).As(typeof(IFirst<>));
        var container = builder.Build();

        Assert.True(container.IsRegistered<IFirst<ParameterlessAndParameterized>>());
        Assert.False(container.IsRegistered<IFirst<OnlyProtectedParameterless>>());
        Assert.False(container.IsRegistered<IFirst<OnlyParameterized>>());
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

    private static Service[] ConfiguredWithCustomServices() => new Service[]
    {
        new PlainService(),
        new CustomService(typeof(IFirst<>)),
        new TypedService(typeof(ISecond<>)),
    };

    private static void AssertClosedCustomServices(Service[] services)
    {
        Assert.Collection(
            services,
            service => Assert.Equal(typeof(IFirst<int>), Assert.IsType<CustomService>(service).ServiceType),
            service => Assert.Equal(new TypedService(typeof(ISecond<int>)), service));
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

    /// <summary>
    /// A service that has no service type, which binding skips.
    /// </summary>
    private sealed class PlainService : Service
    {
        public override string Description => "Plain";

        public override bool Equals(object? obj) => obj is PlainService;

        public override int GetHashCode() => 0;
    }

    /// <summary>
    /// A service with its own type change, that also counts a typed service of the same type as equal.
    /// </summary>
    private sealed class CustomService : Service, IServiceWithType
    {
        public CustomService(Type serviceType)
        {
            ServiceType = serviceType;
        }

        public Type ServiceType
        {
            get;
        }

        public override string Description => "Custom " + ServiceType;

        public Service ChangeType(Type newType) => new CustomService(newType);

        public override bool Equals(object? obj) => obj is CustomService or TypedService && ((IServiceWithType)obj).ServiceType == ServiceType;

        public override int GetHashCode() => ServiceType.GetHashCode();
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

    private class NewHandler<T> : IFirst<T>
        where T : new()
    {
    }

    private class ParameterlessAndParameterized
    {
        public ParameterlessAndParameterized()
        {
        }

        public ParameterlessAndParameterized(int value)
        {
            _ = value;
        }
    }

    private class OnlyProtectedParameterless
    {
        protected OnlyProtectedParameterless()
        {
        }
    }

    private class OnlyParameterized
    {
        public OnlyParameterized(int value)
        {
            _ = value;
        }
    }

#pragma warning disable CS0693 // Inner deliberately reuses the name of Outer's parameter.
    private class Outer<T>
    {
        public interface INested<TNested>
        {
        }

        public class Inner<T> : INested<T>
        {
        }
    }
#pragma warning restore CS0693
}
