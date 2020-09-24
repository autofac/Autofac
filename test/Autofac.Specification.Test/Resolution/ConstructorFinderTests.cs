// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Linq;
using System.Reflection;
using Autofac.Core.Activators.Reflection;
using Xunit;

namespace Autofac.Specification.Test.Resolution
{
    public class ConstructorFinderTests
    {
        [Fact]
        public void AdditionalTypeRegisteredInChildLifetimeScopeConsideredWhenSelectingConstructor()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<MultipleConstructors>();
            builder.RegisterType<A1>();
            var container = builder.Build();
            var instance = container.Resolve<MultipleConstructors>();
            Assert.Equal(1, instance.CalledCtor);

            var lifetimeScope = container.BeginLifetimeScope(b => b.RegisterType<A2>());
            instance = lifetimeScope.Resolve<MultipleConstructors>();
            Assert.Equal(2, instance.CalledCtor);
        }

        [Fact]
        public void CanReplaceConstructorFinderForRegistrationInChildLifetimeScope()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<A1>();
            builder.RegisterType<A2>();
            builder.RegisterType<MultipleConstructors>();
            var container = builder.Build();
            var lifetimeScope = container.BeginLifetimeScope(b =>
            {
                b.RegisterType<MultipleConstructors>()
                    .FindConstructorsWith(type => type.GetTypeInfo().GetConstructors().Where(ci => ci.GetParameters().Length == 1).ToArray());
            });

            var root = container.Resolve<MultipleConstructors>();
            Assert.Equal(2, root.CalledCtor);

            var nested = lifetimeScope.Resolve<MultipleConstructors>();
            Assert.Equal(1, nested.CalledCtor);
        }

        [Fact]
        public void FindConstructorsWith_AsSelfWorksWithCustomConstructorExpression()
        {
            // Issue #907
            // AsSelf ignores custom constructor finders if it's registered
            // before the custom constructor finder.
            var cb = new ContainerBuilder();
            cb.RegisterType<A1>();
            cb.RegisterType<PrivateConstructor>().AsSelf().FindConstructorsWith(type => type.GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic));
            var container = cb.Build();

            var instance = container.Resolve<PrivateConstructor>();

            Assert.NotNull(instance.A1);
        }

        [Fact]
        public void FindConstructorsWith_CanUseFunctionToFindPrivateConstructors()
        {
            var cb = new ContainerBuilder();
            cb.RegisterType<A1>();
            cb.RegisterType<PrivateConstructor>().FindConstructorsWith(type => type.GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic));
            var container = cb.Build();

            var instance = container.Resolve<PrivateConstructor>();

            Assert.NotNull(instance.A1);
        }

        [Fact]
        public void FindConstructorsWith_CustomFinderProvided_CustomFinderInvoked()
        {
            var builder = new ContainerBuilder();
            var finder = new CustomConstructorFinder();
            builder.RegisterType<object>().FindConstructorsWith(finder);
            var container = builder.Build();

            container.Resolve<object>();

            Assert.True(finder.FindConstructorsCalled);
        }

        [Fact]
        public void FindConstructorsWith_FinderFunctionProvided_PassedToConstructorFinder()
        {
            var cb = new ContainerBuilder();
            var finderCalled = false;
            Func<Type, ConstructorInfo[]> finder = type =>
            {
                finderCalled = true;
                return type.GetConstructors();
            };
            cb.RegisterType<A1>();
            cb.RegisterType<MultipleConstructors>().FindConstructorsWith(finder);
            var container = cb.Build();

            container.Resolve<MultipleConstructors>();

            Assert.True(finderCalled);
        }

        // Disable "unused parameter" warnings for test types.
#pragma warning disable IDE0051

        public class A1
        {
        }

        public class A2
        {
        }

        public class CustomConstructorFinder : IConstructorFinder
        {
            public bool FindConstructorsCalled { get; private set; }

            public ConstructorInfo[] FindConstructors(Type targetType)
            {
                FindConstructorsCalled = true;
                return new DefaultConstructorFinder().FindConstructors(targetType);
            }
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

        public class PrivateConstructor
        {
            private PrivateConstructor(A1 a1)
            {
                A1 = a1;
            }

            public A1 A1 { get; set; }
        }

#pragma warning restore IDE0051

    }
}
