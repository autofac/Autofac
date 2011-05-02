using System;
using Autofac;
using Autofac.Builder;

namespace AutofacContrib.Multitenant
{
    /// <summary>
    /// Extends <see cref="IRegistrationBuilder{TLimit, TActivatorData, TStyle}"/> with methods to support multitenancy.
    /// </summary>
    public static class RegistrationExtensions
    {
        /// <summary>
        /// Share one instance of the component within the context of an individual tenant.
        /// </summary>
        /// <typeparam name="TLimit">Registration limit type.</typeparam>
        /// <typeparam name="TActivatorData">Activator data type.</typeparam>
        /// <typeparam name="TRegistrationStyle">Registration style type.</typeparam>
        /// <param name="registration">Registration to set the lifetime scope on.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="registration" /> is <see langword="null" />.
        /// </exception>
        /// <remarks>
        /// <para>
        /// This method is useful when there is a desire to register an individual
        /// component at the root container level and have one instance of the
        /// component created per tenant.
        /// </para>
        /// </remarks>
        public static IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> InstancePerTenant<TLimit, TActivatorData, TRegistrationStyle>(
            this IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> registration)
        {
            if (registration == null)
            {
                throw new ArgumentNullException("registration");
            }
            return registration.InstancePerMatchingLifetimeScope(MultitenantContainer.TenantLifetimeScopeTag);
        }
    }
}
