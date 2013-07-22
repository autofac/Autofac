using System.Reflection;
using Autofac.Core;
using Autofac.Integration.SignalR;
using Microsoft.AspNet.SignalR;
using NUnit.Framework;

namespace Autofac.Tests.Integration.SignalR
{
    [TestFixture]
    public class RegistrationExtensionsFixture
    {
        [Test]
        public void RegisterHubsFindHubInterfaces()
        {
            var builder = new ContainerBuilder();

            builder.RegisterHubs(Assembly.GetExecutingAssembly());

            var container = builder.Build();

            Assert.That(container.IsRegistered<TestHub>(), Is.True);
        }

        [Test]
        public void HubRegistrationsAreExternallyOwned()
        {
            var builder = new ContainerBuilder();
            builder.RegisterHubs(Assembly.GetExecutingAssembly());
            var container = builder.Build();

            var service = new TypedService(typeof(TestHub));
            IComponentRegistration registration;
            container.ComponentRegistry.TryGetRegistration(service, out registration);

            Assert.That(registration.Ownership, Is.EqualTo(InstanceOwnership.ExternallyOwned));
        }
    }

    public class TestHub : Hub
    {
    }
}