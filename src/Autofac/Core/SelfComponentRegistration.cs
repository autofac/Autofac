using System;
using System.Collections.Generic;

using Autofac.Core.Activators.Delegate;
using Autofac.Core.Lifetime;
using Autofac.Core.Registration;

namespace Autofac.Core
{
    internal sealed class SelfComponentRegistration : ComponentRegistration
    {
        public SelfComponentRegistration()
            : base(
                LifetimeScope.SelfRegistrationId,
                new DelegateActivator(typeof(LifetimeScope), (c, p) => { throw new InvalidOperationException(ContainerResources.SelfRegistrationCannotBeActivated); }),
                new CurrentScopeLifetime(),
                InstanceSharing.Shared,
                InstanceOwnership.ExternallyOwned,
                new Service[] { new TypedService(typeof(ILifetimeScope)), new TypedService(typeof(IComponentContext)) },
                new Dictionary<string, object>())
        {
        }
    }
}