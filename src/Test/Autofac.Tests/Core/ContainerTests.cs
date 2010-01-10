using System;
using System.Collections.Generic;
using System.Linq;
using Autofac.Builder;
using NUnit.Framework;
using Autofac.Core.Lifetime;
using Autofac.Core.Registration;
using System.Reflection;
using Autofac.Core;
using Autofac.Core.Activators.Reflection;
using Autofac.Core.Activators.ProvidedInstance;
using Autofac.Core.Activators.Delegate;
using Autofac.Tests.Scenarios.Parameterisation;
using Autofac.Tests.Scenarios.Graph1;

namespace Autofac.Tests.Core
{
    [TestFixture]
    public class ContainerTests
    {
        [Test]
        public void ResolveByName()
        {
            string name = "name";

            var r = Factory.CreateSingletonRegistration(
                new Service[] { new NamedService(name, typeof(string)) },
                Factory.CreateReflectionActivator(typeof(object)));

            var c = new Container();
            c.ComponentRegistry.Register(r);

            object o;

            Assert.IsTrue(c.TryResolve(name, typeof(string), out o));
            Assert.IsNotNull(o);

            Assert.IsFalse(c.IsRegistered<object>());
        }

        [Test]
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
            Assert.IsNotNull(result);
            Assert.AreEqual(aVal, result.A);
            Assert.AreEqual(bVal, result.B);
        }

        [Test]
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
            Assert.IsNotNull(result);
            Assert.AreEqual(aVal, result.A);
            Assert.AreEqual(bVal, result.B);
        }

        [Test]
        public void SupportsIServiceProvider()
        {
            var cb = new ContainerBuilder();
            cb.RegisterType<object>();
            var container = cb.Build();
            var sp = (IServiceProvider)container;
            var o = sp.GetService(typeof(object));
            Assert.IsNotNull(o);
            var s = sp.GetService(typeof(string));
            Assert.IsNull(s);
        }

        [Test]
        public void ResolveByNameWithServiceType()
        {
            var myName = "Something";
            var cb = new ContainerBuilder();
            cb.RegisterType<object>().Named<object>(myName);
            var container = cb.Build();
            var o = container.Resolve<object>(myName);
            Assert.IsNotNull(o);
        }

        [Test]
        public void ContainerProvidesILifetimeScopeAndIContext()
        {
            var container = new Container();
            Assert.IsTrue(container.IsRegistered<ILifetimeScope>());
            Assert.IsTrue(container.IsRegistered<IComponentContext>());
        }

        [Test]
        public void ResolvingLifetimeScopeProvidesCurrentScope()
        {
            var c = new Container();
            var l = c.BeginLifetimeScope();
            Assert.AreSame(l, l.Resolve<ILifetimeScope>());
        }
    }
}
