// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Reflection;

namespace Autofac.Test;

public class TypedParameterTests
{
    private class A
    {
    }

    private class B : A
    {
    }

    private class C
    {
        public C(A a)
        {
        }
    }

    [Fact]
    public void MatchesIdenticallyTypedParameter()
    {
        var param = AParamOfCConstructor();

        var typedParam = new TypedParameter(typeof(A), new A());

        using var container = Factory.CreateEmptyContainer();

        Assert.True(typedParam.CanSupplyValue(param, container, out Func<object> vp));
    }

    private static ParameterInfo AParamOfCConstructor()
    {
        var param = typeof(C)
            .GetTypeInfo()
            .DeclaredConstructors
            .Single()
            .GetParameters()
            .First();
        return param;
    }

    [Fact]
    public void DoesNotMatchPolymorphicallyTypedParameter()
    {
        var param = AParamOfCConstructor();

        var typedParam = new TypedParameter(typeof(B), new B());

        using var container = Factory.CreateEmptyContainer();
        Assert.False(typedParam.CanSupplyValue(param, container, out Func<object> vp));
    }

    [Fact]
    public void DoesNotMatchUnrelatedParameter()
    {
        var param = AParamOfCConstructor();

        var typedParam = new TypedParameter(typeof(string), "Yo!");

        using var container = Factory.CreateEmptyContainer();
        Assert.False(typedParam.CanSupplyValue(param, container, out Func<object> vp));
    }

    [Fact]
    public void FromWorksJustLikeTheConstructor()
    {
        var param = TypedParameter.From(new B());
        Assert.Same(typeof(B), param.Type);
    }
}
