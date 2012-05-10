using Autofac.Builder;
using Autofac.Core;
using Autofac.Core.Registration;
using NUnit.Framework;

namespace Autofac.Tests.Core.Registration
{
    [TestFixture]
    public class CopyOnWriteRegistryTests
    {
        [Test]
        public void WhenRegistrationsAreMadeTheyDoNotAffectTheReadRegistry()
        {
            var read = new ComponentRegistry();
            var cow = new CopyOnWriteRegistry(read, () => new ComponentRegistry());
            var registration = RegistrationBuilder.ForType<object>().CreateRegistration();
            cow.Register(registration);

            var objectService = new TypedService(typeof (object));
            Assert.That(cow.IsRegistered(objectService));
            Assert.That(!read.IsRegistered(objectService));
        }

        [Test]
        public void WhenReadingTheWriteRegistryIsNotCreated()
        {
            var writeRegistryCreated = false;
            var read = new ComponentRegistry();
            var cow = new CopyOnWriteRegistry(read, () =>
            {
                writeRegistryCreated = true;
                return new ComponentRegistry();
            });

            IComponentRegistration unused;
            cow.TryGetRegistration(new TypedService(typeof (object)), out unused);

            Assert.IsFalse(writeRegistryCreated);
        }

        [Test]
        public void RegistrationsMadeByUpdatingAChildScopeDoNotAppearInTheParentScope()
        {
            var container = new ContainerBuilder().Build();
            var childScope = container.BeginLifetimeScope();
            var updater = new ContainerBuilder();
            updater.RegisterType<object>();
            updater.Update(childScope.ComponentRegistry);
            Assert.IsTrue(childScope.IsRegistered<object>());
            Assert.IsFalse(container.IsRegistered<object>());
        }
    }
}
