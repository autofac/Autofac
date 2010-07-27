using System;
using MultitenantExample.MvcApplication.WcfService;

namespace MultitenantExample.MvcApplication.Models
{
    /// <summary>
    /// Model used on the main index view to display information about the current
    /// dependency settings.
    /// </summary>
    public class IndexModel
    {
        /// <summary>
        /// Gets or sets the tenant ID to display.
        /// </summary>
        /// <value>
        /// A <see cref="System.Object"/> that represents the current tenant ID.
        /// </value>
        public object TenantId { get; set; }

        /// <summary>
        /// Gets or sets the controller type name.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> with the controller type name to display.
        /// </value>
        public string ControllerTypeName { get; set; }

        /// <summary>
        /// Gets or sets the dependency type name.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> with the dependency type name to display.
        /// </value>
        public string DependencyTypeName { get; set; }

        /// <summary>
        /// Gets or sets the dependency instance ID.
        /// </summary>
        /// <value>
        /// A <see cref="System.Guid"/> that indicates the unique ID for the dependency instance.
        /// </value>
        public Guid DependencyInstanceId { get; set; }

        /// <summary>
        /// Gets or sets the WCF service information.
        /// </summary>
        /// <value>
        /// A <see cref="MultitenantExample.MvcApplication.WcfService.GetServiceInfoResponse"/>
        /// containing information retrieved from the multitenant WCF service.
        /// </value>
        public GetServiceInfoResponse ServiceInfo { get; set; }
    }
}