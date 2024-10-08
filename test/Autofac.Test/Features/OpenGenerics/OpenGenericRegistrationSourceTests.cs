﻿// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Reflection;
using Autofac.Builder;
using Autofac.Core;
using Autofac.Core.Resolving.Pipeline;
using Autofac.Features.OpenGenerics;
using Autofac.Test.Util;

namespace Autofac.Test.Features.OpenGenerics;

public class OpenGenericRegistrationSourceTests
{
    private interface I<T>
    {
    }

    private class A1<T> : DisposeTracker, I<T>
    {
    }

    [Fact]
    public void GeneratesActivatorAndCorrectServices()
    {
        var g = ConstructSource(typeof(A1<>), typeof(I<>));

        var r = g
            .RegistrationsFor(new TypedService(typeof(I<int>)), s => null)
            .Single();

        Assert.Equal(
            typeof(I<int>),
            r.Services.Cast<TypedService>().Single().ServiceType);

        var container = new ContainerBuilder().Build();
        var invoker = r.Activator.GetPipelineInvoker(container.ComponentRegistry);

        var activatedInstance = invoker(container, Factory.NoParameters);
        Assert.IsType<A1<int>>(activatedInstance);
    }

    private class AWithNew<T> : I<T>
        where T : new()
    {
    }

    [Fact]
    public void DoesNotGenerateActivatorWhenConstructorConstraintBroken()
    {
        Assert.False(CanGenerateActivatorForI<string>(typeof(AWithNew<>)));
    }

    private class PWithNew
    {
    }

    [Fact]
    public void GeneratesActivatorWhenConstructorConstraintMet()
    {
        Assert.True(CanGenerateActivatorForI<PWithNew>(typeof(AWithNew<>)));
    }

    private class AWithDisposable<T> : I<T>
        where T : IDisposable
    {
    }

    [Fact]
    public void DoesNotGenerateActivatorWhenTypeConstraintBroken()
    {
        Assert.False(CanGenerateActivatorForI<string>(typeof(AWithDisposable<>)));
    }

    [Fact]
    public void GeneratesActivatorWhenTypeConstraintMet()
    {
        Assert.True(CanGenerateActivatorForI<DisposeTracker>(typeof(AWithDisposable<>)));
    }

    private class AWithClass<T> : I<T>
        where T : class
    {
    }

    [Fact]
    public void DoesNotGenerateActivatorWhenClassConstraintBroken()
    {
        Assert.False(CanGenerateActivatorForI<int>(typeof(AWithClass<>)));
    }

    [Fact]
    public void GeneratesActivatorWhenClassConstraintMet()
    {
        Assert.True(CanGenerateActivatorForI<string>(typeof(AWithClass<>)));
    }

    private class AWithValue<T> : I<T>
        where T : struct
    {
    }

    [Fact]
    public void DoesNotGenerateActivatorWhenValueConstraintBroken()
    {
        Assert.False(CanGenerateActivatorForI<string>(typeof(AWithValue<>)));
    }

    [Fact]
    public void GeneratesActivatorWhenValueConstraintMet()
    {
        Assert.True(CanGenerateActivatorForI<int>(typeof(AWithValue<>)));
    }

    private static bool CanGenerateActivatorForI<TClosing>(Type implementor)
    {
        var g = ConstructSource(implementor, typeof(I<>));

        var rs = g.RegistrationsFor(new TypedService(typeof(I<TClosing>)), s => null);

        return rs.Count() == 1;
    }

    private interface ITwoParams<T, TU>
    {
    }

    private class TwoParams<T, TU> : ITwoParams<T, TU>
    {
    }

    [Fact]
    public void SupportsMultipleGenericParameters()
    {
        var g = ConstructSource(typeof(TwoParams<,>));

        var rs = g.RegistrationsFor(new TypedService(typeof(TwoParams<int, string>)), s => null);

        Assert.Single(rs);
    }

