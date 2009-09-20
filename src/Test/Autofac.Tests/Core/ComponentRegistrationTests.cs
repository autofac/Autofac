using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
                Factory.CreateSingletonRegistration(
                new Service[] { new TypedService(typeof(object)), null },
                Factory.CreateProvidedInstanceActivator(new object()));
            });
        }

    }
}
