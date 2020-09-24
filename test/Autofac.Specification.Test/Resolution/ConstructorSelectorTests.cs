// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Xunit;

namespace Autofac.Specification.Test.Resolution
{
    public class ConstructorSelectorTests
    {
        public static IEnumerable<object[]> CanSpecifyConstructorByParameterType_Data()
        {
            yield return new object[]
            {
                new[] { typeof(A1) },
                1,
            };

            yield return new object[]
            {
                new[] { typeof(A1), typeof(A2) },
                2,
            };
        }

        [Theory]
        [MemberData(nameof(CanSpecifyConstructorByParameterType_Data))]
        public void CanSpecifyConstructorByParameterType(Type[] selector, int expectedConstructor)
        {
            var cb = new ContainerBuilder();
            cb.RegisterType<A1>();
            cb.RegisterType<A2>();
            cb.RegisterType<MultipleConstructors>().UsingConstructor(selector);

            var c = cb.Build();
            var result = c.Resolve<MultipleConstructors>();

            Assert.NotNull(result);
            Assert.Equal(expectedConstructor, result.CalledCtor);
        }

        public static IEnumerable<object[]> CanSpecifyConstructorByExpression_Data()
        {
            yield return new object[]
            {
                (Expression<Func<MultipleConstructors>>)(() => new MultipleConstructors(default, default)),
                2
            };

            yield return new object[]
            {
                (Expression<Func<MultipleConstructors>>)(() => new MultipleConstructors(default)),
                1
            };
        }

        [Theory]
        [MemberData(nameof(CanSpecifyConstructorByExpression_Data))]
        public void CanSpecifyConstructorByExpression(Expression<Func<MultipleConstructors>> selector, int expectedConstructor)
        {
            var cb = new ContainerBuilder();
            cb.RegisterType<A1>();
            cb.RegisterType<A2>();

            cb.RegisterType<MultipleConstructors>().UsingConstructor(selector);

            var c = cb.Build();
            var result = c.Resolve<MultipleConstructors>();

            Assert.NotNull(result);
            Assert.Equal(expectedConstructor, result.CalledCtor);
        }

        public class A1
        {
        }

        public class A2
        {
        }

        public class MultipleConstructors
        {
            public MultipleConstructors(A1 a1)
            {
                CalledCtor = 1;
            }

            public MultipleConstructors(A1 a1, A2 a2)
            {
                CalledCtor = 2;
            }

            public MultipleConstructors(A1 a1, A2 a2, string s1)
            {
                CalledCtor = 3;
            }

            public int CalledCtor { get; private set; }
        }
    }
}
