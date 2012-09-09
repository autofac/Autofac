using System;
using NUnit.Framework;
using Autofac.Core;

namespace Autofac.Tests.Core
{
    [TestFixture]
    public class ComponentRegistrationTests
    {
        [Test]
        public void Constructor_DetectsNullsAmongServices()
        {
            Assertions.AssertThrows<ArgumentException>(delegate
            {
                var services = new Service[] { new TypedService(typeof(object)), null };
                Factory.CreateSingletonRegistration(services, Factory.CreateProvidedInstanceActivator(new object()));
            });
        }

    }
}
