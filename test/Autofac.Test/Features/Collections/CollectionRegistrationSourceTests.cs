﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Autofac.Builder;
using Autofac.Core.Registration;
using Autofac.Test.Util;
using Xunit;

namespace Autofac.Test.Features.Collections
{
    public class CollectionRegistrationSourceTests
    {
        public interface IFoo
        {
        }

        [Fact]
        public void CollectionInNestedLifetimeScope()
        {
            // Issue #711
            // Note #711 was using named collections; this test is not.
            // Named collections don't have different behavior than the standard
            // auto-generated behavior from a resolve standpoint since you
            // can't resolve a specifically named collection.
            var cb = new ContainerBuilder();
            cb.RegisterType<Foo1>().As<IFoo>();
            cb.RegisterType<Foo2>().As<IFoo>();
            using (var container = cb.Build())
            {
                var collection = container.Resolve<IEnumerable<IFoo>>();
                Assert.Equal(2, collection.Count());

                using (var scope = container.BeginLifetimeScope())
                {
                    collection = container.Resolve<IEnumerable<IFoo>>();
                    Assert.Equal(2, collection.Count());
                }

                using (var scope = container.BeginLifetimeScope(b => { }))
                {
                    collection = container.Resolve<IEnumerable<IFoo>>();
                    Assert.Equal(2, collection.Count());
                }
            }
        }

        [Fact]
        public void EnumerablesFromDifferentLifetimeScopesShouldReturnDifferentCollections()
        {
            var rootBuilder = new ContainerBuilder();
            rootBuilder.RegisterType<Foo1>().As<IFoo>();
            var rootContainer = rootBuilder.Build();

            var scopeA = rootContainer.BeginLifetimeScope(
                scopeBuilder => scopeBuilder.RegisterType<Foo2>().As<IFoo>());
            var arrayA = scopeA.Resolve<IEnumerable<IFoo>>().ToArray();

            var scopeB = rootContainer.BeginLifetimeScope(
                scopeBuilder => scopeBuilder.RegisterType<Foo3>().As<IFoo>());
            var arrayB = scopeB.Resolve<IEnumerable<IFoo>>().ToArray();

            Assert.Equal(2, arrayA.Count());
            Assert.Contains(arrayA, x => x is Foo1);
            Assert.Contains(arrayA, x => x is Foo2);

            Assert.Equal(2, arrayB.Count());
            Assert.Contains(arrayB, x => x is Foo1);
            Assert.Contains(arrayB, x => x is Foo3);
        }

        [Theory]
        [InlineData(typeof(IEnumerable<string>), typeof(string[]))]
        [InlineData(typeof(string[]), typeof(string[]))]
        [InlineData(typeof(IList<string>), typeof(List<string>))]
        [InlineData(typeof(IReadOnlyList<string>), typeof(List<string>))]
        [InlineData(typeof(ICollection<string>), typeof(List<string>))]
        [InlineData(typeof(IReadOnlyCollection<string>), typeof(List<string>))]
        public void LimitTypeSetCorrectlyForServiceType(Type serviceType, Type limitType)
        {
            var registration = new ContainerBuilder()
                .Build()
                .RegistrationFor(serviceType);

            Assert.Equal(limitType, registration.Activator.LimitType);
        }

        [Fact]
        public void ReflectsChangesInComponentRegistry()
        {
            var cb = new ContainerBuilder();
            cb.RegisterInstance("Hello");
            var c = cb.Build();
            Assert.Single(c.Resolve<IEnumerable<string>>());

            var lifetimeScope = c.BeginLifetimeScope(inner => inner.RegisterInstance("World"));
            Assert.Equal(2, lifetimeScope.Resolve<IEnumerable<string>>().Count());
        }

        [Fact]
        public void ResolvesAllAvailableElementsWhenArrayIsRequested()
        {
            var cb = new ContainerBuilder();
            var s1 = "Hello";
            var s2 = "World";
            cb.RegisterInstance(s1);
            cb.RegisterInstance(s2);
            var c = cb.Build();

            var strings = c.Resolve<string[]>();
            Assert.Equal(2, strings.Length);
            Assert.Contains(s1, strings);
            Assert.Contains(s2, strings);
        }

