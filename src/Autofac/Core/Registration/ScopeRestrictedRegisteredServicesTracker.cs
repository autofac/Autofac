using System;

using Autofac.Core.Lifetime;

namespace Autofac.Core.Registration
{
    internal class ScopeRestrictedRegisteredServicesTracker : DefaultRegisteredServicesTracker
    {
        private readonly IComponentLifetime _restrictedRootScopeLifetime;

        public ScopeRestrictedRegisteredServicesTracker(IComponentLifetime restrictedRootScopeLifetime)
        {
            _restrictedRootScopeLifetime = restrictedRootScopeLifetime;
        }

        public override void AddRegistration(IComponentRegistration registration, bool preserveDefaults, bool originatedFromSource = false)
        {
            if (registration == null) throw new ArgumentNullException(nameof(registration));

            var toRegister = registration;

            if (registration.Lifetime is RootScopeLifetime)
                toRegister = new ComponentRegistrationLifetimeDecorator(registration, _restrictedRootScopeLifetime);

            base.AddRegistration(toRegister, preserveDefaults, originatedFromSource);
        }
    }
}