using System;
using System.Collections.Generic;
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
        RegistrationData _registrationData;
        TActivatorData _activatorData;
        IActivatorGenerator<TActivatorData> _activatorGenerator;

        /// <summary>
        /// Try to generate registrations using the provided activator generator, with
        /// the data from the provided registration.
        /// </summary>
        /// <param name="builder">Registration data.</param>
        /// <param name="activatorGenerator">Activator generator.</param>
        public RegistrationSource(RegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> builder, IActivatorGenerator<TActivatorData> activatorGenerator)
        {
            Enforce.ArgumentNotNull(builder, "builder");
            Enforce.ArgumentNotNull(activatorGenerator, "activatorGenerator");

            _registrationData = builder.RegistrationData;
            _activatorData = builder.ActivatorData;
            _activatorGenerator = activatorGenerator;
        }

        /// <summary>
        /// Retrieve a registration for an unregistered service, to be used
        /// by the container.
        /// </summary>
        /// <param name="service">The service that was requested.</param>
        /// <param name="registeredServicesTest">A predicate that can be queried to determine if a service is already registered.</param>
        /// <param name="registration">A registration providing the service.</param>
        /// <returns>True if the registration could be created.</returns>
        public bool TryGetRegistration(Service service, Func<Service, bool> registeredServicesTest, out IComponentRegistration registration)
        {
            Enforce.ArgumentNotNull(service, "service");
            registration = null;

            IInstanceActivator activator;
            IEnumerable<Service> services;
            if (!_activatorGenerator.TryGenerateActivator(service, _registrationData.Services, _activatorData, out activator, out services))
                return false;

            registration = RegistrationBuilder.CreateRegistration(
                Guid.NewGuid(),
                _registrationData,
                activator,
                services);

            return true;
        }
    }
}
