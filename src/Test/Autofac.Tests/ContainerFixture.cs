using System;
using System.Collections.Generic;
using System.Linq;
using Autofac.Builder;
using NUnit.Framework;
using Autofac.Core.Lifetime;
using Autofac.Registration;
using System.Reflection;
using Autofac.Core;
using Autofac.Core.Activators.Reflection;
using Autofac.Core.Activators.ProvidedInstance;
using Autofac.Core.Activators.Delegate;

namespace Autofac.Tests
{
    [TestFixture]
    public class ContainerFixture
    {
        static IComponentRegistration CreateSingletonRegistration(IEnumerable<Service> services, IInstanceActivator activator)
        {
            return CreateRegistration(services, activator, new RootScopeLifetime(), InstanceSharing.Shared);
        }

        static IComponentRegistration CreateRegistration(IEnumerable<Service> services, IInstanceActivator activator, IComponentLifetime lifetime, InstanceSharing sharing)
        {
            return new ComponentRegistration(
                Guid.NewGuid(),
                activator,
                lifetime,
                sharing,
                InstanceOwnership.OwnedByLifetimeScope,
                services,
                new Dictionary<string, object>());
        }

        [Test]
        public void ResolveOptional()
        {
            var target = new Container();
            target.ComponentRegistry.Register(CreateSingletonRegistration(
                new[] { new TypedService(typeof(string)) },
                new ProvidedInstanceActivator("Hello")));

            var inst = target.ResolveOptional<string>();

            Assert.AreEqual("Hello", inst);
        }

        [Test]
        public void ResolveOptionalNotPresent()
        {
            var target = new Container();
            var inst = target.ResolveOptional<string>();
            Assert.IsNull(inst);
        }

        [Test]
        public void ResolveNamedOptionalWithParameters()
        {
            var cb = new ContainerBuilder();
            cb.RegisterType<Parameterised>();
            var container = cb.Build();
            const string param1 = "Hello";
            const int param2 = 42;
            var result = container.ResolveOptional<Parameterised>(
                new NamedParameter("a", param1),
                new NamedParameter("b", param2));
            Assert.IsNotNull(result);
            Assert.AreEqual(param1, result.A);
            Assert.AreEqual(param2, result.B);
        }

        [Test]
        public void ResolveNamedOptionalWithParametersNotPresent()
        {
            var target = new Container();
            var instance = target.ResolveOptional<string>(TypedParameter.From(1));
            Assert.IsNull(instance);
        }

        [Test]
        public void RegisterInstance()
        {
            var builder = new ContainerBuilder();

            var instance = new object();

            builder.RegisterInstance(instance);

            var target = builder.Build();

            Assert.AreSame(instance, target.Resolve<object>());
            Assert.IsTrue(target.IsRegistered<object>());
        }

        [Test]
        public void ReplaceInstance()
        {
            var target = new Container();

            var instance1 = new object();
            var instance2 = new object();

            target.ComponentRegistry.Register(CreateSingletonRegistration(
                new[] { new TypedService(typeof(object)) },
                new ProvidedInstanceActivator(instance1)));

            target.ComponentRegistry.Register(CreateSingletonRegistration(
                new[] { new TypedService(typeof(object)) },
                new ProvidedInstanceActivator(instance2)));

            Assert.AreSame(instance2, target.Resolve<object>());
        }

        [Test]
        public void RegisterComponent()
        {
            var registration = CreateSingletonRegistration(
                new[] { new TypedService(typeof(object)), new TypedService(typeof(string)) },
                new ProvidedInstanceActivator("Hello"));

            var target = new Container();

            target.ComponentRegistry.Register(registration);

            Assert.IsTrue(target.IsRegistered<object>());
            Assert.IsTrue(target.IsRegistered<string>());
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegisterComponentNull()
        {
            var target = new Container();

            target.ComponentRegistry.Register(null);
        }

        [Test]
#if NET20
        [ExpectedException(typeof(ArgumentNullException))]
#else
        [ExpectedException(typeof(ArgumentException))]
#endif
        public void RegisterComponentNullService()
        {
            var registration = CreateSingletonRegistration(
                new Service[] { new TypedService(typeof(object)), null },
                new ProvidedInstanceActivator(new object()));

            var target = new Container();

            target.ComponentRegistry.Register(registration);
        }

        [Test]
        public void RegisterDelegate()
        {
            object instance = new object();
            var target = new Container();
            target.ComponentRegistry.Register(CreateSingletonRegistration(
                new[] { new TypedService(typeof(object)) },
                new DelegateActivator(typeof(object), (c, p) => instance)));
            Assert.AreSame(instance, target.Resolve<object>());
        }

        [Test]
        public void RegisterType()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<object>();
            var target = builder.Build();
            object instance = target.Resolve<object>();
            Assert.IsNotNull(instance);
            Assert.IsInstanceOfType(typeof(object), instance);
        }

        [Test]
        public void ResolveUnregistered()
        {
            try
            {
                var target = new Container();
                target.Resolve<object>();
            }
            catch (ComponentNotRegisteredException se)
            {
                Assert.IsTrue(se.Message.Contains("System.Object"));
                return;
            }
            catch (Exception ex)
            {
                Assert.Fail("Expected a ComponentNotRegisteredException, got {0}.", ex);
                return;
            }

            Assert.Fail("Expected a ComponentNotRegisteredException.");
        }

