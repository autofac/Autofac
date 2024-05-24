// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Reflection;

namespace Autofac.Test;

public class NamedParameterTests
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
    public void MatchesIdenticallyNamedParameter()
    {
        var param = AParamOfCConstructor();

        var namedParam = new NamedParameter("a", new A());

        using var container = Factory.CreateEmptyContainer();
        Assert.True(namedParam.CanSupplyValue(param, container, out Func<object> vp));
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
    public void DoesNotMatchDifferentlyNamedParameter()
    {
        var param = AParamOfCConstructor();

        var namedParam = new NamedParameter("b", new B());

        using var container = Factory.CreateEmptyContainer();
        Assert.False(namedParam.CanSupplyValue(param, container, out Func<object> vp));
    }
}
