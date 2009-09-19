using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Autofac.Core.Registration;
using Autofac.Core;

namespace Autofac.Tests
{
    [TestFixture]
    public class ComponentRegistryTests
    {
        class ObjectRegistrationSource : IRegistrationSource
        {
            public bool TryGetRegistration(Service service, Func<Service, bool> registeredServicesTest, out IComponentRegistration registration)
            {
                var objectService = new TypedService(typeof(object));
                if (service == objectService)
                {
                    registration = Fixture.CreateSingletonObjectRegistration();
                    return true;
                }
                else
                {
                    registration = null;
                    return false;
                }
            }
        }

        [Test]
        public void RegistrationsForServiceIncludeDynamicSources()
        {
            var registry = new ComponentRegistry();
            registry.AddRegistrationSource(new ObjectRegistrationSource());
            Assert.IsFalse(registry.Registrations.Where(
                r => r.Services.Contains(new TypedService(typeof(object)))).Any());
            Assert.IsTrue(registry.RegistrationsFor(new TypedService(typeof(object))).Count() == 1);
        }

        [Test]
        public void ComponentRegisteredEventFired()
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

            var registration = Fixture.CreateSingletonObjectRegistration();
            registry.Register(registration);

            Assert.AreEqual(1, eventCount);
            Assert.IsNotNull(eventSender);
            Assert.AreSame(registry, eventSender);
            Assert.IsNotNull(args);
            Assert.AreSame(registry, args.ComponentRegistry);
            Assert.AreSame(registration, args.ComponentRegistration);
        }

        [Test]
        public void DefaultRegistrationIsForMostRecent()
        {
            var r1 = Fixture.CreateSingletonObjectRegistration();
            var r2 = Fixture.CreateSingletonObjectRegistration();

            var registry = new ComponentRegistry();

            registry.Register(r1);
            registry.Register(r2);

            IComponentRegistration defaultRegistration;
            Assert.IsTrue(registry.TryGetRegistration(new TypedService(typeof(object)), out defaultRegistration));
            Assert.AreSame(r2, defaultRegistration);
        }

        [Test]
        public void DefaultRegistrationFalseWhenAbsent()
        {
            var registry = new ComponentRegistry();
            IComponentRegistration unused;
            Assert.IsFalse(registry.TryGetRegistration(new TypedService(typeof(object)), out unused));
        }

        [Test]
        public void DefaultRegistrationSuppliedDynamically()
        {
            var registry = new ComponentRegistry();
            registry.AddRegistrationSource(new ObjectRegistrationSource());
            IComponentRegistration registration;
            Assert.IsTrue(registry.TryGetRegistration(new TypedService(typeof(object)), out registration));
        }
    }
}
