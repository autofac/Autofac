using System;
using System.Collections.Generic;
using System.Linq;
using Autofac.Component;
using Autofac.Component.Activation;
using Autofac.Component.Scope;
using NUnit.Framework;

namespace Autofac.Tests.Component
{
    [TestFixture]
    public class RegistrationFixture
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
        public void Construct()
        {
            var services = new Service[] { new TypedService(typeof(object)), new TypedService(typeof(string)) };

            var target = CreateRegistration(
                                 services,
                                 new ProvidedInstanceActivator("Hello"),
                                 new ContainerScope());

            var actualServices = new List<Service>(target.Descriptor.Services);

            // Includes Id
            Assert.AreEqual(services.Length + 1, actualServices.Count);

            foreach (Service service in services)
                Assert.IsTrue(actualServices.Contains(service));
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructServicesNull()
        {
            var target = CreateRegistration(
                                 null,
                                 new ProvidedInstanceActivator(new object()),
                                 new ContainerScope());
        }

        [Test]
        public void ConstructNoServicesOk()
        {
            var target = CreateRegistration(
                                 new Service[] { },
                                 new ProvidedInstanceActivator(new object()),
                                 new ContainerScope());
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructActivatorNull()
        {
            var target = CreateRegistration(
                                 new Service[] { new TypedService(typeof(object)) },
                                 null,
                                 new ContainerScope());
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructScopeNull()
        {
            var target = CreateRegistration(
                                 new Service[] { new TypedService(typeof(object)) },
                                 new ProvidedInstanceActivator(new object()),
                                 null);
        }

        class DisposeTrackingScope : DisposeTracker, IScope
        {
            public bool InstanceAvailable {
                get { return false; }
            }

            public object GetInstance() { return null; }

            public void SetInstance(object instance) { }

			public bool DuplicateForNewContext(out IScope newScope)
			{
				newScope = null;
				return false;
			}
		}

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void FactoryAndInstanceIncompatible()
        {
            var target = CreateRegistration(
                             new Service[] { new TypedService(typeof(object)) },
                             new ProvidedInstanceActivator(new object()),
                             new FactoryScope());

            var container = new Container();

            bool newInstance;
            target.ResolveInstance(container, Enumerable.Empty<Parameter>(), new Disposer(), out newInstance);
            target.ResolveInstance(container, Enumerable.Empty<Parameter>(), new Disposer(), out newInstance);
        }

		[Test]
		public void ActivatingFired()
		{
			var instance = new object();
			var container = new Container();
			var target = CreateRegistration(
							new Service[] { new TypedService(typeof(object)) },
							new ProvidedInstanceActivator(instance));

			bool eventFired = false;

			target.Activating += (sender, e) =>
			{
				Assert.AreSame(target, sender);
				Assert.AreSame(instance, e.Instance);
				Assert.AreSame(container, e.Context);
				Assert.AreSame(target, e.Component);
				eventFired = true;
			};

            bool newInstance;
            target.ResolveInstance(container, Enumerable.Empty<Parameter>(), new Disposer(), out newInstance);

			Assert.IsTrue(eventFired);
		}

		[Test]
		public void ActivatedFired()
		{
			var instance = new object();
			var container = new Container();
			var target = CreateRegistration(
							new Service[] { new TypedService(typeof(object)) },
							new ProvidedInstanceActivator(instance));

			bool eventFired = false;

			target.Activated += (sender, e) =>
			{
				Assert.AreSame(target, sender);
				Assert.AreSame(instance, e.Instance);
				Assert.AreSame(container, e.Context);
				Assert.AreSame(target, e.Component);
				eventFired = true;
			};

            bool newInstance;
            target.ResolveInstance(container, Enumerable.Empty<Parameter>(), new Disposer(), out newInstance);
            target.InstanceActivated(container, instance);

			Assert.IsTrue(eventFired);
		}

        [Test]
        public void SameDescriptorInSubcontext()
        {
            var target = CreateRegistration(
                            new Service[] { new TypedService(typeof(object)) },
                            new ReflectionActivator(typeof(object)),
                            new ContainerScope());

            IComponentRegistration subcontext;
            Assert.IsTrue(target.DuplicateForNewContext(out subcontext));

            Assert.AreNotSame(target, subcontext);
            Assert.AreSame(target.Descriptor, subcontext.Descriptor);
        }


        [Test]
        public void ActivatingFiredInSubcontext()
        {
            var target = CreateRegistration(
                            new Service[] { new TypedService(typeof(object)) },
                            new ReflectionActivator(typeof(object)),
                            new ContainerScope());

            IComponentRegistration subcontext;
            Assert.IsTrue(target.DuplicateForNewContext(out subcontext));

            int targetEventCount = 0;
            target.Activating += (sender, e) =>
            {
                ++targetEventCount;
            };

            int subContextEventCount = 0;
            subcontext.Activating += (sender, e) =>
            {
                ++subContextEventCount;
            };

            bool newInstance;
            subcontext.ResolveInstance(Context.Empty, Enumerable.Empty<Parameter>(), new Disposer(), out newInstance);

            Assert.AreEqual(1, targetEventCount);
            Assert.AreEqual(1, subContextEventCount);
        }

        [Test]
        public void SameServiceMultipleTimes()
        {
            var target = CreateRegistration(
                new[] { new TypedService(typeof(object)), new TypedService(typeof(object)) },
                new ProvidedInstanceActivator(new object()));
            Assert.AreEqual(
                1, 
                target
                    .Descriptor
                    .Services
                    .OfType<TypedService>()
                    .Where(t => t.ServiceType == typeof(object))
                    .Count());
        }

        [Test]
        public void ToStringDescribesComponent()
        {
            var target = CreateRegistration(
                new[] { new TypedService(typeof(object)) },
                new ProvidedInstanceActivator(new object()));

            Assert.AreEqual(
        		"Implementor = System.Object, Services = [" + target.Descriptor.Id.ToString() +
        		", System.Object], Activator = Provided Instance, Scope = Singleton, Ownership " +
        		"= Container",
                target.ToString());
        }

        [Test]
        public void ParameterChangesThroughPreparing()
        {
            var p1 = "one";
            var p2 = "two";

            var target = CreateRegistration(
                new[] { new TypedService(typeof(object)) },
                new DelegateActivator((c, p) => p.Named<string>("p1") + p.Named<string>("p2")));

            var providedParams = new Parameter[] { new NamedParameter("p1", p1) };

            target.Preparing += (s, e) => e.Parameters = e.Parameters.Append(new NamedParameter("p2", p2));

            bool newInstance;
            var result = target.ResolveInstance(Context.Empty, providedParams, new Disposer(), out newInstance);

            Assert.AreEqual(p1 + p2, result);
        }

        [Test]
        public void InstanceSuppliedThroughPreparing()
        {
            var activatorProvidedInstance = new object();
            var eventProvidedInstance = new object();

            var target = CreateRegistration(
                new[] { new TypedService(typeof(object)) },
                new ProvidedInstanceActivator(activatorProvidedInstance));

            target.Preparing += (s, e) => e.Instance = eventProvidedInstance;

            bool newInstance;
            var result = target.ResolveInstance(Context.Empty, Enumerable.Empty<Parameter>(), new Disposer(), out newInstance);

            Assert.AreEqual(eventProvidedInstance, result);
        }
    }
}
