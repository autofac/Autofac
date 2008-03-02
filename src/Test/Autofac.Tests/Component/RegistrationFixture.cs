using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using Autofac.Component;
using Autofac.Component.Scope;
using Autofac.Component.Activation;

namespace Autofac.Tests.Component
{
    [TestFixture]
    public class RegistrationFixture
    {
	  	IComponentRegistration CreateRegistration(IEnumerable<Service> services, IActivator activator)
    	{
    		return new Registration(services, activator, new SingletonScope(), InstanceOwnership.Container);
    	}
  	
	  	IComponentRegistration CreateRegistration(IEnumerable<Service> services, IActivator activator, IScope scope)
    	{
    		return new Registration(services, activator, scope, InstanceOwnership.Container);
    	}
  	
  		[Test]
        public void Construct()
        {
            var services = new Service[] { new TypedService(typeof(object)), new TypedService(typeof(string)) };

            var target = CreateRegistration(
                                 services,
                                 new ProvidedInstanceActivator("Hello"),
                                 new ContainerScope());

            var actualServices = new List<Service>(target.Services);

            Assert.AreEqual(services.Length, actualServices.Count);

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
            target.ResolveInstance(container, ActivationParameters.Empty, new Disposer(), out newInstance);
            target.ResolveInstance(container, ActivationParameters.Empty, new Disposer(), out newInstance);
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
            target.ResolveInstance(container, ActivationParameters.Empty, new Disposer(), out newInstance);

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
            target.ResolveInstance(container, ActivationParameters.Empty, new Disposer(), out newInstance);
            target.InstanceActivated(container, instance);

			Assert.IsTrue(eventFired);
		}

        [Test]
        public void ExtendedPropertiesExistInSubcontext()
        {
            var target = CreateRegistration(
                            new Service[] { new TypedService(typeof(object)) },
                            new ReflectionActivator(typeof(object)),
                            new ContainerScope());

            var property = new KeyValuePair<string, object>("name", "value");

            target.ExtendedProperties.Add(property);

            Assert.AreEqual(1, target.ExtendedProperties.Count);
            Assert.IsTrue(target.ExtendedProperties.Contains(property));

            IComponentRegistration subcontext;
            Assert.IsTrue(target.DuplicateForNewContext(out subcontext));

            Assert.AreEqual(1, subcontext.ExtendedProperties.Count);
            Assert.IsTrue(subcontext.ExtendedProperties.Contains(property));
        }
	}
}
