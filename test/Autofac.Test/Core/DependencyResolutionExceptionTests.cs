// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Permissions;
using Autofac.Core;

namespace Autofac.Test.Core;

public class DependencyResolutionExceptionTests
{
    private class A
    {
        public const string Message = "This is the original exception.";

        public A()
        {
            throw new InvalidOperationException(Message);
        }
    }

    private class B
    {
        public B(A a)
        {
        }
    }

    private class C
    {
        public C(B b)
        {
        }
    }

    [Fact]
    public void ExceptionMessageUnwrapsNestedResolutionFailures()
    {
        var builder = new ContainerBuilder();
        builder.RegisterType<A>();
        builder.Register(c => new B(c.Resolve<A>()));
        builder.RegisterType<C>();

        Exception ex;
        using (var container = builder.Build())
        {
            ex = Assert.Throws<DependencyResolutionException>(() => container.Resolve<C>());
        }

        var n = GetType().FullName;
        Assert.Equal($"An exception was thrown while activating {n}+C -> λ:{n}+B -> {n}+A.", ex.Message);

        var inner = ex.InnerException;
        Assert.IsType<DependencyResolutionException>(inner);
        Assert.Equal("An exception was thrown while invoking the constructor 'Void .ctor()' on type 'A'.", inner.Message);

        Assert.IsType<InvalidOperationException>(inner.InnerException);
        Assert.Equal(A.Message, inner.InnerException.Message);
    }
}
