// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Autofac.Specification.Test.Resolution.Graph1;
using Xunit;

namespace Autofac.Specification.Test.Resolution
{
    public class ComplexGraphTests
    {
        [Fact]
        public void CanCorrectlyBuildGraph1()
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<A1>().SingleInstance();
            builder.RegisterType<CD1>().As<IC1, ID1>().SingleInstance();
            builder.RegisterType<E1>().SingleInstance();
            builder.Register(ctr => new B1(ctr.Resolve<A1>()));

            var target = builder.Build();

            var e = target.Resolve<E1>();
            var a = target.Resolve<A1>();
            var b = target.Resolve<B1>();
            var c = target.Resolve<IC1>();
            var d = target.Resolve<ID1>();

            Assert.IsType<CD1>(c);
            var cd = (CD1)c;

            Assert.Same(a, b.A);
            Assert.Same(a, cd.A);
            Assert.NotSame(b, cd.B);
            Assert.Same(c, e.C);
            Assert.NotSame(b, e.B);
            Assert.NotSame(e.B, cd.B);
        }
    }
}
