// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Reflection;
using Autofac.Core;
using Autofac.Core.Activators.Reflection;

namespace Autofac.Test.Core.Activators.Reflection;

public class ConstructorBinderTests
{
    private class ThrowsInCtor
    {
        public const string Message = "Message";

        public ThrowsInCtor()
        {
            throw new InvalidOperationException(Message);
        }
    }

    private class ServiceWithInParameter
    {
        public ServiceWithInParameter(in int input)
        {
        }
    }

    private class CtorWithDoubleParam
    {
        public double Value
        {
            get;
        }

        public CtorWithDoubleParam(double value)
        {
            Value = value;
        }
    }

    public enum Foo
    {
        A,
        B,
    }

    private class CtorWithInt
    {
        public int Value
        {
            get;
        }

        public CtorWithInt(int value)
        {
            Value = value;
        }
    }

    private class Dependency
    {
    }

    private class Missing
    {
    }

    private class CtorWithDependency
    {
        public CtorWithDependency(Dependency dependency)
        {
            Dependency = dependency;
        }

        public Dependency Dependency
        {
            get;
        }
    }

    private class CtorWithDependencyAndMissing
    {
        public CtorWithDependencyAndMissing(Dependency dependency, Missing missing)
        {
        }
    }

    private class CountingAutowiringParameter : AutowiringParameter
    {
        public int Calls
        {
            get; private set;
        }

        public override bool CanSupplyValue(ParameterInfo pi, IComponentContext context, [NotNullWhen(returnValue: true)] out Func<object?>? valueProvider)
        {
            Calls++;
            return base.CanSupplyValue(pi, context, out valueProvider);
        }
    }

    [Fact]
    public void WhenAnExceptionIsThrownFromAConstructor_TheInnerExceptionIsWrapped()
    {
        var ci = typeof(ThrowsInCtor).GetTypeInfo().DeclaredConstructors.Single();
        var cpb = new ConstructorBinder(ci).Bind(Enumerable.Empty<Parameter>(), new ContainerBuilder().Build());
        var dx = Assert.Throws<DependencyResolutionException>(() =>
            cpb.Instantiate());

        Assert.Contains(nameof(ThrowsInCtor), dx.Message, StringComparison.Ordinal);
        Assert.NotNull(dx.InnerException);
        Assert.Equal(ThrowsInCtor.Message, dx.InnerException.Message);
    }

    [Fact]
    public void WhenPrimitiveTypeIsProvidedForPrimitiveParameterConversionWillBeAttempted()
    {
        var ci = typeof(CtorWithDoubleParam).GetTypeInfo().DeclaredConstructors.Single();
        var cpb = new ConstructorBinder(ci).Bind(new[] { new PositionalParameter(0, 1), }, new ContainerBuilder().Build());
        var instance = (CtorWithDoubleParam)cpb.Instantiate();

        Assert.Equal(1d, instance.Value);
    }

    [Fact]
    public void WhenEnumTypeIsProvidedForIntParameterConversionWillBeAttempted()
    {
        var ci = typeof(CtorWithInt).GetTypeInfo().DeclaredConstructors.Single();
        var cpb = new ConstructorBinder(ci).Bind(new[] { new PositionalParameter(0, Foo.B), }, new ContainerBuilder().Build());
        var instance = (CtorWithInt)cpb.Instantiate();

        Assert.Equal(1, instance.Value);
    }

    [Fact]
    public void ParametersCanBeProvidedAsAnyEnumerable()
    {
        var ci = typeof(CtorWithInt).GetTypeInfo().DeclaredConstructors.Single();
        var parameters = new List<Parameter> { new PositionalParameter(0, 42) };
        var cpb = new ConstructorBinder(ci).Bind(parameters, new ContainerBuilder().Build());
        var instance = (CtorWithInt)cpb.Instantiate();

        Assert.Equal(42, instance.Value);
    }

    [Fact]
    public void AutowiringParameterSubclassIsStillAsked()
    {
        var builder = new ContainerBuilder();
        builder.RegisterType<Dependency>();
        var container = builder.Build();
        var parameter = new CountingAutowiringParameter();
        var ci = typeof(CtorWithDependency).GetTypeInfo().DeclaredConstructors.Single();

        var instance = (CtorWithDependency)new ConstructorBinder(ci).Bind(new Parameter[] { parameter }, container).Instantiate();

        Assert.Equal(1, parameter.Calls);
        Assert.NotNull(instance.Dependency);
    }

    [Fact]
    public void AutowiredArgumentsAreResolvedOnlyWhenInstantiated()
    {
        var resolved = 0;
        var builder = new ContainerBuilder();
        builder.Register(c =>
        {
            resolved++;
            return new Dependency();
        });
        var container = builder.Build();
        var parameters = new Parameter[] { new AutowiringParameter() };

        var cannotBind = typeof(CtorWithDependencyAndMissing).GetTypeInfo().DeclaredConstructors.Single();
        Assert.False(new ConstructorBinder(cannotBind).Bind(parameters, container).CanInstantiate);
        Assert.Equal(0, resolved);

        var canBind = typeof(CtorWithDependency).GetTypeInfo().DeclaredConstructors.Single();
        var bound = new ConstructorBinder(canBind).Bind(parameters, container);
        Assert.Equal(0, resolved);

        bound.Instantiate();
        Assert.Equal(1, resolved);
    }

    [Fact]
    public void ForBindSuccessInstantiatesWithValueRetrievers()
    {
        var binder = new ConstructorBinder(typeof(CtorWithInt).GetTypeInfo().DeclaredConstructors.Single());
        var bound = BoundConstructor.ForBindSuccess(binder, args => new CtorWithInt((int)args[0]!), new Func<object?>[] { () => 42 });

        var instance = (CtorWithInt)bound.Instantiate();

        Assert.Equal(42, instance.Value);
    }

    [Fact]
    public void WhenUsingByRefParameterExceptionIsThrownForIllegalParameter() // added for issue 1126
    {
        var constructorInfo = typeof(ServiceWithInParameter).GetTypeInfo().DeclaredConstructors.Single();
        var constructorBinder = new ConstructorBinder(constructorInfo);
        var boundConstructor = constructorBinder.Bind(new[] { new PositionalParameter(0, 111) }, new ContainerBuilder().Build());
        Assert.False(boundConstructor.CanInstantiate);
        Assert.Throws<InvalidOperationException>(() => boundConstructor.Instantiate());
    }
}