        [Fact]
        public void ResolvesAllAvailableElementsWhenCollectionIsRequested()
        {
            var cb = new ContainerBuilder();
            const string s1 = "Hello";
            const string s2 = "World";
            cb.RegisterInstance(s1);
            cb.RegisterInstance(s2);
            var c = cb.Build();

            var strings = c.Resolve<ICollection<string>>();

            Assert.Equal(2, strings.Count);
            Assert.Contains(s1, strings);
            Assert.Contains(s2, strings);
            Assert.IsType<List<string>>(strings);
        }

        [Fact]
        public void ResolvesAllAvailableElementsWhenEnumerableIsRequested()
        {
            var cb = new ContainerBuilder();
            var s1 = "Hello";
            var s2 = "World";
            cb.RegisterInstance(s1);
            cb.RegisterInstance(s2);
            var c = cb.Build();

            var strings = c.Resolve<IEnumerable<string>>();
            Assert.Equal(2, strings.Count());
            Assert.Contains(s1, strings);
            Assert.Contains(s2, strings);
        }

        [Fact]
        public void ResolvesAllAvailableElementsWhenListIsRequested()
        {
            var cb = new ContainerBuilder();
            const string s1 = "Hello";
            const string s2 = "World";
            cb.RegisterInstance(s1);
            cb.RegisterInstance(s2);
            var c = cb.Build();

            var strings = c.Resolve<IList<string>>();

            Assert.Equal(2, strings.Count);
            Assert.Contains(s1, strings);
            Assert.Contains(s2, strings);
            Assert.IsType<List<string>>(strings);
        }

        [Fact]
        public void ResolvesAllAvailableElementsWhenReadOnlyCollectionIsRequested()
        {
            var cb = new ContainerBuilder();
            const string s1 = "Hello";
            const string s2 = "World";
            cb.RegisterInstance(s1);
            cb.RegisterInstance(s2);
            var c = cb.Build();

            var strings = c.Resolve<IReadOnlyCollection<string>>();

            Assert.Equal(2, strings.Count);
            Assert.Contains(s1, strings);
            Assert.Contains(s2, strings);
        }

        [Fact]
        public void ResolvesAllAvailableElementsWhenReadOnlyListIsRequested()
        {
            var cb = new ContainerBuilder();
            const string s1 = "Hello";
            const string s2 = "World";
            cb.RegisterInstance(s1);
            cb.RegisterInstance(s2);
            var c = cb.Build();

            var strings = c.Resolve<IReadOnlyList<string>>();

            Assert.Equal(2, strings.Count);
            Assert.Contains(s1, strings);
            Assert.Contains(s2, strings);
        }

        [Fact]
        public void ResolvesCollectionItemsFromCurrentLifetimeScope()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<DisposeTracker>();
            var container = builder.Build();

            DisposeTracker tracker;
            using (var ls = container.BeginLifetimeScope())
                tracker = ls.Resolve<IEnumerable<DisposeTracker>>().First();

            Assert.True(tracker.IsDisposed);
        }

        [Fact]
        public void ResolvingClosedCollectionTypeThrowsException()
        {
            var cb = new ContainerBuilder();
            const string s1 = "Hello";
            const string s2 = "World";
            cb.RegisterInstance(s1);
            cb.RegisterInstance(s2);
            var c = cb.Build();

            Assert.Throws<ComponentNotRegisteredException>(() => c.Resolve<Collection<string>>());
        }

        [Fact]
        public void ResolvingClosedListTypeThrowsException()
        {
            var cb = new ContainerBuilder();
            const string s1 = "Hello";
            const string s2 = "World";
            cb.RegisterInstance(s1);
            cb.RegisterInstance(s2);
            var c = cb.Build();

            Assert.Throws<ComponentNotRegisteredException>(() => c.Resolve<List<string>>());
        }

        [Fact]
        public void ResolvingClosedReadOnlyCollectionTypeThrowsException()
        {
            var cb = new ContainerBuilder();
            const string s1 = "Hello";
            const string s2 = "World";
            cb.RegisterInstance(s1);
            cb.RegisterInstance(s2);
            var c = cb.Build();

            Assert.Throws<ComponentNotRegisteredException>(() => c.Resolve<ReadOnlyCollection<string>>());
        }

        public class Foo1 : IFoo
        {
        }

        public class Foo2 : IFoo
        {
        }

        public class Foo3 : IFoo
        {
        }
    }
}
