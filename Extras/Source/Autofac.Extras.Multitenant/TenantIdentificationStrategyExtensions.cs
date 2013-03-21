using System;
using System.Collections.Generic;
using System.Linq;

namespace Autofac.Extras.Multitenant
{
    /// <summary>
    /// Extension methods for working with <see cref="Autofac.Extras.Multitenant.ITenantIdentificationStrategy"/>.
    /// </summary>
    public static class TenantIdentificationStrategyExtensions
    {
        /// <summary>
        /// Gets a typed tenant ID from a strategy or the default value for the type
        /// if identification fails.
        /// </summary>
        /// <typeparam name="T">The type of the tenant ID.</typeparam>
        /// <param name="strategy">
        /// The <see cref="Autofac.Extras.Multitenant.ITenantIdentificationStrategy"/> from which the tenant ID should be retrieved.
        /// </param>
        /// <returns>
        /// If tenant identification succeeds, the ID from <paramref name="strategy" /> is converted to
        /// <typeparamref name="T"/> and returned. If identification fails, the default value for
        /// <typeparamref name="T"/> is returned.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="strategy" /> is <see langword="null" />.
        /// </exception>
        public static T IdentifyTenant<T>(this ITenantIdentificationStrategy strategy)
        {
            if (strategy == null)
            {
                throw new ArgumentNullException("strategy");
            }
            object id;
            if (strategy.TryIdentifyTenant(out id))
            {
                return (T)id;
            }
            return default(T);
        }
    }
}
