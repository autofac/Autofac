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

        //[Test]
        //[ExpectedException(typeof(DependencyResolutionException))]
        //public void InnerCannotResolveOuterDependencies()
        //{
        //    var outerBuilder = new ContainerBuilder();
        //    outerBuilder.RegisterType<B1>().SingleInstance();
        //    var outer = outerBuilder.Build();

        //    var innerBuilder = new ContainerBuilder();
        //    innerBuilder.RegisterType<C1>();
        //    innerBuilder.RegisterType<A1>();
        //    var inner = outer.BeginLifetimeScope();
        //    innerBuilder.Build(inner);

        //    var unused = inner.Resolve<C1>();
        //}

        //[Test]
        //public void OuterInstancesCannotReferenceInner()
        //{
        //    var builder = new ContainerBuilder();
        //    builder.RegisterType<A1>().WithScope(InstanceSharing.Container);
        //    builder.RegisterType<B1>().WithScope(InstanceSharing.Factory);

        //    var outer = builder.Build();

        //    var inner = outer.BeginLifetimeScope();

        //    var outerB = outer.Resolve<B1>();
        //    var innerB = inner.Resolve<B1>();
        //    var outerA = outer.Resolve<A1>();
        //    var innerA = inner.Resolve<A1>();

        //    Assert.AreSame(innerA, innerB.A1);
        //    Assert.AreSame(outerA, outerB.A1);
        //    Assert.AreNotSame(innerA, outerA);
        //    Assert.AreNotSame(innerB, outerB);
        //}


        [Test]
        public void ResolveByName()
        {
            string name = "name";

            var r = Factory.CreateSingletonRegistration(
                new Service[] { new NamedService(name) },
                Factory.CreateReflectionActivator(typeof(object)));

            var c = new Container();
            c.ComponentRegistry.Register(r);

            object o;

            Assert.IsTrue(c.TryResolve(name, out o));
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
            cb.RegisterType<object>().Named(myName);
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
    }
}
