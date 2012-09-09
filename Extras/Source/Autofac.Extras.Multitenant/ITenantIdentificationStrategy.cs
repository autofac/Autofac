using System;
using System.Diagnostics.CodeAnalysis;

namespace Autofac.Extras.Multitenant
{
    /// <summary>
    /// Defines a provider that determines the current tenant ID from
    /// execution context.
    /// </summary>
    public interface ITenantIdentificationStrategy
    {
        /// <summary>
        /// Attempts to identify the tenant from the current execution context.
        /// </summary>
        /// <param name="tenantId">
        /// The current tenant identifier.
        /// </param>
        /// <returns>
        /// <see langword="true" /> if the tenant could be identified; <see langword="false" />
        /// if not.
        /// </returns>
        [SuppressMessage("Microsoft.Design", "CA1007:UseGenericsWhereAppropriate", Justification = "Tenant identifiers are objects.")]
        bool TryIdentifyTenant(out object tenantId);
    }
}
