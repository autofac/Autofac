// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Linq.Expressions;
using System.Reflection;
using Autofac.Core;
using Autofac.Core.Activators.Reflection;
using Xunit;

namespace Autofac.Test.Builder
{
    public class ReflectiveRegistrationBuilderTests
    {
        [Fact]
        public void ExposesImplementationType()
        {
            var cb = new ContainerBuilder();
            cb.RegisterType(typeof(A1)).As<object>();
            var container = cb.Build();
            Assert.True(container.ComponentRegistry.TryGetRegistration(new TypedService(typeof(object)), out IComponentRegistration cr));
            Assert.Equal(typeof(A1), cr.Activator.LimitType);
        }

        [Fact]
        public void FindConstructorsWith_CustomFinderProvided_AddedToRegistration()
        {
            var cb = new ContainerBuilder();
            var constructorFinder = Mocks.GetConstructorFinder();
            var registration = cb.RegisterType<MultipleConstructors>().FindConstructorsWith(constructorFinder);

            Assert.Equal(constructorFinder, registration.ActivatorData.ConstructorFinder);
        }

        [Fact]
        public void FindConstructorsWith_NullCustomFinderProvided_ThrowsException()
        {
            var cb = new ContainerBuilder();

            var exception = Assert.Throws<ArgumentNullException>(
                () => cb.RegisterType<MultipleConstructors>().FindConstructorsWith((IConstructorFinder)null));

            Assert.Equal("constructorFinder", exception.ParamName);
        }

        [Fact]
        public void FindConstructorsWith_NullFinderFunctionProvided_ThrowsException()
        {
            var cb = new ContainerBuilder();

            var exception = Assert.Throws<ArgumentNullException>(
                () => cb.RegisterType<MultipleConstructors>().FindConstructorsWith((Func<Type, ConstructorInfo[]>)null));

            Assert.Equal("finder", exception.ParamName);
        }

        [Fact]
        public void NullIsNotAValidConstructor()
        {
            var cb = new ContainerBuilder();
            var registration = cb.RegisterType<MultipleConstructors>();
            Assert.Throws<ArgumentNullException>(() => registration.UsingConstructor((Type[])null));
        }

        [Fact]
        public void NullIsNotAValidExpressionConstructor()
        {
            var cb = new ContainerBuilder();
            var registration = cb.RegisterType<MultipleConstructors>();
            Assert.Throws<ArgumentNullException>(() => registration.UsingConstructor((Expression<Func<MultipleConstructors>>)null));
        }

        [Fact]
        public void UsingConstructorThatIsNotPresent_ThrowsArgumentException()
        {
            var cb = new ContainerBuilder();
            var registration = cb.RegisterType<MultipleConstructors>();
            Assert.Throws<ArgumentException>(() => registration.UsingConstructor(typeof(A2)));
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
