using System;

using Autofac.Core.Lifetime;

namespace Autofac.Core.Registration
{
    /// <summary>
    /// A <see cref="IRegisteredServicesTracker" /> where the singletons are scoped with the provided <see cref="IComponentLifetime" />.
    /// </summary>
    internal class ScopeRestrictedRegisteredServicesTracker : DefaultRegisteredServicesTracker
    {
        private readonly IComponentLifetime _restrictedRootScopeLifetime;

        /// <summary>
        /// Initializes a new instance of the <see cref="ScopeRestrictedRegisteredServicesTracker"/> class.
        /// </summary>
        /// <param name="restrictedRootScopeLifetime">The scope to which registrations are restricted.</param>
        internal ScopeRestrictedRegisteredServicesTracker(IComponentLifetime restrictedRootScopeLifetime)
        {
            _restrictedRootScopeLifetime = restrictedRootScopeLifetime;
        }

        /// <summary>
        /// Adds a registration to the list of registered services.
        /// </summary>
        /// <param name="registration">The registration to add.</param>
        /// <param name="preserveDefaults">Indicates whehter the defaults should be preserved.</param>
        /// <param name="originatedFromSource">Indicates whether this is an explicitly added registration or that it has been added by a different source.</param>
        public override void AddRegistration(IComponentRegistration registration, bool preserveDefaults, bool originatedFromSource = false)
        {
            if (registration == null)
            {
                throw new ArgumentNullException(nameof(registration));
            }

            var toRegister = registration;

            if (registration.Lifetime is RootScopeLifetime && !(registration is ExternalComponentRegistration))
            {
#pragma warning disable CA2000 // Dispose objects before losing scope
                toRegister = new ComponentRegistrationLifetimeDecorator(registration, _restrictedRootScopeLifetime);
#pragma warning restore CA2000 // Dispose objects before losing scope
            }

            base.AddRegistration(toRegister, preserveDefaults, originatedFromSource);
        }
    }
}
