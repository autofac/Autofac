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
using Autofac.Tests.Scenarios.Dependencies;

namespace Autofac.Tests.Core
{
    [TestFixture]
    public class ContainerTests
    {

        // In the below scenario, B depends on A, CD depends on A and B,
        // and E depends on IC and B.

        #region Scenario Classes

        class A : DisposeTracker { }

        class B : DisposeTracker
        {
            public A A;

            public B(A a)
            {
                A = a;
            }
        }

        interface IC { }

        class C : DisposeTracker
        {
            public B B;

            public C(B b)
            {
                B = b;
            }
        }

        interface ID { }

        class CD : DisposeTracker, IC, ID
        {
            public A A;
            public B B;

            public CD(A a, B b)
            {
                A = a;
                B = b;
            }
        }

        class E : DisposeTracker
        {
            public B B;
            public IC C;

            public E(B b, IC c)
            {
                B = b;
                C = c;
            }
        }

        class F
        {
            public IList<A> AList;
            public F(IList<A> aList)
            {
                AList = aList;
            }
        }

        #endregion

        //[Test]
        //[ExpectedException(typeof(DependencyResolutionException))]
        //public void InnerCannotResolveOuterDependencies()
        //{
        //    var outerBuilder = new ContainerBuilder();
        //    outerBuilder.RegisterType<B>().SingleSharedInstance();
        //    var outer = outerBuilder.Build();

        //    var innerBuilder = new ContainerBuilder();
        //    innerBuilder.RegisterType<C>();
        //    innerBuilder.RegisterType<A>();
        //    var inner = outer.BeginLifetimeScope();
        //    innerBuilder.Build(inner);

        //    var unused = inner.Resolve<C>();
        //}

        //[Test]
        //public void OuterInstancesCannotReferenceInner()
        //{
        //    var builder = new ContainerBuilder();
        //    builder.RegisterType<A>().WithScope(InstanceSharing.Container);
        //    builder.RegisterType<B>().WithScope(InstanceSharing.Factory);

        //    var outer = builder.Build();

        //    var inner = outer.BeginLifetimeScope();

        //    var outerB = outer.Resolve<B>();
        //    var innerB = inner.Resolve<B>();
        //    var outerA = outer.Resolve<A>();
        //    var innerA = inner.Resolve<A>();

        //    Assert.AreSame(innerA, innerB.A);
        //    Assert.AreSame(outerA, outerB.A);
        //    Assert.AreNotSame(innerA, outerA);
        //    Assert.AreNotSame(innerB, outerB);
        //}

        [Test]
        public void IntegrationTest()
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<A>().SingleSharedInstance();
            builder.RegisterType<CD>().As<IC, ID>().SingleSharedInstance();
            builder.RegisterType<E>().SingleSharedInstance();
            builder.RegisterDelegate(ctr => new B(ctr.Resolve<A>()));

            var target = builder.Build();

            E e = target.Resolve<E>();
            A a = target.Resolve<A>();
            B b = target.Resolve<B>();
            IC c = target.Resolve<IC>();
            ID d = target.Resolve<ID>();

            Assert.IsInstanceOfType(typeof(CD), c);
            CD cd = (CD)c;

            Assert.AreSame(a, b.A);
            Assert.AreSame(a, cd.A);
            Assert.AreNotSame(b, cd.B);
            Assert.AreSame(c, e.C);
            Assert.AreNotSame(b, e.B);
            Assert.AreNotSame(e.B, cd.B);
        }

        [Test]
        public void DisposeOrder1()
        {
            var target = new Container();

            target.ComponentRegistry.Register(Factory.CreateSingletonRegistration(typeof(A)));
            target.ComponentRegistry.Register(Factory.CreateSingletonRegistration(typeof(B)));

            A a = target.Resolve<A>();
            B b = target.Resolve<B>();

            Queue<object> disposeOrder = new Queue<object>();

            a.Disposing += (s, e) => disposeOrder.Enqueue(a);
            b.Disposing += (s, e) => disposeOrder.Enqueue(b);

            target.Dispose();

            // B depends on A, therefore B should be disposed first

            Assert.AreEqual(2, disposeOrder.Count);
            Assert.AreSame(b, disposeOrder.Dequeue());
            Assert.AreSame(a, disposeOrder.Dequeue());
        }

        // In this version, resolve order is reversed.
        [Test]
        public void DisposeOrder2()
        {
            var target = new Container();

            target.ComponentRegistry.Register(Factory.CreateSingletonRegistration(typeof(A)));
            target.ComponentRegistry.Register(Factory.CreateSingletonRegistration(typeof(B)));

            B b = target.Resolve<B>();
            A a = target.Resolve<A>();

            Queue<object> disposeOrder = new Queue<object>();

            a.Disposing += (s, e) => disposeOrder.Enqueue(a);
            b.Disposing += (s, e) => disposeOrder.Enqueue(b);

            target.Dispose();

            // B depends on A, therefore B should be disposed first

            Assert.AreEqual(2, disposeOrder.Count);
            Assert.AreSame(b, disposeOrder.Dequeue());
            Assert.AreSame(a, disposeOrder.Dequeue());
        }

        [Test]
        public void ResolveSingletonFromContext()
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<A>().SingleSharedInstance();

            var target = builder.Build();

            var context = target.BeginLifetimeScope();

            var ctxA = context.Resolve<A>();
            var targetA = target.Resolve<A>();

            Assert.AreSame(ctxA, targetA);
            Assert.IsNotNull(ctxA);

            Assert.IsFalse(ctxA.IsDisposed);

            context.Dispose();

            Assert.IsFalse(ctxA.IsDisposed);

            target.Dispose();

            Assert.IsTrue(ctxA.IsDisposed);
        }

        [Test]
        public void ResolveTransientFromContext()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<A>();

            var target = builder.Build();

            var context = target.BeginLifetimeScope();

            var ctxA = context.Resolve<A>();
            var targetA = target.Resolve<A>();

            Assert.IsNotNull(ctxA);
            Assert.IsNotNull(targetA);
            Assert.AreNotSame(ctxA, targetA);

            Assert.IsFalse(targetA.IsDisposed);
            Assert.IsFalse(ctxA.IsDisposed);

            context.Dispose();

            Assert.IsFalse(targetA.IsDisposed);
            Assert.IsTrue(ctxA.IsDisposed);

            target.Dispose();

            Assert.IsTrue(targetA.IsDisposed);
            Assert.IsTrue(ctxA.IsDisposed);
        }

        [Test]
        public void ResolveScopedFromContext()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<A>().ShareInstanceInLifetimeScope();

            var target = builder.Build();

            var context = target.BeginLifetimeScope();

            var ctxA = context.Resolve<A>();
            var ctxA2 = context.Resolve<A>();

            Assert.IsNotNull(ctxA);
            Assert.AreSame(ctxA, ctxA2);

            var targetA = target.Resolve<A>();
            var targetA2 = target.Resolve<A>();

            Assert.IsNotNull(targetA);
            Assert.AreSame(targetA, targetA2);
            Assert.AreNotSame(ctxA, targetA);

            Assert.IsFalse(targetA.IsDisposed);
            Assert.IsFalse(ctxA.IsDisposed);

            context.Dispose();

            Assert.IsFalse(targetA.IsDisposed);
            Assert.IsTrue(ctxA.IsDisposed);

            target.Dispose();

            Assert.IsTrue(targetA.IsDisposed);
            Assert.IsTrue(ctxA.IsDisposed);
        }

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
            cb.RegisterDelegate((c, p) => new Parameterised(p.Named<string>("a"), p.Named<int>("b")));
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
