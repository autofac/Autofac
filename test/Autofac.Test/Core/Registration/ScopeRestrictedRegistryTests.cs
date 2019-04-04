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
            IComponentRegistryBuilder builder = new ScopeRestrictedRegistryBuilder(new object(), new Dictionary<string, object>());

            builder.AddRegistrationSource(new ObjectRegistrationSource());

            var typedService = new TypedService(typeof(object));
            var registry = builder.Build();
            registry.TryGetRegistration(typedService, out IComponentRegistration registration);

            Assert.IsType<ComponentRegistrationLifetimeDecorator>(registration);
        }

        [Fact]
        public void SingletonsRegisteredDirectlyAreWrappedWithLifetimeDecorator()
        {
            IComponentRegistryBuilder builder = new ScopeRestrictedRegistryBuilder(new object(), new Dictionary<string, object>());

            builder.Register(ObjectRegistration);

            var registry = builder.Build();

            var typedService = new TypedService(typeof(object));
            registry.TryGetRegistration(typedService, out IComponentRegistration registration);

            Assert.IsType<ComponentRegistrationLifetimeDecorator>(registration);
        }
    }
}