    [Fact]
    public void SupportsMultipleGenericParametersMappedFromService()
    {
        var g = ConstructSource(typeof(TwoParams<,>), typeof(ITwoParams<,>));

        var rs = g.RegistrationsFor(new TypedService(typeof(ITwoParams<int, string>)), s => null);

        Assert.Single(rs);
    }

    private interface IEntity<TId>
    {
    }

    private class EntityOfInt : IEntity<int>
    {
    }

    private class Repository<T, TId>
        where T : IEntity<TId>
    {
    }

    [Fact]
    public void SupportsCodependentTypeConstraints()
    {
        var g = ConstructSource(typeof(Repository<,>));

        var rs = g.RegistrationsFor(new TypedService(typeof(Repository<EntityOfInt, int>)), s => null);

        Assert.Single(rs);
    }

    private interface IHaveNoParameters
    {
    }

    private interface IHaveOneParameter<T>
    {
    }

    private interface IHaveTwoParameters<T, TU>
    {
    }

    private interface IHaveThreeParameters<T, TU, TV>
    {
    }

    private class HaveTwoParameters<T, TU> : IHaveThreeParameters<T, TU, TU>, IHaveTwoParameters<T, T>, IHaveOneParameter<T>, IHaveNoParameters
    {
    }

    private interface IUnrelated
    {
    }

    [Fact]
    public void RejectsServicesWithoutTypeParameters()
    {
        Assert.Throws<ArgumentException>(() => ConstructSource(typeof(HaveTwoParameters<,>), typeof(IHaveNoParameters)));
    }

    [Fact]
    public void RejectsServicesNotInTheInheritanceChain()
    {
        Assert.Throws<ArgumentException>(() => ConstructSource(typeof(HaveTwoParameters<,>), typeof(IUnrelated)));
    }

    [Fact]
    public void IgnoresServicesWithoutEnoughParameters()
    {
        Assert.False(SourceCanSupply<IHaveOneParameter<int>>(typeof(HaveTwoParameters<,>)));
    }

    [Fact]
    public void IgnoresServicesThatDoNotSupplyAllParameters()
    {
        Assert.False(SourceCanSupply<IHaveTwoParameters<int, int>>(typeof(HaveTwoParameters<,>)));
    }

    [Fact]
    public void AcceptsServicesWithMoreParametersWhenAllImplementationParametersCovered()
    {
        Assert.True(SourceCanSupply<IHaveThreeParameters<int, string, string>>(typeof(HaveTwoParameters<,>)));
    }

    [Fact]
    public void IgnoresServicesWithMismatchedParameters()
    {
        Assert.True(!SourceCanSupply<IHaveThreeParameters<int, string, decimal>>(typeof(HaveTwoParameters<,>)));
    }

    private static bool SourceCanSupply<TClosedService>(Type component)
    {
        var service = typeof(TClosedService).GetGenericTypeDefinition();
        var source = ConstructSource(component, service);

        var closedServiceType = typeof(TClosedService);
        var registrations = source.RegistrationsFor(new TypedService(closedServiceType), s => Enumerable.Empty<ServiceRegistration>()).ToList();
        if (registrations.Count != 1)
        {
            return false;
        }

        var registration = registrations.Single();

        var container = new ContainerBuilder().Build();

        var invoker = registration.Activator.GetPipelineInvoker(container.ComponentRegistry);

        var instance = invoker(container, Factory.NoParameters);

        Assert.True(closedServiceType.GetTypeInfo().IsAssignableFrom(instance.GetType().GetTypeInfo()));
        return true;
    }

    private static OpenGenericRegistrationSource ConstructSource(Type component, Type service = null)
    {
        return new OpenGenericRegistrationSource(
            new RegistrationData(new TypedService(service ?? component)),
            new ResolvePipelineBuilder(PipelineType.Registration),
            new ReflectionActivatorData(component));
    }
}
