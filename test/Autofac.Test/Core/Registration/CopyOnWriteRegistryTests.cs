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
            var read = Factory.CreateEmptyComponentRegistry();
            var cowBuilder = new CopyOnWriteRegistryBuilder(read, () => Factory.CreateEmptyComponentRegistryBuilder());
            var registration = RegistrationBuilder.ForType<object>().CreateRegistration();
            cowBuilder.Register(registration);
            var cow = cowBuilder.Build();

            var objectService = new TypedService(typeof(object));
            Assert.True(cow.IsRegistered(objectService));
            Assert.False(read.IsRegistered(objectService));
        }

        [Fact]
        public void WhenReadingTheWriteRegistryIsNotCreated()
        {
            var writeRegistryCreated = false;
            var read = Factory.CreateEmptyComponentRegistry();
            var cowBuilder = new CopyOnWriteRegistryBuilder(read, () =>
            {
                writeRegistryCreated = true;
                return Factory.CreateEmptyComponentRegistryBuilder();
            });

            var registry = cowBuilder.Build();
            IComponentRegistration unused;
            registry.TryGetRegistration(new TypedService(typeof(object)), out unused);

            Assert.False(writeRegistryCreated);
        }

        [Fact]
        public void RegistrationsMadeByUpdatingAChildScopeDoNotAppearInTheParentScope()
        {
            var container = new ContainerBuilder().Build();
            var childScope = container.BeginLifetimeScope(x => x.RegisterType<object>());
            Assert.True(childScope.IsRegistered<object>());
            Assert.False(container.IsRegistered<object>());
        }

        [Fact]
        public void OnlyRegistrationsMadeOnTheRegistryAreDisposedWhenTheRegistryIsDisposed()
        {
            var componentRegistration = Mocks.GetComponentRegistration();
            var readOnlyRegistryBuilder = Factory.CreateEmptyComponentRegistryBuilder();
            readOnlyRegistryBuilder.Register(componentRegistration);
            var readOnlyRegistry = readOnlyRegistryBuilder.Build();
            var cowBuilder = new CopyOnWriteRegistryBuilder(readOnlyRegistry, () => Factory.CreateEmptyComponentRegistryBuilder());
            var nestedComponentRegistration = Mocks.GetComponentRegistration();
            cowBuilder.Register(nestedComponentRegistration);
            var cow = cowBuilder.Build();
            cow.Dispose();
            Assert.False(componentRegistration.IsDisposed);
            Assert.True(nestedComponentRegistration.IsDisposed);
        }
    }
}
