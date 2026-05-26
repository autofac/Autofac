// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Autofac.Test.Core;

public class KeyedServiceKeyParameterTests
{
    [Fact]
    public void Ctor_NullServiceKey()
    {
        Assert.Throws<ArgumentNullException>(() => new KeyedServiceKeyParameter(null!));
    }

    [Fact]
    public void ServiceKey_ReturnsCtorValue()
    {
        var parameter = new KeyedServiceKeyParameter("expected");

        Assert.Equal("expected", parameter.ServiceKey);
    }

    [Fact]
    public void CanSupplyValue_NullParameter()
    {
        var parameter = new KeyedServiceKeyParameter("key");

        Assert.Throws<ArgumentNullException>(
            () => parameter.CanSupplyValue(null!, new ContainerBuilder().Build(), out _));
    }

    [Fact]
    public void CanSupplyValue_NullContext()
    {
        var parameterInfo = typeof(NeedsConstructorKey).GetConstructors().Single().GetParameters().Single();
        var parameter = new KeyedServiceKeyParameter("key");

        Assert.Throws<ArgumentNullException>(
            () => parameter.CanSupplyValue(parameterInfo, null!, out _));
    }

    [Fact]
    public void CanSupplyValue_AttributeMissing()
    {
        var parameterInfo = typeof(PlainService).GetConstructors().Single().GetParameters().Single();
        var parameter = new KeyedServiceKeyParameter("key");
        using var context = new ContainerBuilder().Build();

        var result = parameter.CanSupplyValue(parameterInfo, context, out var valueProvider);

        Assert.False(result);
        Assert.Null(valueProvider);
    }

    [Fact]
    public void CanSupplyValue_AttributePresentReturnsValue()
    {
        var parameterInfo = typeof(NeedsConstructorKey).GetConstructors().Single().GetParameters().Single();
        var parameter = new KeyedServiceKeyParameter("expected");
        using var context = new ContainerBuilder().Build();

        var result = parameter.CanSupplyValue(parameterInfo, context, out var valueProvider);

        Assert.True(result);
        Assert.NotNull(valueProvider);
        Assert.Equal("expected", valueProvider());
    }

    private sealed class PlainService
    {
        public PlainService(object value)
        {
        }
    }

    private sealed class NeedsConstructorKey
    {
        public NeedsConstructorKey([ServiceKey] object key)
        {
            Key = key;
        }

        public object Key
        {
            get;
        }
    }
}
