using System;
using System.Collections.Generic;
using System.Linq;
using Autofac.Core;
using Autofac.Util;

namespace Autofac.Builder
{
    /// <summary>
    /// Registration source based on a registration builder.
    /// </summary>
    /// <typeparam name="TLimit">LimitType for the registration.</typeparam>
    /// <typeparam name="TActivatorData">Type of activator data.</typeparam>
    /// <typeparam name="TRegistrationStyle">Registration style.</typeparam>
    public class RegistrationSource<TLimit, TActivatorData, TRegistrationStyle> : IRegistrationSource
    {
        readonly RegistrationData _registrationData;
        readonly TActivatorData _activatorData;
        readonly IActivatorGenerator<TActivatorData> _activatorGenerator;

        /// <summary>
        /// Try to generate registrations using the provided activator generator, with
        /// the data from the provided registration.
        /// </summary>
        /// <param name="builder">Registration data.</param>
        /// <param name="activatorGenerator">Activator generator.</param>
        public RegistrationSource(IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> builder, IActivatorGenerator<TActivatorData> activatorGenerator)
        {
            Enforce.ArgumentNotNull(builder, "builder");
            Enforce.ArgumentNotNull(activatorGenerator, "activatorGenerator");

            _registrationData = builder.RegistrationData;
            _activatorData = builder.ActivatorData;
            _activatorGenerator = activatorGenerator;
        }

        /// <summary>
        /// Retrieve registrations for an unregistered service, to be used
        /// by the container.
        /// </summary>
        /// <param name="service">The service that was requested.</param>
        /// <param name="registrationAccessor">A function that will return existing registrations for a service.</param>
        /// <returns>Registrations providing the service.</returns>
        public IEnumerable<IComponentRegistration> RegistrationsFor(Service service, Func<Service, IEnumerable<IComponentRegistration>> registrationAccessor)
        {
            Enforce.ArgumentNotNull(service, "service");
            var result = Enumerable.Empty<IComponentRegistration>();

            IInstanceActivator activator;
            IEnumerable<Service> services;
            if (_activatorGenerator.TryGenerateActivator(service, _registrationData.Services, _activatorData, out activator, out services))
            {
                result = new[] {
                    RegistrationBuilder.CreateRegistration(
                        Guid.NewGuid(),
                        _registrationData,
                        activator,
                        services)
                };
            }

            return result;
        }
    }
}
