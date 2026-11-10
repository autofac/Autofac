// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Reflection;
using Autofac.Core;
using Autofac.Features.Decorators;

namespace Autofac.Test.Features.Decorators;

public class CompatibleServiceParameterTests
{
    private static readonly ParameterInfo _serviceParameter = typeof(Decorator).GetConstructors()[0].GetParameters()[0];

    private interface IService
    {
    }

    [Fact]
    public void CanSupplyValueRequiresParameterInfo()
    {
        var parameter = new CompatibleServiceParameter(typeof(IService), new Implementation());

        var exception = Assert.Throws<ArgumentNullException>(
            () => parameter.CanSupplyValue(null!, Substitute.For<IComponentContext>(), out _));

        Assert.Equal("pi", exception.ParamName);
    }

    [Fact]
    public void CanSupplyValueRequiresContext()
    {
        var parameter = new CompatibleServiceParameter(typeof(IService), new Implementation());

        var exception = Assert.Throws<ArgumentNullException>(
            () => parameter.CanSupplyValue(_serviceParameter, null!, out _));

        Assert.Equal("context", exception.ParamName);
    }

    [Fact]
    public void SuppliesTheInstanceToACompatibleParameter()
    {
        var instance = new Implementation();
        var parameter = new CompatibleServiceParameter(typeof(IService), instance);

        Assert.True(parameter.CanSupplyValue(_serviceParameter, Substitute.For<IComponentContext>(), out var valueProvider));
        Assert.Same(instance, valueProvider());
    }

    private sealed class Implementation : IService
    {
    }

    private sealed class Decorator : IService
    {
        public Decorator(IService decorated)
        {
            Decorated = decorated;
        }

        public IService Decorated
        {
            get;
        }
    }
}
