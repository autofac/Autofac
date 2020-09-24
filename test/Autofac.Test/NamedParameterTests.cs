// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Linq;
using System.Reflection;
using Autofac.Core;
using Xunit;

namespace Autofac.Test
{
    public class NamedParameterTests
    {
        public class A
        {
        }

        public class B : A
        {
        }

        public class C
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

            Assert.True(namedParam.CanSupplyValue(param, Factory.CreateEmptyContainer(), out Func<object> vp));
        }

        private static System.Reflection.ParameterInfo AParamOfCConstructor()
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

            Assert.False(namedParam.CanSupplyValue(param, Factory.CreateEmptyContainer(), out Func<object> vp));
        }
    }
}
