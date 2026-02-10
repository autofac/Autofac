// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac.Builder;
using Autofac.Core;
using Autofac.Core.Activators.Delegate;
using Autofac.Core.Registration;
using Autofac.Util;

namespace Autofac.Features.KeyedServices;

/// <summary>
/// Provides fallback registrations for keyed services that can be satisfied by <see cref="KeyedService.AnyKey"/>.
/// </summary>
internal sealed class AnyKeyRegistrationSource : IRegistrationSource
{
    /// <inheritdoc/>
    public bool IsAdapterForIndividualComponents => true;

    /// <inheritdoc/>
    public IEnumerable<IComponentRegistration> RegistrationsFor(Service service, Func<Service, IEnumerable<ServiceRegistration>> registrationAccessor)
    {
        if (service == null)
        {
            throw new ArgumentNullException(nameof(service));
        }

        if (registrationAccessor == null)
        {
            throw new ArgumentNullException(nameof(registrationAccessor));
        }

        if (service is not KeyedService keyedService ||
            KeyedService.IsAnyKey(keyedService.ServiceKey) ||
            keyedService.ServiceType.IsCollectionServiceType())
        {
            return Enumerable.Empty<IComponentRegistration>();
        }

        // If there are already specific registrations for this key, do nothing.
        if (registrationAccessor(service).Any())
        {
            return Enumerable.Empty<IComponentRegistration>();
        }

        var anyKeyService = new KeyedService(KeyedService.AnyKey, keyedService.ServiceType);
        var anyKeyRegistrations = registrationAccessor(anyKeyService).ToArray();

        if (anyKeyRegistrations.Length == 0)
        {
            return Enumerable.Empty<IComponentRegistration>();
        }

        return anyKeyRegistrations.Select(r => CreateAdapterRegistration(r, keyedService));
    }

    private static ComponentRegistration CreateAdapterRegistration(ServiceRegistration anyKeyRegistration, KeyedService requestedService)
    {
        var metadata = new Dictionary<string, object?>(anyKeyRegistration.Registration.Metadata)
        {
            [MetadataKeys.AnyKeyAdapter] = true,
        };

        ComponentRegistration? adapterRegistration = null;

        var activator = new DelegateActivator(
            requestedService.ServiceType,
            (c, p) =>
            {
                var request = new ResolveRequest(
                    new KeyedService(KeyedService.AnyKey, requestedService.ServiceType),
                    anyKeyRegistration,
                    p,
                    adapterRegistration);

                return c.ResolveComponent(request);
            });

        adapterRegistration = new ComponentRegistration(
            Guid.NewGuid(),
            activator,
            anyKeyRegistration.Registration.Lifetime,
            anyKeyRegistration.Registration.Sharing,
            anyKeyRegistration.Registration.Ownership,
            new[] { requestedService },
            metadata,
            anyKeyRegistration.Registration);

        return adapterRegistration;
    }
}
