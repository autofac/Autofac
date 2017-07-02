using System;
using System.Collections.Generic;
using Autofac.Builder;
using Autofac.Core;
using Autofac.Core.Registration;
using Xunit;

namespace Autofac.Test.Core.Registration
{
    public sealed class ScopeRestrictedRegistryTests
    {
        private static readonly IComponentRegistration ObjectRegistration =
            RegistrationBuilder.ForType<object>().SingleInstance().CreateRegistration();

        private class ObjectRegistrationSource : IRegistrationSource
        {
            public IEnumerable<IComponentRegistration> RegistrationsFor(
                Service service,
                Func<Service, IEnumerable<IComponentRegistration>> registrationAccessor)
            {
                yield return ObjectRegistration;
            }

            public bool IsAdapterForIndividualComponents => false;
        }

        [Fact]
        public void SingletonsFromRegistrationSourceAreWrappedWithLifetimeDecorator()
        {
            var registry = new ScopeRestrictedRegistry(new object(), new Dictionary<string, object>());

            registry.AddRegistrationSource(new ObjectRegistrationSource());

            var typedService = new TypedService(typeof(object));
            registry.TryGetRegistration(typedService, out IComponentRegistration registration);

            Assert.IsType<ComponentRegistrationLifetimeDecorator>(registration);
        }

        [Fact]
        public void SingletonsRegisteredDirectlyAreWrappedWithLifetimeDecorator()
        {
            var registry = new ScopeRestrictedRegistry(new object(), new Dictionary<string, object>());

            registry.Register(ObjectRegistration);

            var typedService = new TypedService(typeof(object));
            registry.TryGetRegistration(typedService, out IComponentRegistration registration);

            Assert.IsType<ComponentRegistrationLifetimeDecorator>(registration);
        }
    }
}
