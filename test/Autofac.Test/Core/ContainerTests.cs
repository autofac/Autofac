// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading.Tasks;
using Autofac.Core;
using Autofac.Core.Registration;
using Autofac.Core.Resolving.Pipeline;
using Autofac.Test.Scenarios.Parameterisation;
using Autofac.Test.Util;
using Xunit;

namespace Autofac.Test.Core
{
    public class ContainerTests
    {
        [Fact]
        public void ResolveByName()
        {
            string name = "name";

            var r = Factory.CreateSingletonRegistration(
                new Service[] { new KeyedService(name, typeof(string)) },
                Factory.CreateReflectionActivator(typeof(object)));

            var builder = Factory.CreateEmptyComponentRegistryBuilder();
            builder.Register(r);

            var c = new ContainerBuilder(builder).Build();

            Assert.True(c.TryResolveNamed(name, typeof(string), out object o));
            Assert.NotNull(o);

            Assert.False(c.IsRegistered<object>());
        }

        [Fact]
        public void RegisterParameterisedWithDelegate()
        {
            var cb = new ContainerBuilder();
            cb.Register((c, p) => new Parameterised(p.Named<string>("a"), p.Named<int>("b")));
            var container = cb.Build();
            var aVal = "Hello";
            var bVal = 42;
            var result = container.Resolve<Parameterised>(
                new NamedParameter("a", aVal),
                new NamedParameter("b", bVal));
            Assert.NotNull(result);
            Assert.Equal(aVal, result.A);
            Assert.Equal(bVal, result.B);
        }

        [Fact]
        public void RegisterParameterisedWithReflection()
        {
            var cb = new ContainerBuilder();
            cb.RegisterType<Parameterised>();
            var container = cb.Build();
            var aVal = "Hello";
            var bVal = 42;
            var result = container.Resolve<Parameterised>(
                new NamedParameter("a", aVal),
                new NamedParameter("b", bVal));
            Assert.NotNull(result);
            Assert.Equal(aVal, result.A);
            Assert.Equal(bVal, result.B);
        }

        [Fact]
        public void SupportsIServiceProvider()
        {
            var cb = new ContainerBuilder();
            cb.RegisterType<object>();
            var container = cb.Build();
            var sp = (IServiceProvider)container;
            var o = sp.GetService(typeof(object));
            Assert.NotNull(o);
            var s = sp.GetService(typeof(string));
            Assert.Null(s);
        }

        [Fact]
        public void ResolveByNameWithServiceType()
        {
            var myName = "Something";
            var cb = new ContainerBuilder();
            cb.RegisterType<object>().Named<object>(myName);
            var container = cb.Build();
            var o = container.ResolveNamed<object>(myName);
            Assert.NotNull(o);
        }

        [Fact]
        public void ResolveByKeyWithServiceType()
        {
            var myKey = new object();
            var component = new object();

            var cb = new ContainerBuilder();
            cb.Register(c => component).Keyed<object>(myKey);
            var container = cb.Build();

            var o = container.ResolveKeyed<object>(myKey);
            Assert.Same(component, o);
        }

        [Fact]
        public void ResolveByKeyWithServiceTypeNonGeneric()
        {
            var myKey = new object();
            var component = new object();

            var cb = new ContainerBuilder();
            cb.Register(c => component).Keyed<object>(myKey);
            var container = cb.Build();

            var o = container.ResolveKeyed(myKey, typeof(object));
            Assert.Same(component, o);
        }

        [Fact]
        public void ContainerProvidesILifetimeScopeAndIContext()
        {
            var container = Factory.CreateEmptyContainer();
            Assert.True(container.IsRegistered<ILifetimeScope>());
            Assert.True(container.IsRegistered<IComponentContext>());
        }

        [Fact]
        public void ResolvingLifetimeScopeProvidesCurrentScope()
        {
            var c = Factory.CreateEmptyContainer();
            var l = c.BeginLifetimeScope();
            Assert.Same(l, l.Resolve<ILifetimeScope>());
        }

        [Fact]
        public void ResolvingComponentContextProvidesCurrentScope()
        {
            var c = Factory.CreateEmptyContainer();
            var l = c.BeginLifetimeScope();
            Assert.Same(l, l.Resolve<IComponentContext>());
        }

        [Fact]
        public void ReplacingAnInstanceInActivatingHandlerSubstitutesForResult()
        {
            var supplied = new object();

            var cb = new ContainerBuilder();
            cb.RegisterType<object>().OnActivating(e => e.ReplaceInstance(supplied));
            var c = cb.Build();

            var resolved = c.Resolve<object>();

            Assert.Same(supplied, resolved);
        }

        [Fact]
        public void ReplaceInstance_RegistrationActivatingHandlerProvidesResultToRelease()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<ReplaceableComponent>()
                .OnActivating(c => c.ReplaceInstance(new ReplaceableComponent { IsReplaced = true }))
                .OnRelease(c => Assert.True(c.IsReplaced));

            using (var container = builder.Build())
            {
                container.Resolve<ReplaceableComponent>();
            }
        }

        [Fact]
        public void ReplaceInstance_ModuleActivatingHandlerProvidesResultToRelease()
        {
            var builder = new ContainerBuilder();
            builder.RegisterModule<ReplaceInstanceModule>();
            builder.RegisterType<ReplaceableComponent>()
                .OnRelease(c => Assert.True(c.IsReplaced));

            using (var container = builder.Build())
            {
                container.Resolve<ReplaceableComponent>();
            }
        }

        [Fact]
        public async ValueTask AsyncContainerDisposeTriggersAsyncServiceDispose()
        {
            var builder = new ContainerBuilder();
            builder.Register(c => new AsyncDisposeTracker()).SingleInstance();

            AsyncDisposeTracker tracker;

            await using (var container = builder.Build())
            {
                tracker = container.Resolve<AsyncDisposeTracker>();

                Assert.False(tracker.IsSyncDisposed);
                Assert.False(tracker.IsAsyncDisposed);
            }

            Assert.False(tracker.IsSyncDisposed);
            Assert.True(tracker.IsAsyncDisposed);
        }

        private class ReplaceInstanceModule : Module
        {
            protected override void AttachToComponentRegistration(IComponentRegistryBuilder componentRegistry, IComponentRegistration registration)
            {
                registration.PipelineBuilding += (o, builder) => builder.Use(PipelinePhase.Activation, (ctxt, next) =>
                {
                    ctxt.Instance = new ReplaceableComponent { IsReplaced = true };
                });
            }
        }

        private class ReplaceableComponent
        {
            public bool IsReplaced { get; set; }
        }
    }
}
