// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using Autofac.Builder;
using Autofac.Core;
using Autofac.Core.Lifetime;
using Autofac.Core.Registration;
using Xunit;

namespace Autofac.Test.Core.Registration
{
    public sealed class ScopeRestrictedRegisteredServicesTrackerTests
    {
        private static readonly IComponentRegistration ObjectRegistration =
            RegistrationBuilder.ForType<object>().SingleInstance().CreateRegistration();

        private class ObjectRegistrationSource : IRegistrationSource
        {
            public IEnumerable<IComponentRegistration> RegistrationsFor(
                Service service,
                Func<Service, IEnumerable<ServiceRegistration>> registrationAccessor)
            {
                yield return ObjectRegistration;
            }

            public bool IsAdapterForIndividualComponents => false;
        }

        [Fact]
        public void SingletonsFromRegistrationSourceAreWrappedWithLifetimeDecorator()
        {
            var restrictedRootScopeLifetime = new MatchingScopeLifetime(new object());
            var tracker = new ScopeRestrictedRegisteredServicesTracker(restrictedRootScopeLifetime);

            var builder = new ComponentRegistryBuilder(tracker, new Dictionary<string, object>());

            builder.AddRegistrationSource(new ObjectRegistrationSource());

            var typedService = new TypedService(typeof(object));
            var registry = builder.Build();
            registry.TryGetRegistration(typedService, out IComponentRegistration registration);

            Assert.IsType<ComponentRegistrationLifetimeDecorator>(registration);
        }

        [Fact]
        public void SingletonsRegisteredDirectlyAreWrappedWithLifetimeDecorator()
        {
            var restrictedRootScopeLifetime = new MatchingScopeLifetime(new object());
            var tracker = new ScopeRestrictedRegisteredServicesTracker(restrictedRootScopeLifetime);

            var builder = new ComponentRegistryBuilder(tracker, new Dictionary<string, object>());

            builder.Register(ObjectRegistration);

            var registry = builder.Build();

            var typedService = new TypedService(typeof(object));
            registry.TryGetRegistration(typedService, out IComponentRegistration registration);

            Assert.IsType<ComponentRegistrationLifetimeDecorator>(registration);
        }
    }
}
