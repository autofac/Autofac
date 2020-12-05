// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Xunit;

using InternalTypeExtensions = Autofac.Util.TypeExtensions;

namespace Autofac.Test.Util
{
    public class InternalTypeExtensionsTests
    {
        [Theory]
        [InlineData(typeof(OpenGenericInheritOpenGenericBase<>), typeof(Derived<>))]
        [InlineData(typeof(OpenGenericInheritOpenGenericBase<>), typeof(Base<>))]
        [InlineData(typeof(OpenGenericInheritOpenGenericBase<>), typeof(IChildInterface<>))]
        [InlineData(typeof(OpenGenericInheritOpenGenericBase<>), typeof(IChildInterface))]
        [InlineData(typeof(OpenGenericInheritOpenGenericBase<>), typeof(IParentsInterface<>))]
        [InlineData(typeof(OpenGenericInheritOpenGenericBase<>), typeof(IParentsInterface))]
        [InlineData(typeof(OpenGenericInheritNonGenericBase<>), typeof(Derived))]
        [InlineData(typeof(OpenGenericInheritNonGenericBase<>), typeof(Base))]
        public void IsOpenGenericTypeOf_Should_Return_True(Type openGenericType, Type typeToValidate)
        {
            Assert.True(InternalTypeExtensions.IsOpenGenericTypeOf(openGenericType, typeToValidate));
        }

        private class Base
        {
        }

        private class Base<T>
        {
        }

        private class Derived : Base
        {
        }

        private class Derived<T> : Base<T>
        {
        }

        private interface IParentsInterface
        {
        }

        private interface IParentsInterface<T>
        {
        }

        private interface IChildInterface : IParentsInterface
        {
        }

        private interface IChildInterface<T> : IParentsInterface<T>
        {
        }

        private class OpenGenericInheritNonGenericBase<T> : Derived, IChildInterface<int>
        {
        }

        private class OpenGenericInheritOpenGenericBase<T> : Derived<T>, IChildInterface<T>, IChildInterface
        {
        }
    }
}
