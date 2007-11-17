using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using Autofac.Component;
using Autofac.Component.Scope;
using Autofac.Component.Activation;
using Autofac.Component.Registration;

namespace Autofac.Tests.Component.Registration
{
    [TestFixture]
    public class ComponentRegistrationFixture
    {
        [Test]
        public void Construct()
        {
            var services = new[] { typeof(object), typeof(string) };

            var target = new ComponentRegistration(
                                 services,
                                 new ProvidedInstanceActivator("Hello"),
                                 new ContainerScope());

            var actualServices = new List<Type>(target.Services);

            Assert.AreEqual(services.Length, actualServices.Count);

            foreach (Type service in services)
                Assert.IsTrue(actualServices.Contains(service));
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructServicesNull()
        {
            var target = new ComponentRegistration(
                                 null,
                                 new ProvidedInstanceActivator(new object()),
                                 new ContainerScope());
        }

        [Test]
        public void ConstructNoServicesOk()
        {
            var target = new ComponentRegistration(
                                 new Type[] { },
                                 new ProvidedInstanceActivator(new object()),
                                 new ContainerScope());
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructActivatorNull()
        {
            var target = new ComponentRegistration(
                                 new[] { typeof(object) },
                                 null,
                                 new ContainerScope());
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructScopeNull()
        {
            var target = new ComponentRegistration(
                                 new[] { typeof(object) },
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

			public bool TryDuplicateForNewContext(out IScope newScope)
			{
				newScope = null;
				return false;
			}
		}

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void FactoryAndInstanceIncompatible()
        {
            var target = new ComponentRegistration(
                             new[] { typeof(object) },
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
			var target = new ComponentRegistration(
							new[] { typeof(object) },
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
			var target = new ComponentRegistration(
							new[] { typeof(object) },
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
	}
}
