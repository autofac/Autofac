using System;
using Autofac.Core;
using Autofac.Test.Scenarios.Parameterisation;
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

            var c = new Container();
            c.ComponentRegistry.Register(r);

            object o;

            Assert.True(c.TryResolveNamed(name, typeof(string), out o));
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
            var container = new Container();
            Assert.True(container.IsRegistered<ILifetimeScope>());
            Assert.True(container.IsRegistered<IComponentContext>());
        }

        [Fact]
        public void ResolvingLifetimeScopeProvidesCurrentScope()
        {
            var c = new Container();
            var l = c.BeginLifetimeScope();
            Assert.Same(l, l.Resolve<ILifetimeScope>());
        }

        [Fact]
        public void ResolvingComponentContextProvidesCurrentScope()
        {
            var c = new Container();
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

        private class ReplaceInstanceModule : Module
        {
            protected override void AttachToComponentRegistration(IComponentRegistry componentRegistry, IComponentRegistration registration)
            {
                registration.Activating += (o, args) =>
                {
                    args.ReplaceInstance(new ReplaceableComponent { IsReplaced = true });
                };
            }
        }

        private class ReplaceableComponent
        {
            public bool IsReplaced { get; set; }
        }
    }
}