        [Test]
        public void CircularDependency()
        {
            try
            {
                var builder = new ContainerBuilder();
                builder.RegisterDelegate(c => c.Resolve<object>());

                var target = builder.Build();
                target.Resolve<object>();
            }
            catch (DependencyResolutionException de)
            {
                Assert.IsNull(de.InnerException);
                Assert.IsTrue(de.Message.Contains("System.Object -> System.Object"));
                Assert.IsFalse(de.Message.Contains("System.Object -> System.Object -> System.Object"));
                return;
            }
            catch (Exception ex)
            {
                Assert.Fail("Expected a DependencyResolutionException, got {0}.", ex);
                return;
            }

            Assert.Fail("Expected a DependencyResolutionException.");
        }

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

        static ReflectionActivator CreateActivator(Type implementation)
        {
            return new ReflectionActivator(
                implementation,
                new BindingFlagsConstructorFinder(BindingFlags.Public),
                new MostParametersConstructorSelector(),
                Enumerable.Empty<Parameter>());
        }

        [Test]
        public void DisposeOrder1()
        {
            var target = new Container();

            target.ComponentRegistry.Register(CreateSingletonRegistration(
                new[] { new TypedService(typeof(A)) },
                CreateActivator(typeof(A))));

            target.ComponentRegistry.Register(CreateSingletonRegistration(
                new[] { new TypedService(typeof(B)) },
                CreateActivator(typeof(B))));

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

            target.ComponentRegistry.Register(CreateSingletonRegistration(
                new Service[] { new TypedService(typeof(A)) },
                CreateActivator(typeof(A))));

            target.ComponentRegistry.Register(CreateSingletonRegistration(
                new Service[] { new TypedService(typeof(B)) },
                CreateActivator(typeof(B))));

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
            builder.RegisterType<A>().InstancePerLifetimeScope();

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

        class ObjectRegistrationSource : IRegistrationSource
        {
            public bool TryGetRegistration(Service service, Func<Service, bool> registeredServicesTest, out IComponentRegistration registration)
            {
                Assert.AreEqual(typeof(object), ((TypedService)service).ServiceType);
                registration = CreateSingletonRegistration(
                    new[] { service },
                    CreateActivator(typeof(object)));
                return true;
            }
        }

        [Test]
        public void AddRegistrationInServiceNotRegistered()
        {
            var c = new Container();

            Assert.IsFalse(c.IsRegistered<object>());

            c.ComponentRegistry.AddRegistrationSource(new ObjectRegistrationSource());

            Assert.IsTrue(c.IsRegistered<object>());

            var o = c.Resolve<object>();
            Assert.IsNotNull(o);
        }

        [Test]
        public void ResolveByName()
        {
            string name = "name";

            var r = CreateSingletonRegistration(
                new Service[] { new NamedService(name) },
                CreateActivator(typeof(object)));

            var c = new Container();
            c.ComponentRegistry.Register(r);

            object o;

            Assert.IsTrue(c.TryResolve(name, out o));
            Assert.IsNotNull(o);

            Assert.IsFalse(c.IsRegistered<object>());
        }

        class DependsByCtor
        {
            public DependsByCtor(DependsByProp o)
            {
                Dep = o;
            }

            public DependsByProp Dep { get; private set; }
        }

        class DependsByProp
        {
            public DependsByCtor Dep { get; set; }
        }

        [Test]
        public void CtorPropDependencyOkOrder1()
        {
            var cb = new ContainerBuilder();
            cb.RegisterType<DependsByCtor>().SingleSharedInstance();
            cb.RegisterType<DependsByProp>().SingleSharedInstance().PropertiesAutowired(true);

            var c = cb.Build();
            var dbp = c.Resolve<DependsByProp>();

            Assert.IsNotNull(dbp.Dep);
            Assert.IsNotNull(dbp.Dep.Dep);
            Assert.AreSame(dbp, dbp.Dep.Dep);
        }

        [Test]
        public void CtorPropDependencyOkOrder2()
        {
            var cb = new ContainerBuilder();
            cb.RegisterType<DependsByCtor>().SingleSharedInstance();
            cb.RegisterType<DependsByProp>().SingleSharedInstance().PropertiesAutowired(true);

            var c = cb.Build();
            var dbc = c.Resolve<DependsByCtor>();

            Assert.IsNotNull(dbc.Dep);
            Assert.IsNotNull(dbc.Dep.Dep);
            Assert.AreSame(dbc, dbc.Dep.Dep);
        }

        [Test]
        [ExpectedException(typeof(DependencyResolutionException))]
        public void CtorPropDependencyFactoriesOrder1()
        {
            var cb = new ContainerBuilder();
            cb.RegisterType<DependsByCtor>();
            cb.RegisterType<DependsByProp>().PropertiesAutowired(true);

            var c = cb.Build();
            var dbp = c.Resolve<DependsByProp>();
        }

        [Test]
        [ExpectedException(typeof(DependencyResolutionException))]
        public void CtorPropDependencyFactoriesOrder2()
        {
            var cb = new ContainerBuilder();
            var ac = 0;
            cb.RegisterType<DependsByCtor>().OnActivating(e => { ++ac; });
            cb.RegisterType<DependsByProp>().OnActivating(e => { ++ac; })
                .PropertiesAutowired(true);

            var c = cb.Build();
            var dbc = c.Resolve<DependsByCtor>();

            Assert.AreEqual(2, ac);
        }

        class Parameterised
        {
            public string A { get; private set; }
            public int B { get; private set; }

            public Parameterised(string a, int b)
            {
                A = a;
                B = b;
            }
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
