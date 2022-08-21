// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac.Core.Lifetime;

namespace Autofac.Core.Registration;

/// <summary>
/// A <see cref="IRegisteredServicesTracker" /> where the singletons are scoped with the provided <see cref="IComponentLifetime" />.
/// </summary>
internal sealed class ScopeRestrictedRegisteredServicesTracker : DefaultRegisteredServicesTracker
{
    private readonly IComponentLifetime _restrictedRootScopeLifetime;

    /// <summary>
    /// Initializes a new instance of the <see cref="ScopeRestrictedRegisteredServicesTracker"/> class.
    /// </summary>
    /// <param name="restrictedRootScopeLifetime">The scope to which registrations are restricted.</param>
    internal ScopeRestrictedRegisteredServicesTracker(IComponentLifetime restrictedRootScopeLifetime, ReflectionCache reflectionCache)
        : base(reflectionCache)
    {
        _restrictedRootScopeLifetime = restrictedRootScopeLifetime;
    }

    public override ReflectionCache ReflectionCache
    {
        get => base.ReflectionCache;

        // The reason for this exception (preventing reflection cache modification in nested scopes),
        // is that even if you give a new reflection cache in a nested scope, not all the resolves that happen in
        // that scope will use that cache. Registration Sources in parent scopes will use the original
        // cache, leading to user confusion. Since there isn't a strong usecase for replacing the reflection cache
        // in nested scopes (as opposed to clearing specific Assemblies from the top-level cache), it makes
        // most sense to explicitly prevent the override to reduce confusion.
        set => throw new InvalidOperationException("The reflection cache for a container cannot be changed in nested lifetime scopes.");
    }

    /// <inheritdoc/>
    public override void AddRegistration(IComponentRegistration registration, bool preserveDefaults, bool originatedFromSource = false)
    {
        if (registration == null)
        {
            throw new ArgumentNullException(nameof(registration));
        }

        var toRegister = registration;

        if (registration.Lifetime is RootScopeLifetime && registration is not ExternalComponentRegistration)
        {
#pragma warning disable CA2000 // Dispose objects before losing scope
            toRegister = new ComponentRegistrationLifetimeDecorator(registration, _restrictedRootScopeLifetime);
#pragma warning restore CA2000 // Dispose objects before losing scope
        }

        base.AddRegistration(toRegister, preserveDefaults, originatedFromSource);
    }
}
