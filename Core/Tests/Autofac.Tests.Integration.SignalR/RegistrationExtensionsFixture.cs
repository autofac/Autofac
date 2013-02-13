using System.Reflection;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using NUnit.Framework;
using Autofac.Integration.SignalR;

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
    }

    public class TestHub : Hub
    {
    }
}