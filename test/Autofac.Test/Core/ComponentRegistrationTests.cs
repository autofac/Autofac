using System;
using Xunit;
using Autofac.Core;

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

    }
}
