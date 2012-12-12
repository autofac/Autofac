using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.ServiceModel;
using Autofac.Core;
using Autofac.Core.Lifetime;

namespace Autofac.Integration.Wcf
{
    /// <summary>
    /// Simple resolver for WCF service implementations. Allows for single-tenant
    /// handling of named or typed services.
    /// </summary>
    public class DefaultServiceImplementationDataProvider : IServiceImplementationDataProvider
    {
        /// <summary>
        /// Gets data about a service implementation.
        /// </summary>
        /// <param name="value">
        /// The constructor string passed in to the service host factory
        /// that is used to determine which type to host/use as a service
        /// implementation.
        /// </param>
        /// <returns>
        /// A <see cref="Autofac.Integration.Wcf.ServiceImplementationData"/>
        /// object containing information about which type to use in
        /// the service host and which type to use to resolve the implementation.
        /// </returns>
        /// <remarks>
        /// <para>
        /// This resolver takes the constructor string stored in the .svc file
        /// and resolves a matching keyed or typed service from the root
        /// application container. That resolved type is used both for the
        /// service host as well as the implementation type.
        /// </para>
        /// </remarks>
        /// <exception cref="System.InvalidOperationException">
        /// Thrown if the <see cref="Autofac.Integration.Wcf.AutofacHostFactory.Container"/>
        /// is <see langword="null" />;
        /// if the service indicated by <paramref name="value" />
        /// is not registered with the <see cref="Autofac.Integration.Wcf.AutofacHostFactory.Container"/>;
        /// or if the service is a singleton that isn't registered as a singleton.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="value" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// Thrown if <paramref name="value" /> is empty.
        /// </exception>
        public virtual ServiceImplementationData GetServiceImplementationData(string value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            if (value.Length == 0)
            {
                throw new ArgumentException(String.Format(CultureInfo.CurrentCulture, Properties.Resources.ArgumentException_StringEmpty, "value"));
            }
            if (AutofacHostFactory.Container == null)
            {
                throw new InvalidOperationException(AutofacHostFactoryResources.ContainerIsNull);
            }
            IComponentRegistration registration = null;
            if (!AutofacHostFactory.Container.ComponentRegistry.TryGetRegistration(new KeyedService(value, typeof(object)), out registration))
            {
                Type serviceType = Type.GetType(value, false);
                if (serviceType != null)
                {
                    AutofacHostFactory.Container.ComponentRegistry.TryGetRegistration(new TypedService(serviceType), out registration);
                }
            }

            if (registration == null)
            {
                throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, AutofacHostFactoryResources.ServiceNotRegistered, value));
            }

            var data = new ServiceImplementationData
            {
                ConstructorString = value,
                ServiceTypeToHost = registration.Activator.LimitType,
                ImplementationResolver = l => l.ResolveComponent(registration, Enumerable.Empty<Parameter>())
            };

            var implementationType = registration.Activator.LimitType;
            if (IsSingletonWcfService(implementationType))
            {
                if (!IsRegistrationSingleInstance(registration))
                {
                    throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, AutofacHostFactoryResources.ServiceMustBeSingleInstance, implementationType.FullName));
                }

                data.HostAsSingleton = true;
            }
            else
            {
                if (IsRegistrationSingleInstance(registration))
                {
                    throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, AutofacHostFactoryResources.ServiceMustNotBeSingleInstance, implementationType.FullName));
                }
            }

            return data;
        }

        static bool IsRegistrationSingleInstance(IComponentRegistration registration)
        {
            return registration.Sharing == InstanceSharing.Shared && registration.Lifetime is RootScopeLifetime;
        }

        static bool IsSingletonWcfService(Type implementationType)
        {
            var behavior = implementationType
                .GetCustomAttributes(typeof(ServiceBehaviorAttribute), true)
                .OfType<ServiceBehaviorAttribute>()
                .FirstOrDefault();

            return behavior != null && behavior.InstanceContextMode == InstanceContextMode.Single;
        }
    }
}
