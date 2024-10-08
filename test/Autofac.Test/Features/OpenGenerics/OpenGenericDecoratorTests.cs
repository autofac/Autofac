﻿// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Autofac.Test.Features.OpenGenerics;

public class OpenGenericDecoratorTests
{
    private interface IService<T>
    {
        IService<T> Decorated { get; }
    }

    private class ImplementorA<T> : IService<T>
    {
        public IService<T> Decorated
        {
            get { return this; }
        }
    }

    private class ImplementorB<T> : IService<T>
    {
        public IService<T> Decorated
        {
            get { return this; }
        }
    }

    private class StringImplementor : IService<string>
    {
        public IService<string> Decorated
        {
            get { return this; }
        }
    }

    private abstract class Decorator<T> : IService<T>
    {
        protected Decorator(IService<T> decorated)
        {
            Decorated = decorated;
        }

        public IService<T> Decorated { get; }
    }

    private class DecoratorA<T> : Decorator<T>
    {
        public DecoratorA(IService<T> decorated)
            : base(decorated)
        {
        }
    }

    private class DecoratorB<T> : Decorator<T>
    {
        public DecoratorB(IService<T> decorated, string parameter)
            : base(decorated)
        {
            Parameter = parameter;
        }

        public string Parameter { get; }
    }

    private const string ParameterValue = "Abc";

    private readonly IContainer _container;

    public OpenGenericDecoratorTests()
    {
        // Order is:
        //    A -> B(p) -> ImplementorA
        //    A -> B(p) -> ImplementorB
        //    A -> B(p) -> StringImplementor (string only)
        var builder = new ContainerBuilder();

        builder.RegisterType<StringImplementor>()
            .Named<IService<string>>("implementor");

        builder.RegisterGeneric(typeof(ImplementorA<>))
            .Named("implementor", typeof(IService<>));

        builder.RegisterGeneric(typeof(ImplementorB<>))
            .Named("implementor", typeof(IService<>));

        builder.RegisterGenericDecorator(typeof(DecoratorB<>), typeof(IService<>), fromKey: "implementor", toKey: "b")
            .WithParameter("parameter", ParameterValue);

        builder.RegisterGenericDecorator(typeof(DecoratorA<>), typeof(IService<>), fromKey: "b");

        _container = builder.Build();
    }

    [Fact]
    public void CanResolveDecoratorService()
    {
        Assert.NotNull(_container.Resolve<IService<int>>());
    }

    [Fact]
    public void ThereAreTwoImplementorsOfInt()
    {
        Assert.Equal(2, _container.ResolveNamed<IEnumerable<IService<int>>>("implementor").Count());
    }

    [Fact]
    public void ThereAreTwoBLevelDecoratorsOfInt()
    {
        Assert.Equal(2, _container.ResolveNamed<IEnumerable<IService<int>>>("b").Count());
    }

    [Fact]
    public void TheDefaultImplementorIsTheLastRegistered()
    {
        var defaultChain = _container.Resolve<IService<int>>();
        Assert.IsType<ImplementorB<int>>(defaultChain.Decorated.Decorated);
    }

    [Fact]
    public void AllGenericImplemetationsAreDecorated()
    {
        Assert.Equal(2, _container.Resolve<IEnumerable<IService<int>>>().Count());
    }

    [Fact]
    public void WhenClosedImplementationsAreAvailableTheyAreDecorated()
    {
        Assert.Equal(3, _container.Resolve<IEnumerable<IService<string>>>().Count());
    }

    [Fact]
    public void TheFirstDecoratorIsA()
    {
        Assert.IsType<DecoratorA<int>>(_container.Resolve<IService<int>>());
    }

    [Fact]
    public void TheSecondDecoratorIsB()
    {
        Assert.IsType<DecoratorB<int>>(_container.Resolve<IService<int>>().Decorated);
    }

    [Fact]
    public void ParametersArePassedToB()
    {
        Assert.Equal(ParameterValue, ((DecoratorB<int>)_container.Resolve<IService<int>>().Decorated).Parameter);
    }
}
