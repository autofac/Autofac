using System;
using System.ServiceModel;

namespace Autofac.Extras.Multitenant.Wcf
{
    /// <summary>
    /// Extension for <see cref="System.ServiceModel.OperationContext"/>
    /// that allows propagation of the tenant ID.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Use this extension in conjunction with the
    /// <see cref="Autofac.Extras.Multitenant.Wcf.OperationContextTenantIdentificationStrategy"/>
    /// to determine which tenant a given operation is running under.
    /// </para>
    /// <para>
    /// For example, you could use an <see cref="System.ServiceModel.Dispatcher.IDispatchMessageInspector"/>
    /// that gets the tenant ID from an incoming header and adds a
    /// <see cref="Autofac.Extras.Multitenant.Wcf.TenantIdentificationContextExtension"/>
    /// to the current <see cref="System.ServiceModel.OperationContext"/> with
    /// the tenant ID value. Then you could register the
    /// <see cref="Autofac.Extras.Multitenant.Wcf.OperationContextTenantIdentificationStrategy"/>
    /// as the mechanism for determining the tenant ID when resolving multitenant dependencies.
    /// </para>
    /// <para>
    /// The <see cref="Autofac.Extras.Multitenant.Wcf.TenantPropagationBehavior{TTenantId}"/>
    /// is a behavior that does exactly that - adds the tenant ID to outbound messages on the client
    /// and parses them on the service side. For a usage example, see
    /// <see cref="Autofac.Extras.Multitenant.Wcf.TenantPropagationBehavior{TTenantId}"/>
    /// </para>
    /// </remarks>
    /// <seealso cref="Autofac.Extras.Multitenant.Wcf.OperationContextTenantIdentificationStrategy"/>
    /// <seealso cref="Autofac.Extras.Multitenant.Wcf.TenantPropagationBehavior{TTenantId}"/>
    public class TenantIdentificationContextExtension : IExtension<OperationContext>
    {
        /// <summary>
        /// Gets or sets the tenant ID.
        /// </summary>
        /// <value>
        /// An <see cref="System.Object"/> that uniquely identifies the tenant
        /// under which the current operation is executing.
        /// </value>
        public object TenantId { get; set; }

        /// <summary>
        /// Enables an extension object to find out when it has been aggregated.
        /// </summary>
        /// <param name="owner">
        /// The extensible object that aggregates this extension.
        /// </param>
        public virtual void Attach(OperationContext owner)
        {
        }

        /// <summary>
        /// Enables an object to find out when it is no longer aggregated.
        /// </summary>
        /// <param name="owner">
        /// The extensible object that aggregates this extension.
        /// </param>
        public virtual void Detach(OperationContext owner)
        {
        }
    }
}
