using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Autofac.Component;
using Autofac.Component.Activation;
using Autofac.Component.Scope;
using Autofac.Builder;

namespace Autofac.Tests
{
    [TestFixture]
    public class ContainerFixture
    {
    	static IComponentRegistration CreateRegistration(IEnumerable<Service> services, IActivator activator)
    	{
            return CreateRegistration(services, activator, new SingletonScope());
    	}
  	
	  	static IComponentRegistration CreateRegistration(IEnumerable<Service> services, IActivator activator, IScope scope)
    	{
            return new Registration(
                new Descriptor(
                    new UniqueService(),
                    services,
                    typeof(object)),
                activator,
                scope,
                InstanceOwnership.Container);
        }
	  	
		[Test]
		public void ResolveOptional()
		{
			var target = new Container();
			target.RegisterComponent(CreateRegistration(
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
        public void RegisterInstance()
        {
            var builder = new ContainerBuilder();

            var instance = new object();

            builder.Register(instance);

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

			target.RegisterComponent(CreateRegistration(
				new[] { new TypedService(typeof(object)) },
				new ProvidedInstanceActivator(instance1)));

			target.RegisterComponent(CreateRegistration(
				new[] { new TypedService(typeof(object)) },
				new ProvidedInstanceActivator(instance2)));

            Assert.AreSame(instance2, target.Resolve<object>());
        }

        [Test]
        public void RegisterComponent()
        {
            var registration = CreateRegistration(
                new[] { new TypedService(typeof(object)), new TypedService(typeof(string)) },
                new ProvidedInstanceActivator("Hello"),
                new ContainerScope());

            var target = new Container();

            target.RegisterComponent(registration);

            Assert.IsTrue(target.IsRegistered<object>());
            Assert.IsTrue(target.IsRegistered<string>());
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegisterComponentNull()
        {
            var target = new Container();

            target.RegisterComponent(null);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void RegisterComponentNullService()
        {
            var registration = CreateRegistration(
                new Service[] { new TypedService(typeof(object)), null },
                new ProvidedInstanceActivator(new object()),
                new ContainerScope());

            var target = new Container();

            target.RegisterComponent(registration);
        }

        [Test]
        public void RegisterDelegate()
        {
            object instance = new object();
            var target = new Container();
			target.RegisterComponent(CreateRegistration(
				new[] { new TypedService(typeof(object)) },
				new DelegateActivator((c, p) => instance)));
			Assert.AreSame(instance, target.Resolve<object>());
        }

        [Test]
        public void RegisterType()
        {
            var builder = new ContainerBuilder();
			builder.Register<object>();
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
				builder.Register(c => c.Resolve<object>());

				var target = builder.Build();
                target.Resolve<object>();
            }
            catch (DependencyResolutionException de)
            {
                Assert.IsNull(de.InnerException);
                Assert.IsTrue(de.Message.Contains("System.Object -> System.Object"));
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

        class B : DisposeTracker {
            public A A;

            public B(A a) {
                A = a;
            }
        }
        
        interface IC { }

        class C : DisposeTracker {
            public B B;

            public C(B b) {
                B = b;
            }
        }
        
        interface ID { }

        class CD : DisposeTracker, IC, ID {
            public A A;
            public B B;

            public CD(A a, B b) {
                A = a;
                B = b;
            }
        }

        class E : DisposeTracker {
            public B B;
            public IC C;

            public E(B b, IC c) {
                B = b;
                C = c;
            }
        }

        class F {
            public IList<A> AList;
            public F(IList<A> aList) {
                AList = aList;
            }
        }

        #endregion

        [Test]
        [ExpectedException(typeof(DependencyResolutionException))]
        public void InnerCannotResolveOuterDependencies()
        {
            var outerBuilder = new ContainerBuilder();
            outerBuilder.Register<B>()
                .WithScope(InstanceScope.Singleton);
            var outer = outerBuilder.Build();

            var innerBuilder = new ContainerBuilder();
            innerBuilder.Register<C>();
            innerBuilder.Register<A>();
            var inner = outer.CreateInnerContainer();
            innerBuilder.Build(inner);

            var unused = inner.Resolve<C>();
        }

        [Test]
        public void OuterInstancesCannotReferenceInner()
        {
            var builder = new ContainerBuilder();
            builder.Register<A>().WithScope(InstanceScope.Container);
            builder.Register<B>().WithScope(InstanceScope.Factory);

            var outer = builder.Build();

            var inner = outer.CreateInnerContainer();

            var outerB = outer.Resolve<B>();
            var innerB = inner.Resolve<B>();
            var outerA = outer.Resolve<A>();
            var innerA = inner.Resolve<A>();

            Assert.AreSame(innerA, innerB.A);
            Assert.AreSame(outerA, outerB.A);
            Assert.AreNotSame(innerA, outerA);
            Assert.AreNotSame(innerB, outerB);
        }

        [Test]
        public void IntegrationTest()
        {
            var builder = new ContainerBuilder();

            builder.Register<A>().WithScope(InstanceScope.Singleton);
            builder.Register<CD>().As<IC, ID>().WithScope(InstanceScope.Singleton);
            builder.Register<E>().WithScope(InstanceScope.Singleton);
			builder.Register(ctr => new B(ctr.Resolve<A>()))
				.WithScope(InstanceScope.Factory);

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

			target.RegisterComponent(CreateRegistration(
				new[] { new TypedService(typeof(A)) },
				new ReflectionActivator(typeof(A))));

			target.RegisterComponent(CreateRegistration(
				new[] { new TypedService(typeof(B)) },
				new ReflectionActivator(typeof(B))));

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

			target.RegisterComponent(CreateRegistration(
				new Service[] { new TypedService(typeof(A)) },
				new ReflectionActivator(typeof(A))));

			target.RegisterComponent(CreateRegistration(
				new Service[] { new TypedService(typeof(B)) },
				new ReflectionActivator(typeof(B))));

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

			builder.Register<A>()
                .WithScope(InstanceScope.Singleton);

			var target = builder.Build();

			var context = target.CreateInnerContainer();

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
			var target = new Container();

			target.RegisterComponent(CreateRegistration(
				new Service[] { new TypedService(typeof(A)) },
				new ReflectionActivator(typeof(A)),
				new FactoryScope()));

			var context = target.CreateInnerContainer();

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
			var target = new Container();

			target.RegisterComponent(CreateRegistration(
				new Service[] { new TypedService(typeof(A)) },
				new ReflectionActivator(typeof(A)),
				new ContainerScope()));

			var context = target.CreateInnerContainer();

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
		public void ActivatingFired()
		{
			var instance = new object();
			var container = new Container();
			var registration = CreateRegistration(
							new[] { new TypedService(typeof(object)) },
							new ProvidedInstanceActivator(instance));
			container.RegisterComponent(registration);

			bool eventFired = false;

			container.Activating += (sender, e) =>
			{
				Assert.AreSame(container, sender);
				Assert.AreSame(instance, e.Instance);
				Assert.AreSame(container, e.Context);
				Assert.AreSame(registration, e.Component);
				eventFired = true;
			};

            bool newInstance;
			registration.ResolveInstance(container, ActivationParameters.Empty, new Disposer(), out newInstance);

			Assert.IsTrue(eventFired);
		}

		[Test]
		public void ActivatedFired()
		{
			var instance = new object();
			var container = new Container();
			var registration = CreateRegistration(
							new[] { new TypedService(typeof(object)) },
							new ProvidedInstanceActivator(instance));
			container.RegisterComponent(registration);

			bool eventFired = false;

			container.Activated += (sender, e) =>
			{
				Assert.AreSame(container, sender);
				Assert.AreSame(instance, e.Instance);
				Assert.AreSame(registration, e.Component);
				eventFired = true;
			};

			container.Resolve<object>();

			Assert.IsTrue(eventFired);
		}


		[Test]
		public void ActivatingFiredInSubcontext()
		{
			var cb = new ContainerBuilder();
			cb.Register<object>().WithScope(InstanceScope.Factory);
			var container = cb.Build();

			bool eventFired = false;

			var context = container.CreateInnerContainer();

			container.Activating += (sender, e) =>
			{
				Assert.AreSame(container, sender);
				eventFired = true;
			};

			context.Resolve<object>();

			Assert.IsTrue(eventFired);
		}

		class ObjectRegistrationSource : IRegistrationSource
		{
			public bool TryGetRegistration(Service service, out IComponentRegistration registration)
			{
				Assert.AreEqual(typeof(object), ((TypedService)service).ServiceType);
				registration = CreateRegistration(
					new[] { service },
					new ReflectionActivator(typeof(object)));
				return true;
			}
		}

		[Test]
		public void AddRegistrationInServiceNotRegistered()
		{
			var c = new Container();

			Assert.IsFalse(c.IsRegistered<object>());

			c.AddRegistrationSource(new ObjectRegistrationSource());

			Assert.IsTrue(c.IsRegistered<object>());

			var o = c.Resolve<object>();
			Assert.IsNotNull(o);
		}

        [Test]
        public void ResolveByName()
        {
            string name = "name";

            var r = CreateRegistration(
                new Service[] { new NamedService(name) },
                new ReflectionActivator(typeof(object)));

            var c = new Container();
            c.RegisterComponent(r);

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
            cb.Register<DependsByCtor>();
            cb.Register<DependsByProp>()
                .OnActivated(ActivatedHandler.InjectProperties);

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
            cb.Register<DependsByCtor>();
            cb.Register<DependsByProp>()
                .OnActivated(ActivatedHandler.InjectProperties);

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
            using (cb.SetDefaultScope(InstanceScope.Factory))
            {
                cb.Register<DependsByCtor>();
                cb.Register<DependsByProp>()
                    .OnActivated(ActivatedHandler.InjectProperties);
            }

            var c = cb.Build();
            var dbp = c.Resolve<DependsByProp>();
        }

        [Test]
        [ExpectedException(typeof(DependencyResolutionException))]
        public void CtorPropDependencyFactoriesOrder2()
        {
            var cb = new ContainerBuilder();
            using (cb.SetDefaultScope(InstanceScope.Factory))
            {
                cb.Register<DependsByCtor>();
                cb.Register<DependsByProp>()
                    .OnActivated(ActivatedHandler.InjectProperties);
            }

            var c = cb.Build();
            var dbc = c.Resolve<DependsByCtor>();
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
            cb.Register((c, p) => new Parameterised(p.Get<string>("a"), p.Get<int>("b")));
            var container = cb.Build();
            var aVal = "Hello";
            var bVal = 42;
            var result = container.Resolve<Parameterised>(
                new Parameter("a", aVal),
                new Parameter("b", bVal));
            Assert.IsNotNull(result);
            Assert.AreEqual(aVal, result.A);
            Assert.AreEqual(bVal, result.B);
        }

        [Test]
        public void RegisterParameterisedWithReflection()
        {
            var cb = new ContainerBuilder();
            cb.Register<Parameterised>();
            var container = cb.Build();
            var aVal = "Hello";
            var bVal = 42;
            var result = container.Resolve<Parameterised>(
                new Parameter("a", aVal),
                new Parameter("b", bVal));
            Assert.IsNotNull(result);
            Assert.AreEqual(aVal, result.A);
            Assert.AreEqual(bVal, result.B);
        }

        [Test]
        public void SupportsIServiceProvider()
        {
            var cb = new ContainerBuilder();
            cb.Register<object>();
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
            cb.Register<object>().Named(myName);
            var container = cb.Build();
            var o = container.Resolve<object>(myName);
            Assert.IsNotNull(o);
        }

        [Test]
        public void ComponentRegistrationsExposed()
        {
            var builder = new ContainerBuilder();
            builder.Register<object>();
            builder.Register<object>();
            builder.Register("hello");
            var container = builder.Build();
            var registrations = new List<IComponentRegistration>(container.ComponentRegistrations);
            // The container registers itself :) hence 3 + 1.
            Assert.AreEqual(4, registrations.Count);
            Assert.IsTrue(registrations[0].Descriptor.Services.Contains(new TypedService(typeof(IContainer))));
            Assert.IsTrue(registrations[1].Descriptor.Services.Contains(new TypedService(typeof(object))));
            Assert.IsTrue(registrations[2].Descriptor.Services.Contains(new TypedService(typeof(object))));
            Assert.IsTrue(registrations[3].Descriptor.Services.Contains(new TypedService(typeof(string))));
        }

        [Test]
        public void ComponentRegisteredEventFired()
        {
            object eventSender = null;
            ComponentRegisteredEventArgs args = null;
            var eventCount = 0;

            var container = new Container();
            container.ComponentRegistered += (sender, e) => {
                eventSender = sender;
                args = e;
                ++eventCount;
            };

            var builder = new ContainerBuilder();
            builder.Register<object>();
            builder.Build(container);

            Assert.AreEqual(1, eventCount);
            Assert.IsNotNull(eventSender);
            Assert.AreSame(container, eventSender);
            Assert.IsNotNull(args);
            Assert.AreSame(container, args.Container);
            Assert.IsNotNull(args.ComponentRegistration.Descriptor.Services.FirstOrDefault(
                s => s == new TypedService(typeof(object))));
        }

        [Test]
        public void ComponentRegisteredNotFiredOnNewContext()
        {
            var eventCount = 0;

            var container = new Container();
            container.ComponentRegistered += (sender, e) =>
            {
                ++eventCount;
            };

            var builder = new ContainerBuilder();
            builder.Register<object>().ContainerScoped();
            builder.Build(container);

            var inner = container.CreateInnerContainer();
            inner.Resolve<object>();

            Assert.AreEqual(1, eventCount);
        }

        [Test]
        public void DefaultRegistrationIsForMostRecent()
        {
            var builder = new ContainerBuilder();
            builder.Register<object>().As<object>().Named("first");
            builder.Register<object>().As<object>().Named("second");
            var container = builder.Build();
            
            IComponentRegistration defaultRegistration;
            Assert.IsTrue(container.TryGetDefaultRegistrationFor(new TypedService(typeof(object)), out defaultRegistration));
            Assert.IsTrue(defaultRegistration.Descriptor.Services.Contains(new NamedService("second")));
        }

        [Test]
        public void DefaultRegistrationFalseWhenAbsent()
        {
            var container = new Container();
            IComponentRegistration unused;
            Assert.IsFalse(container.TryGetDefaultRegistrationFor(new TypedService(typeof(object)), out unused));
        }

        [Test]
        public void DefaultRegistrationSuppliedDynamically()
        {
            var container = new Container();
            container.AddRegistrationSource(new ObjectRegistrationSource());
            IComponentRegistration registration;
            Assert.IsTrue(container.TryGetDefaultRegistrationFor(new TypedService(typeof(object)), out registration));
        }
        
        [Test]
        public void IdSameInSubcontext()
        {
        	var builder = new ContainerBuilder();
        	builder.Register<object>().ContainerScoped();
        	
        	var container = builder.Build();
        	IComponentRegistration r1;
        	Assert.IsTrue(container.TryGetDefaultRegistrationFor(new TypedService(typeof(object)), out r1));
        	
        	var inner = container.CreateInnerContainer();
        	IComponentRegistration r2;
        	Assert.IsTrue(inner.TryGetDefaultRegistrationFor(new TypedService(typeof(object)), out r2));
        	
        	Assert.AreNotSame(r1, r2);
            Assert.AreEqual(r1.Descriptor.Id, r2.Descriptor.Id);
        }
    }
}
