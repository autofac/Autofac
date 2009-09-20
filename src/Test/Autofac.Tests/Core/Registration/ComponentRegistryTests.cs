using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Autofac.Core.Registration;
using Autofac.Core;
using Autofac.Tests.Scenarios.RegistrationSources;

namespace Autofac.Tests.Core.Registration
{
    [TestFixture]
    public class ComponentRegistryTests
    {
        [Test]
        public void Register_DoesNotAcceptNull()
        {
            var registry = new ComponentRegistry();
            Assertions.AssertThrows<ArgumentNullException>(delegate
            {
                registry.Register(null);
            });
        }

        [Test]
        public void RegistrationsForServiceIncludeDynamicSources_WhenNoOtherImplementationsRegistered()
        {
            var registry = new ComponentRegistry();
            registry.AddRegistrationSource(new ObjectRegistrationSource());
            Assert.IsFalse(registry.Registrations.Where(
                r => r.Services.Contains(new TypedService(typeof(object)))).Any());
            Assert.IsTrue(registry.RegistrationsFor(new TypedService(typeof(object))).Count() == 1);
        }

        [Test]
        [Ignore("Not implemented")]
        public void RegistrationsForServiceIncludeDynamicSources_WhenOtherImplementationRegistered()
        {
            var registry = new ComponentRegistry();
            registry.Register(Factory.CreateSingletonObjectRegistration());
            registry.AddRegistrationSource(new ObjectRegistrationSource());
            Assert.IsTrue(registry.RegistrationsFor(new TypedService(typeof(object))).Count() == 2);
        }

        [Test]
        public void WhenRegistrationIsMad_ComponentRegisteredEventFired()
        {
            object eventSender = null;
            ComponentRegisteredEventArgs args = null;
            var eventCount = 0;

            var registry = new ComponentRegistry();
            registry.Registered += (sender, e) =>
            {
                eventSender = sender;
                args = e;
                ++eventCount;
            };

            var registration = Factory.CreateSingletonObjectRegistration();
            registry.Register(registration);

            Assert.AreEqual(1, eventCount);
            Assert.IsNotNull(eventSender);
            Assert.AreSame(registry, eventSender);
            Assert.IsNotNull(args);
            Assert.AreSame(registry, args.ComponentRegistry);
            Assert.AreSame(registration, args.ComponentRegistration);
        }

        [Test]
        public void WhenMultipleProvidersOfServiceExist_DefaultRegistrationIsMostRecent()
        {
            var r1 = Factory.CreateSingletonObjectRegistration();
            var r2 = Factory.CreateSingletonObjectRegistration();

            var registry = new ComponentRegistry();

            registry.Register(r1);
            registry.Register(r2);

            IComponentRegistration defaultRegistration;
            Assert.IsTrue(registry.TryGetRegistration(new TypedService(typeof(object)), out defaultRegistration));
            Assert.AreSame(r2, defaultRegistration);
        }

        [Test]
        public void WhenNoImplementers_TryGetRegistrationReturnsFalse()
        {
            var registry = new ComponentRegistry();
            IComponentRegistration unused;
            Assert.IsFalse(registry.TryGetRegistration(new TypedService(typeof(object)), out unused));
        }

        [Test]
        public void WhenNoImplementerIsDirectlyRegistered_RegistrationCanBeProvidedDynamically()
        {
            var registry = new ComponentRegistry();
            registry.AddRegistrationSource(new ObjectRegistrationSource());
            IComponentRegistration registration;
            Assert.IsTrue(registry.TryGetRegistration(new TypedService(typeof(object)), out registration));
        }
    }
}
