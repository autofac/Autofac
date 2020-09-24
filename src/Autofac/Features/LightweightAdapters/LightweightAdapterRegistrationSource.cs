// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Autofac.Builder;
using Autofac.Core;

namespace Autofac.Features.LightweightAdapters
{
    /// <summary>
    /// A registration source for registered adapters.
    /// </summary>
    internal class LightweightAdapterRegistrationSource : IRegistrationSource
    {
        private readonly RegistrationData _registrationData;
        private readonly LightweightAdapterActivatorData _activatorData;

        /// <summary>
        /// Initializes a new instance of the <see cref="LightweightAdapterRegistrationSource"/> class.
        /// </summary>
        /// <param name="registrationData">The registration data for the adapter.</param>
        /// <param name="activatorData">The activator data for the adapter.</param>
        public LightweightAdapterRegistrationSource(
            RegistrationData registrationData,
            LightweightAdapterActivatorData activatorData)
        {
            _registrationData = registrationData ?? throw new ArgumentNullException(nameof(registrationData));
            _activatorData = activatorData ?? throw new ArgumentNullException(nameof(activatorData));

            if (registrationData.Services.Contains(activatorData.FromService))
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, LightweightAdapterRegistrationSourceResources.FromAndToMustDiffer, activatorData.FromService));
            }
        }

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

            if (_registrationData.Services.Contains(service))
            {
                return registrationAccessor(_activatorData.FromService)
                    .Select(r =>
                    {
                        var rb = RegistrationBuilder
                            .ForDelegate((c, p) => _activatorData.Adapter(
                                c, Enumerable.Empty<Parameter>(), c.ResolveComponent(new ResolveRequest(_activatorData.FromService, r, p))))
                            .Targeting(r.Registration)
                            .InheritRegistrationOrderFrom(r.Registration);

                        rb.RegistrationData.CopyFrom(_registrationData, true);

                        return rb.CreateRegistration();
                    });
            }

            if (
                //// requested and adaptee are services with type
                //// not including decorators here
                //// and if this registration source contains requested service's type
                (service is IServiceWithType requestedServiceWithType && _activatorData.FromService is IServiceWithType adapteeServiceWithType) &&
                (requestedServiceWithType.ServiceType != adapteeServiceWithType.ServiceType) &&
                _registrationData.Services.OfType<IServiceWithType>().Any(s => s.ServiceType == requestedServiceWithType.ServiceType))
            {
                // we try to find registrations for the adaptee service but preserve info from the requested service e.g. keys
                var serviceToFind = requestedServiceWithType.ChangeType(adapteeServiceWithType.ServiceType);

                return registrationAccessor(serviceToFind)
                    .Select(r =>
                    {
                        var rb = RegistrationBuilder
                            .ForDelegate((c, p) => _activatorData.Adapter(
                                c, p, c.ResolveComponent(new ResolveRequest(serviceToFind, r, Enumerable.Empty<Parameter>()))))
                            .Targeting(r.Registration);

                        rb.RegistrationData.CopyFrom(_registrationData, true);

                        // we explicitly add requested service to the RegistrationData
                        rb.RegistrationData.AddService(service);

                        return rb.CreateRegistration();
                    });
            }

            return Enumerable.Empty<IComponentRegistration>();
        }

        /// <inheritdoc/>
        public bool IsAdapterForIndividualComponents => true;

        /// <inheritdoc/>
        public override string ToString()
        {
            return string.Format(
                CultureInfo.CurrentCulture,
                LightweightAdapterRegistrationSourceResources.AdapterFromToDescription,
                _activatorData.FromService.Description,
                string.Join(", ", _registrationData.Services.Select(s => s.Description).ToArray()));
        }
    }
}
