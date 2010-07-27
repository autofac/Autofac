using System;
using AutofacContrib.Multitenant;

namespace MultitenantExample.ConsoleApplication
{
    /// <summary>
    /// Simple tenant ID strategy that allows you to manually specify what the
    /// current tenant ID is.
    /// </summary>
    /// <remarks>
    /// <para>
    /// While this simple implementation requires you to manually specify the
    /// current tenant ID, in a more "real world" application, you could
    /// get the tenant ID from an environment variable, a role on the current
    /// thread principal, a value from an incoming message, or anywhere else
    /// that would work for your application.
    /// </para>
    /// </remarks>
    public class ManualTenantIdentificationStrategy : ITenantIdentificationStrategy
    {
        /// <summary>
        /// Gets or sets the current tenant ID. "0" is the "default tenant."
        /// </summary>
        /// <value>
        /// An <see cref="System.Object"/> that will be returned by
        /// <see cref="MultitenantExample.ConsoleApplication.ManualTenantIdentificationStrategy.TryIdentifyTenant"/>
        /// when the current tenant ID is requested.
        /// </value>
        public object CurrentTenantId { get; set; }

        /// <summary>
        /// Returns the tenant ID that was manually specified in
        /// <see cref="MultitenantExample.ConsoleApplication.ManualTenantIdentificationStrategy.CurrentTenantId"/>.
        /// </summary>
        /// <param name="tenantId">The current tenant identifier.</param>
        /// <returns>
        /// This implementation always returns <see langword="true"/>.
        /// </returns>
        public bool TryIdentifyTenant(out object tenantId)
        {
            if (this.CurrentTenantId.ToString() == "0")
            {
                // 0 is the "default tenant ID"
                tenantId = null;
            }
            else
            {
                // If the current tenant isn't default, return the actual ID.
                tenantId = this.CurrentTenantId;
            }
            return true;
        }
    }
}
