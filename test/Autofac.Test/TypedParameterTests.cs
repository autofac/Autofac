// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Linq;
using System.Reflection;
using Xunit;

namespace Autofac.Test
{
    public class TypedParameterTests
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
        public void MatchesIdenticallyTypedParameter()
        {
            var param = AParamOfCConstructor();

            var typedParam = new TypedParameter(typeof(A), new A());

            Assert.True(typedParam.CanSupplyValue(param, Factory.CreateEmptyContainer(), out Func<object> vp));
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
        public void DoesNotMatchPolymorphicallyTypedParameter()
        {
            var param = AParamOfCConstructor();

            var typedParam = new TypedParameter(typeof(B), new B());

            Assert.False(typedParam.CanSupplyValue(param, Factory.CreateEmptyContainer(), out Func<object> vp));
        }

        [Fact]
        public void DoesNotMatchUnrelatedParameter()
        {
            var param = AParamOfCConstructor();

            var typedParam = new TypedParameter(typeof(string), "Yo!");

            Assert.False(typedParam.CanSupplyValue(param, Factory.CreateEmptyContainer(), out Func<object> vp));
        }

        [Fact]
        public void FromWorksJustLikeTheConstructor()
        {
            var param = TypedParameter.From(new B());
            Assert.Same(typeof(B), param.Type);
        }
    }
}
