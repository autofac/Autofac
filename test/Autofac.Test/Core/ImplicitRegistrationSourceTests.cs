// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using Autofac.Core;
using Autofac.Core.Registration;
using Xunit;

namespace Autofac.Test.Core
{
    public class ImplicitRegistrationSourceTests
    {
        [InlineData(typeof(string))]
        [InlineData(typeof(TypeWithTwoGenericParams<,>))]
        [Theory]
        public void InvalidTypeTest(Type type)
        {
            Assert.Throws<InvalidOperationException>(() => new AnyTypeImplicitRegistrationSource(type));
        }

        [Fact]
        public void NothingRegistered()
        {
            var builder = new ContainerBuilder();

            builder.RegisterSource(new MappedImplicitRegistrationSource());

            using (var container = builder.Build())
            {
                Assert.Throws<ComponentNotRegisteredException>(() => container.Resolve<Mapped<string>>());
            }
        }

        [Fact]
        public void SingleEntry()
        {
            const string Instance1 = "string1";
            var builder = new ContainerBuilder();

            builder.RegisterSource(new MappedImplicitRegistrationSource());
            builder.RegisterInstance(Instance1);

            using (var container = builder.Build())
            {
                var mapped = container.Resolve<Mapped<string>>();

                Assert.Same(Instance1, mapped.Instance);
            }
        }

        [Fact]
        public void EnumerableOrdered()
        {
            const string Instance1 = "string1";
            const string Instance2 = "string2";
            const string Instance3 = "string3";

            var builder = new ContainerBuilder();

            builder.RegisterSource(new MappedImplicitRegistrationSource());
            builder.RegisterInstance(Instance1);
            builder.RegisterInstance(Instance2);
            builder.RegisterInstance(Instance3);

            using (var container = builder.Build())
            {
                var mapped = container.Resolve<IEnumerable<Mapped<string>>>();

                Assert.Collection(
                    mapped,
                    m => Assert.Same(Instance1, m.Instance),
                    m => Assert.Same(Instance2, m.Instance),
                    m => Assert.Same(Instance3, m.Instance));
            }
        }

        [Fact]
        public void MultipleImplicitChained()
        {
            const string Instance1 = "string1";
            const string Instance2 = "string2";
            const string Instance3 = "string3";

            var builder = new ContainerBuilder();

            builder.RegisterSource(new MappedImplicitRegistrationSource());
            builder.RegisterInstance(Instance1);
            builder.RegisterInstance(Instance2);
            builder.RegisterInstance(Instance3);

            using (var container = builder.Build())
            {
                var mapped = container.Resolve<IEnumerable<Lazy<Mapped<string>>>>();

                Assert.Collection(
                    mapped,
                    m => Assert.Same(Instance1, m.Value.Instance),
                    m => Assert.Same(Instance2, m.Value.Instance),
                    m => Assert.Same(Instance3, m.Value.Instance));
            }
        }

        private class TypeWithTwoGenericParams<T1, T2>
        {
        }

        private class AnyTypeImplicitRegistrationSource : ImplicitRegistrationSource
        {
            public AnyTypeImplicitRegistrationSource(Type type)
                : base(type)
            {
            }

            protected override object ResolveInstance<T>(IComponentContext ctx, ResolveRequest request) => throw new NotImplementedException();
        }

        private class Mapped<T>
        {
            public Mapped(T instance)
            {
                Instance = instance;
            }

            public T Instance { get; }
        }

        private class MappedImplicitRegistrationSource : ImplicitRegistrationSource
        {
            public MappedImplicitRegistrationSource()
                : base(typeof(Mapped<>))
            {
            }

            protected override object ResolveInstance<T>(IComponentContext ctx, ResolveRequest request)
            {
                return new Mapped<T>((T)ctx.ResolveComponent(request));
            }
        }
    }
}
