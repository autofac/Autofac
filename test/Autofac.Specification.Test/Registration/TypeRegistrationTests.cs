// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Autofac.Specification.Test.Registration
{
    public class TypeRegistrationTests
    {
        private interface IA
        {
        }

        private interface IB
        {
        }

        private interface IC
        {
        }

        public interface IMyService
        {
        }

        [Fact]
        public void AsImplementedInterfacesGeneric()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<ABC>().AsImplementedInterfaces();
            var context = builder.Build();

            context.Resolve<IA>();
            context.Resolve<IB>();
            context.Resolve<IC>();
        }

        [Fact]
        public void AsImplementedInterfacesNonGeneric()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType(typeof(ABC)).AsImplementedInterfaces();
            var context = builder.Build();

            context.Resolve<IA>();
            context.Resolve<IB>();
            context.Resolve<IC>();
        }

        [Fact]
        public void AsSelfGeneric()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<ABC>().AsSelf();
            var context = builder.Build();

            context.Resolve<ABC>();
        }

        [Fact]
        public void AsSelfNonGeneric()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType(typeof(ABC)).AsSelf();
            var context = builder.Build();

            context.Resolve<ABC>();
        }

        [Fact]
        public void OneTypeImplementMultipleInterfaces_OtherObjectsImplementingOneOfThoseInterfaces_CanBeResolved()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType(typeof(ABC)).As(typeof(IA), typeof(IB));
            builder.RegisterType(typeof(A)).As(typeof(IA));

            var container = builder.Build();
            var lifetime = container.BeginLifetimeScope(cb => cb.Register(_ => new object()));
            Assert.NotNull(lifetime.Resolve<IB>());

            var allImplementationsOfServiceA = lifetime.Resolve<IEnumerable<IA>>();
            Assert.Equal(2, allImplementationsOfServiceA.Count());
        }

        [Fact]
        public void RegisterTypeAsSupportedAndUnsupportedService()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<A>().As<IA, IB>();
            Assert.Throws<ArgumentException>(() => builder.Build());
        }

        [Fact]
        public void RegisterTypeAsUnsupportedService()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<string>().As<IA>();
            Assert.Throws<ArgumentException>(() => builder.Build());
        }

        [Fact]
        public void RegisterTypesCanBeFilteredByAssignableTo()
        {
            var container = new ContainerBuilder().Build().BeginLifetimeScope(b =>
                b.RegisterTypes(typeof(MyComponent), typeof(MyComponent2))
                    .AssignableTo(typeof(IMyService)));

            Assert.Single(container.ComponentRegistry.Registrations);
            Assert.True(container.TryResolve(typeof(MyComponent), out object obj));
            Assert.False(container.TryResolve(typeof(MyComponent2), out obj));
        }

        [Fact]
        public void TypeRegisteredOnlyWithServiceNotRegisteredAsSelf()
        {
            var cb = new ContainerBuilder();
            cb.RegisterType<A>().As<IA>();
            var c = cb.Build();
            Assert.False(c.IsRegistered<A>());
        }

        [Fact]
        public void TypeRegisteredWithMultipleServicesCanBeResolved()
        {
            var target = new ContainerBuilder();
            target.RegisterType<ABC>()
                .As<IA, IB, IC>()
                .SingleInstance();
            var container = target.Build();
            var a = container.Resolve<IA>();
            var b = container.Resolve<IB>();
            var c = container.Resolve<IC>();
            Assert.NotNull(a);
            Assert.Same(a, b);
            Assert.Same(b, c);
        }

        [Fact]
        public void TypeRegisteredWithoutServiceCanBeResolved()
        {
            var cb = new ContainerBuilder();
            cb.RegisterType<A>();
            var c = cb.Build();
            var a = c.Resolve<A>();
            Assert.NotNull(a);
            Assert.IsType<A>(a);
        }

        [Fact]
        public void TypeRegisteredWithSingleServiceCanBeResolved()
        {
            var cb = new ContainerBuilder();
            cb.RegisterType<A>().As<IA>();
            var c = cb.Build();
            var a = c.Resolve<IA>();
            Assert.NotNull(a);
            Assert.IsType<A>(a);
        }

        private class A : IA
        {
        }

        private class ABC : IA, IB, IC
        {
        }

        public sealed class MyComponent : IMyService
        {
        }

        public sealed class MyComponent2
        {
        }
    }
}
