using Autofac.Builder;
using Autofac.Core;
using Autofac.Core.Registration;
using Xunit;

namespace Autofac.Test.Core.Registration
{
    public class CopyOnWriteRegistryTests
    {
        [Fact]
        public void WhenRegistrationsAreMadeTheyDoNotAffectTheReadRegistry()
        {
            var read = new ComponentRegistry();
            var cow = new CopyOnWriteRegistry(read, () => new ComponentRegistry());
            var registration = RegistrationBuilder.ForType<object>().CreateRegistration();
            cow.Register(registration);

            var objectService = new TypedService(typeof(object));
            Assert.True(cow.IsRegistered(objectService));
            Assert.False(read.IsRegistered(objectService));
        }

        [Fact]
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
            cow.TryGetRegistration(new TypedService(typeof(object)), out unused);

            Assert.False(writeRegistryCreated);
        }

        [Fact]
        public void RegistrationsMadeByUpdatingAChildScopeDoNotAppearInTheParentScope()
        {
            var container = new ContainerBuilder().Build();
            var childScope = container.BeginLifetimeScope();
            var updater = new ContainerBuilder();
            updater.RegisterType<object>();
            updater.UpdateRegistry(childScope.ComponentRegistry);
            Assert.True(childScope.IsRegistered<object>());
            Assert.False(container.IsRegistered<object>());
        }
    }
}
