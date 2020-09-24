// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Autofac.Builder;
using Autofac.Core;
using Autofac.Util;

namespace Autofac.Features.GeneratedFactories
{
    /// <summary>
    /// Registration source for generated factory methods (i.e. when resolving <see cref="Func{T}"/> or some variant).
    /// </summary>
    internal class GeneratedFactoryRegistrationSource : IRegistrationSource
    {
        /// <summary>
        /// Retrieve registrations for an unregistered service, to be used
        /// by the container.
        /// </summary>
        /// <param name="service">The service that was requested.</param>
        /// <param name="registrationAccessor">A function that will return existing registrations for a service.</param>
        /// <returns>Registrations providing the service.</returns>
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

            if (!(service is IServiceWithType ts) || !ts.ServiceType.IsDelegate())
            {
                return Enumerable.Empty<IComponentRegistration>();
            }

            var resultType = ts.ServiceType.FunctionReturnType();
            var resultTypeService = ts.ChangeType(resultType);

            return registrationAccessor(resultTypeService)
                .Select(r =>
                {
                    var factory = new FactoryGenerator(ts.ServiceType, resultTypeService, r, ParameterMapping.Adaptive);
                    var rb = RegistrationBuilder.ForDelegate(ts.ServiceType, factory.GenerateFactory)
                        .InstancePerLifetimeScope()
                        .ExternallyOwned()
                        .As(service)
                        .Targeting(r.Registration)
                        .InheritRegistrationOrderFrom(r.Registration);

                    return rb.CreateRegistration();
                });
        }

        /// <inheritdoc/>
        public bool IsAdapterForIndividualComponents => true;

        /// <inheritdoc/>
        public override string ToString()
        {
            return GeneratedFactoryRegistrationSourceResources.GeneratedFactoryRegistrationSourceDescription;
        }
    }
}
