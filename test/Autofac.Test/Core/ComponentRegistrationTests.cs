using System;
using System.Collections.Generic;
using System.Linq;
using Autofac.Builder;
using Autofac.Core;
using Autofac.Core.Registration;
using Xunit;

namespace Autofac.Test.Core
{
    public class ComponentRegistrationTests
    {
        [Fact]
        public void Constructor_DetectsNullsAmongServices()
        {
            Assert.Throws<ArgumentException>(delegate
            {
                var services = new Service[] { new TypedService(typeof(object)), null };
                Factory.CreateSingletonRegistration(services, Factory.CreateProvidedInstanceActivator(new object()));
            });
        }

        [Fact]
        public void ShouldHaveRegistrationOrderMetadataKey()
        {
            var services = new Service[] { new TypedService(typeof(object)) };

            var registration = Factory.CreateSingletonRegistration(services, Factory.CreateProvidedInstanceActivator(new object()));

            Assert.Contains(MetadataKeys.RegistrationOrderMetadataKey, registration.Metadata.Keys);
        }

        [Fact]
        public void ShouldHaveAscendingRegistrationOrderMetadataValue()
        {
            var services = new Service[] { new TypedService(typeof(string)) };
            var registration1 = Factory.CreateSingletonRegistration(services, Factory.CreateProvidedInstanceActivator("s1"));
            var registration2 = Factory.CreateSingletonRegistration(services, Factory.CreateProvidedInstanceActivator("s2"));
            var registration3 = Factory.CreateSingletonRegistration(services, Factory.CreateProvidedInstanceActivator("s3"));
            var registrations = new List<IComponentRegistration> { registration2, registration3, registration1 };

            var orderedRegistrations = registrations.OrderBy(cr => cr.GetRegistrationOrder()).ToArray();

            Assert.Same(registration1, orderedRegistrations[0]);
            Assert.Same(registration2, orderedRegistrations[1]);
            Assert.Same(registration3, orderedRegistrations[2]);
        }
    }
}
