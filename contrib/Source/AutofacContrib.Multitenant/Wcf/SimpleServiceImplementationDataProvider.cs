using System;
using System.Globalization;
using System.Linq;
using Autofac.Core;

namespace AutofacContrib.Multitenant.Wcf
{
    /// <summary>
    /// Simple resolver for WCF service implementations. Allows for single-tenant
    /// handling of named or typed services. Does not support multitenancy.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The behavior of this resolver is the same as the original WCF integration
    /// Autofac host factory where named or typed services are allowed and multitenancy
    /// is not supported.
    /// </para>
    /// </remarks>
    public class SimpleServiceImplementationDataProvider : IServiceImplementationDataProvider
    {
        /// <summary>
        /// Gets data about a service implementation.
        /// </summary>
        /// <param name="value">The constructor string passed in to the service host factory
        /// that is used to determine which type to host/use as a service
        /// implementation.</param>
        /// <returns>
        /// A <see cref="AutofacContrib.Multitenant.Wcf.ServiceImplementationData"/>
        /// object containing information about which type to use in
        /// the service host and which type to use to resolve the implementation.
        /// </returns>
        /// <remarks>
        /// <para>
        /// This resolver takes the constructor string stored in the .svc file
        /// and resolves a matching typed service from the root
        /// application container. That resolved type is used both for the
        /// service host as well as the implementation type.
        /// </para>
        /// </remarks>
        /// <exception cref="System.InvalidOperationException">
        /// Thrown if the <see cref="AutofacContrib.Multitenant.Wcf.AutofacHostFactory.Container"/>
        /// is <see langword="null" />; or
        /// if the service indicated by <paramref name="value" />
        /// is not registered with the <see cref="AutofacContrib.Multitenant.Wcf.AutofacHostFactory.Container"/>.
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
                throw new ArgumentException(String.Format(CultureInfo.CurrentUICulture, Properties.Resources.ArgumentException_StringEmpty, "value"));
            }
            if (AutofacHostFactory.Container == null)
            {
                throw new InvalidOperationException(Properties.Resources.AutofacHostFactory_ContainerIsNull);
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
                throw new InvalidOperationException(
                    string.Format(CultureInfo.CurrentCulture, Properties.Resources.AutofacHostFactory_ServiceNotRegistered, value));
            }

            return new ServiceImplementationData()
            {
                ConstructorString = value,
                ServiceTypeToHost = registration.Activator.LimitType,
                ImplementationResolver = l => l.Resolve(registration, Enumerable.Empty<Parameter>())
            };
        }
    }
}
