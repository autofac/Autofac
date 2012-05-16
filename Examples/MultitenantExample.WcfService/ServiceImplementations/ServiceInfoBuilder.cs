using System;
using AutofacContrib.Multitenant;
using MultitenantExample.WcfService.Dependencies;

namespace MultitenantExample.WcfService.ServiceImplementations
{
    /// <summary>
    /// Common logic for building a service info response.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This <see langword="static" /> builder class is used instead of an
    /// inheritance hierarchy to illustrate that service implementations only
    /// have to implement the same service contract; they don't need to share
    /// an inheritance chain.
    /// </para>
    /// </remarks>
    public static class ServiceInfoBuilder
    {
        /// <summary>
        /// Builds a service info response.
        /// </summary>
        /// <param name="serviceImplementation">
        /// The service implementation that will be returning the response.
        /// </param>
        /// <param name="dependency">
        /// The dependency that was provided to the service implementation on construction.
        /// </param>
        /// <param name="tenantIdStrategy">
        /// The tenant ID strategy.
        /// </param>
        /// <returns>
        /// A populated service info response.
        /// </returns>
        public static GetServiceInfoResponse Build(IMultitenantService serviceImplementation, IDependency dependency, ITenantIdentificationStrategy tenantIdStrategy)
        {
            object tenantId = null;
            bool success = tenantIdStrategy.TryIdentifyTenant(out tenantId);
            if (!success || tenantId == null)
            {
                tenantId = "[Default Tenant]";
            }

            var response = new GetServiceInfoResponse()
            {
                ServiceImplementationTypeName = serviceImplementation.GetType().Name,
                DependencyInstanceId = dependency.InstanceId,
                DependencyTypeName = dependency.GetType().Name,
                TenantId = tenantId.ToString()
            };
            return response;
        }
    }
}