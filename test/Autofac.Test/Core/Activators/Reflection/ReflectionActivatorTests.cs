﻿// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Reflection;
using Autofac.Core;
using Autofac.Core.Activators.Reflection;
using Autofac.Test.Scenarios.ConstructorSelection;
using Autofac.Test.Scenarios.Dependencies;

namespace Autofac.Test.Core.Activators.Reflection;

public class ReflectionActivatorTests
{
    [Fact]
    public void Pipeline_DependenciesNotAvailable_ThrowsException()
    {
        using var target = Factory.CreateReflectionActivator(typeof(DependsByCtor));

        using var container = Factory.CreateEmptyContainer();
        var invoker = target.GetPipelineInvoker(container.ComponentRegistry);

        var ex = Assert.Throws<DependencyResolutionException>(
            () => invoker(container, Factory.NoParameters));

        // I.e. the type of the missing dependency.
        Assert.Contains(nameof(DependsByProp), ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Pipeline_ResolvesConstructorDependencies()
    {
        var o = new object();
        const string s = "s";

        var builder = new ContainerBuilder();
        builder.RegisterInstance(o);
        builder.RegisterInstance(s);
        var container = builder.Build();

        using var target = Factory.CreateReflectionActivator(typeof(Dependent));
        var invoker = target.GetPipelineInvoker(container.ComponentRegistry);
        var instance = invoker(container, Factory.NoParameters);

        Assert.NotNull(instance);
        Assert.IsType<Dependent>(instance);

        var dependent = (Dependent)instance;

        Assert.Same(o, dependent.TheObject);
        Assert.Same(s, dependent.TheString);
    }

    [Fact]
    public void Pipeline_ReturnsInstanceOfTargetType()
    {
        using var target = Factory.CreateReflectionActivator(typeof(object));
        using var container = Factory.CreateEmptyContainer();
        var invoker = target.GetPipelineInvoker(container.ComponentRegistry);
        var instance = invoker(container, Factory.NoParameters);

        Assert.NotNull(instance);
        Assert.IsType<object>(instance);
    }

    [Fact]
    public void ByDefault_ChoosesConstructorWithMostResolvableParameters()
    {
        var builder = new ContainerBuilder();
        builder.RegisterType(typeof(object));
        var container = builder.Build();

        using var target = Factory.CreateReflectionActivator(typeof(MultipleConstructors));
        var invoker = target.GetPipelineInvoker(container.ComponentRegistry);
        var instance = invoker(container, Factory.NoParameters);

        Assert.NotNull(instance);
        Assert.IsType<MultipleConstructors>(instance);
    }

    [Fact]
    public void ByDefault_ChoosesMostParameterisedConstructor()
    {
        var parameters = new Parameter[]
        {
                new NamedParameter("i", 1),
                new NamedParameter("s", "str"),
        };

        using var target = Factory.CreateReflectionActivator(typeof(ThreeConstructors), parameters);
        using var container = Factory.CreateEmptyContainer();
        var invoker = target.GetPipelineInvoker(container.ComponentRegistry);
        var instance = invoker(container, Factory.NoParameters);

        Assert.NotNull(instance);
        Assert.IsType<ThreeConstructors>(instance);

        var typedInstance = (ThreeConstructors)instance;

        Assert.Equal(2, typedInstance.CalledConstructorParameterCount);
    }

    [Fact]
    public void CanResolveConstructorsWithGenericParameters()
    {
        using var activator = Factory.CreateReflectionActivator(typeof(WithGenericCtor<string>));
        var parameters = new Parameter[] { new NamedParameter("t", "Hello") };
        using var container = Factory.CreateEmptyContainer();
        var invoker = activator.GetPipelineInvoker(container.ComponentRegistry);
        var instance = invoker(container, parameters);
        Assert.IsType<WithGenericCtor<string>>(instance);
    }

    [Fact]
    public void Constructor_DoesNotAcceptNullFinder()
    {
        Assert.Throws<ArgumentNullException>(
            () => new ReflectionActivator(
                typeof(object),
                null,
                Mocks.GetConstructorSelector(),
                Factory.NoParameters,
                Factory.NoProperties));
    }

    [Fact]
    public void Constructor_DoesNotAcceptNullParameters()
    {
        Assert.Throws<ArgumentNullException>(
            () => new ReflectionActivator(
                typeof(object),
                Mocks.GetConstructorFinder(),
                Mocks.GetConstructorSelector(),
                null,
                Factory.NoProperties));
    }

    [Fact]
    public void Constructor_DoesNotAcceptNullProperties()
    {
        Assert.Throws<ArgumentNullException>(
            () => new ReflectionActivator(
                typeof(object),
                Mocks.GetConstructorFinder(),
                Mocks.GetConstructorSelector(),
                Factory.NoParameters,
                null));
    }

    [Fact]
    public void Constructor_DoesNotAcceptNullSelector()
    {
        Assert.Throws<ArgumentNullException>(
            () => new ReflectionActivator(
                typeof(object),
                Mocks.GetConstructorFinder(),
                null,
                Factory.NoParameters,
                Factory.NoProperties));
    }

    [Fact]
    public void Constructor_DoesNotAcceptNullType()
    {
        Assert.Throws<ArgumentNullException>(
            () => new ReflectionActivator(
                null,
                Mocks.GetConstructorFinder(),
                Mocks.GetConstructorSelector(),
                Factory.NoParameters,
                Factory.NoProperties));
    }

    [Fact]
    public void NonPublicConstructorsIgnored()
    {
        using var target = Factory.CreateReflectionActivator(typeof(InternalDefaultConstructor));

        // Constructor finding happens at pipeline construction; not when the pipeline is invoked.
        var invoker = target.GetPipelineInvoker(Factory.CreateEmptyComponentRegistry());

        var dx = Assert.Throws<DependencyResolutionException>(() =>
            invoker(Factory.CreateEmptyContainer(), Factory.NoParameters));

        Assert.Contains(typeof(InternalDefaultConstructor).Name, dx.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void PropertiesWithPrivateSetters_AreIgnored()
    {
        var setters = new Parameter[] { new NamedPropertyParameter("P", 1) };
        using var activator = Factory.CreateReflectionActivator(typeof(PrivateSetProperty), Factory.NoParameters, setters);
        using var container = Factory.CreateEmptyContainer();
        var invoker = activator.GetPipelineInvoker(container.ComponentRegistry);
        var instance = invoker(container, Factory.NoParameters);
        Assert.IsType<PrivateSetProperty>(instance);
    }

    [Fact]
    public void ProvidedParameters_OverrideThoseInContext()
    {
        var containedInstance = new object();

        var builder = new ContainerBuilder();
        builder.RegisterInstance(containedInstance);
        var container = builder.Build();

        var parameterInstance = new object();
        var parameters = new Parameter[] { new NamedParameter("p", parameterInstance) };

        using var target = Factory.CreateReflectionActivator(typeof(AcceptsObjectParameter), parameters);

        var invoker = target.GetPipelineInvoker(container.ComponentRegistry);

        var instance = (AcceptsObjectParameter)invoker(container, Factory.NoParameters);

        Assert.Same(parameterInstance, instance.P);
        Assert.NotSame(containedInstance, instance.P);
    }

    [Fact]
    public void SetsMultipleConfiguredProperties()
    {
        const int p1 = 1;
        const int p2 = 2;
        var properties = new[]
        {
            new NamedPropertyParameter("P1", p1),
            new NamedPropertyParameter("P2", p2),
        };
        using var target = Factory.CreateReflectionActivator(typeof(R), Enumerable.Empty<Parameter>(), properties);
        using var container = Factory.CreateEmptyContainer();
        var invoker = target.GetPipelineInvoker(container.ComponentRegistry);
        var instance = (R)invoker(container, Enumerable.Empty<Parameter>());
        Assert.Equal(1, instance.P1);
        Assert.Equal(2, instance.P2);
    }

    [Fact]
    public void ThrowsWhenNoPublicConstructors()
    {
        using var target = Factory.CreateReflectionActivator(typeof(NoPublicConstructor));
        var dx = Assert.Throws<NoConstructorsFoundException>(
            () => target.GetPipelineInvoker(Factory.CreateEmptyComponentRegistry()));

        Assert.Contains(typeof(NoPublicConstructor).FullName, dx.Message, StringComparison.Ordinal);
        Assert.Equal(typeof(NoPublicConstructor), dx.OffendingType);
    }

    [Fact]
    public void WhenNullReferenceTypeParameterSupplied_ItIsPassedToTheComponent()
    {
        var parameters = new Parameter[] { new NamedParameter("p", null) };

        using var target = Factory.CreateReflectionActivator(typeof(AcceptsObjectParameter), parameters);

        using var container = Factory.CreateEmptyContainer();
        var invoker = target.GetPipelineInvoker(container.ComponentRegistry);

        var instance = invoker(container, Factory.NoParameters);

        Assert.NotNull(instance);
        Assert.IsType<AcceptsObjectParameter>(instance);

        var typedInstance = (AcceptsObjectParameter)instance;

        Assert.Null(typedInstance.P);
    }

    [Fact]
    public void WhenReferenceTypeParameterSupplied_ItIsProvidedToTheComponent()
    {
        var p = new object();
        var parameters = new Parameter[] { new NamedParameter("p", p) };

        using var target = Factory.CreateReflectionActivator(typeof(AcceptsObjectParameter), parameters);

        using var container = Factory.CreateEmptyContainer();
        var invoker = target.GetPipelineInvoker(container.ComponentRegistry);

        var instance = invoker(container, Factory.NoParameters);

        Assert.NotNull(instance);
        Assert.IsType<AcceptsObjectParameter>(instance);

        var typedInstance = (AcceptsObjectParameter)instance;

        Assert.Equal(p, typedInstance.P);
    }

    [Fact]
    public void WhenValueTypeParameterIsSuppliedWithNull_TheDefaultForTheValueTypeIsSet()
    {
        var parameters = new Parameter[] { new NamedParameter("i", null) };

        using var target = Factory.CreateReflectionActivator(typeof(AcceptsIntParameter), parameters);

        using var container = Factory.CreateEmptyContainer();
        var invoker = target.GetPipelineInvoker(container.ComponentRegistry);

        var instance = invoker(container, Factory.NoParameters);

        Assert.NotNull(instance);
        Assert.IsType<AcceptsIntParameter>(instance);

        var typedInstance = (AcceptsIntParameter)instance;

        Assert.Equal(0, typedInstance.I);
    }

    [Fact]
    public void WhenValueTypeParameterSupplied_ItIsPassedToTheComponent()
    {
        const int i = 42;
        var parameters = new Parameter[] { new NamedParameter("i", i) };

        using var target = Factory.CreateReflectionActivator(typeof(AcceptsIntParameter), parameters);

        using var container = Factory.CreateEmptyContainer();
        var invoker = target.GetPipelineInvoker(container.ComponentRegistry);

        var instance = invoker(container, Factory.NoParameters);

        Assert.NotNull(instance);
        Assert.IsType<AcceptsIntParameter>(instance);

        var typedInstance = (AcceptsIntParameter)instance;

        Assert.Equal(i, typedInstance.I);
    }

    [Fact]
    public void ConstructorSelectorCannotReturnInvalidBinding()
    {
        using var target = Factory.CreateReflectionActivator(typeof(ThreeConstructors), new MisbehavingConstructorSelector());

        using var container = Factory.CreateEmptyContainer();
        var invoker = target.GetPipelineInvoker(container.ComponentRegistry);

        Assert.Throws<InvalidOperationException>(() => invoker(container, Factory.NoParameters));
    }

    [Fact]
    public void CustomBinderNameIncludedInErrorMessage()
    {
        using var target = Factory.CreateReflectionActivator(typeof(InternalDefaultConstructor), new SimpleConstructorFinder());

        // Constructor finding happens at pipeline construction; not when the pipeline is invoked.
        var invoker = target.GetPipelineInvoker(Factory.CreateEmptyComponentRegistry());

        var dx = Assert.Throws<DependencyResolutionException>(() =>
            invoker(Factory.CreateEmptyContainer(), Factory.NoParameters));

        Assert.Contains(typeof(SimpleConstructorFinder).Name, dx.Message, StringComparison.Ordinal);
    }

    private class MisbehavingConstructorSelector : IConstructorSelector
    {
        public BoundConstructor SelectConstructorBinding(BoundConstructor[] constructorBindings, IEnumerable<Parameter> parameters)
        {
            return constructorBindings.First(x => !x.CanInstantiate);
        }
    }

    private class SimpleConstructorFinder : IConstructorFinder
    {
        public ConstructorInfo[] FindConstructors(Type targetType) => targetType.GetDeclaredPublicConstructors();
    }

    private class AcceptsIntParameter
    {
        public AcceptsIntParameter(int i)
        {
            I = i;
        }

        public int I { get; private set; }
    }

    private class AcceptsObjectParameter
    {
        public AcceptsObjectParameter(object p)
        {
            P = p;
        }

        public object P { get; private set; }
    }

    private class InternalDefaultConstructor
    {
        public InternalDefaultConstructor(int x)
        {
        }

        internal InternalDefaultConstructor()
        {
        }
    }

    private class NoPublicConstructor
    {
        internal NoPublicConstructor()
        {
        }
    }

    private class PrivateSetProperty
    {
        public int GetProperty { get; private set; }

        public int P { get; set; }
    }

    private class R
    {
        public int P1 { get; set; }

        public int P2 { get; set; }
    }

    private class ThreeConstructors
    {
        public ThreeConstructors()
        {
            CalledConstructorParameterCount = 0;
        }

        public ThreeConstructors(int i)
        {
            CalledConstructorParameterCount = 1;
        }

        public ThreeConstructors(int i, string s)
        {
            CalledConstructorParameterCount = 2;
        }

        public int CalledConstructorParameterCount { get; private set; }
    }

    private class WithGenericCtor<T>
    {
        public WithGenericCtor(T t)
        {
        }
    }
}
