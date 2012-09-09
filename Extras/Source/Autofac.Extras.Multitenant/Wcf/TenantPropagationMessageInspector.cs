using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using Autofac;
using System.Diagnostics.CodeAnalysis;

namespace Autofac.Extras.Multitenant.Wcf
{
    /// <summary>
    /// Message inspector that helps in passing the tenant ID from a WCF client
    /// to the respective service.
    /// </summary>
    /// <typeparam name="TTenantId">
    /// The type of the tenant ID to propagate. Must be nullable and
    /// serializable so it can be added to a message header.
    /// </typeparam>
    /// <remarks>
    /// <para>
    /// Use this in conjunction with the <see cref="Autofac.Extras.Multitenant.Wcf.TenantPropagationBehavior{TTenantId}"/>
    /// to automatically get the tenant ID on the WCF client end, add the ID
    /// to a header on the outbound message, and have the tenant ID read from
    /// headers on the service side and added to the operation context in an
    /// <see cref="Autofac.Extras.Multitenant.Wcf.TenantIdentificationContextExtension"/>.
    /// This allows you, on the service side, to use the
    /// <see cref="Autofac.Extras.Multitenant.Wcf.OperationContextTenantIdentificationStrategy"/>
    /// as your registered <see cref="Autofac.Extras.Multitenant.ITenantIdentificationStrategy"/>.
    /// </para>
    /// <para>
    /// For a usage example, see <see cref="Autofac.Extras.Multitenant.Wcf.TenantPropagationBehavior{TTenantId}"/>.
    /// </para>
    /// </remarks>
    /// <seealso cref="Autofac.Extras.Multitenant.Wcf.TenantPropagationBehavior{TTenantId}"/>
    public class TenantPropagationMessageInspector<TTenantId> : IClientMessageInspector, IDispatchMessageInspector
    {
        /// <summary>
        /// Namespace of the header that gets added to messages and carries tenant information.
        /// </summary>
        protected const string TenantHeaderNamespace = "urn:Autofac.Extras.Multitenant";

        /// <summary>
        /// Name of the header that gets added to messages and carries the tenant ID.
        /// </summary>
        protected const string TenantHeaderName = "tenantId";

        /// <summary>
        /// Gets the strategy used for identifying the current tenant.
        /// </summary>
        /// <value>
        /// An <see cref="Autofac.Extras.Multitenant.ITenantIdentificationStrategy"/>
        /// used to identify the current tenant from the execution context.
        /// </value>
        public ITenantIdentificationStrategy TenantIdentificationStrategy { get; private set; }

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="Autofac.Extras.Multitenant.Wcf.TenantPropagationMessageInspector{TTenantId}"/> class.
        /// </summary>
        /// <param name="tenantIdentificationStrategy">
        /// The strategy to use for identifying the current tenant.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="tenantIdentificationStrategy" /> is <see langword="null" />.
        /// </exception>
        public TenantPropagationMessageInspector(ITenantIdentificationStrategy tenantIdentificationStrategy)
        {
            if (tenantIdentificationStrategy == null)
            {
                throw new ArgumentNullException("tenantIdentificationStrategy");
            }
            this.TenantIdentificationStrategy = tenantIdentificationStrategy;
        }

        /// <summary>
        /// Enables inspection or modification of a message after a reply message
        /// is received but prior to passing it back to the client application.
        /// </summary>
        /// <param name="reply">
        /// The message to be transformed into types and handed back to the client
        /// application.
        /// </param>
        /// <param name="correlationState">
        /// Correlation state data.
        /// </param>
        public void AfterReceiveReply(ref Message reply, object correlationState)
        {
        }

        /// <summary>
        /// Inspects inbound message headers and adds an
        /// <see cref="Autofac.Extras.Multitenant.Wcf.TenantIdentificationContextExtension"/>
        /// to the current operation context with the tenant ID.
        /// </summary>
        /// <param name="request">The request message.</param>
        /// <param name="channel">The incoming channel.</param>
        /// <param name="instanceContext">The current service instance.</param>
        /// <returns>
        /// Always returns <see langword="null" />. There is no correlation state
        /// value to be managed in this inspector.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="request" /> is <see langword="null" />.
        /// </exception>
        public object AfterReceiveRequest(ref Message request, IClientChannel channel, InstanceContext instanceContext)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }
            var context = OperationContext.Current;
            try
            {
                var tenantId = request.Headers.GetHeader<TTenantId>(TenantHeaderName, TenantHeaderNamespace);
                context.Extensions.Add(new TenantIdentificationContextExtension() { TenantId = tenantId });
            }
            catch (MessageHeaderException)
            {
                // The additional header won't be there when we update service
                // references; only when the behavior is on the client end.
            }
            return null;
        }

        /// <summary>
        /// Adds the tenant ID to the outbound message headers.
        /// </summary>
        /// <param name="request">The message to be sent to the service.</param>
        /// <param name="channel">The client object channel.</param>
        /// <returns>
        /// Always returns <see langword="null" />. There is no correlation state
        /// value to be managed in this inspector.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="request" /> is <see langword="null" />.
        /// </exception>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public object BeforeSendRequest(ref Message request, IClientChannel channel)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }
            // ApplicationContainer is used rather than RequestLifetime because
            // you don't want individual tenants overriding the mechanism
            // that determines tenant.
            object contextTenantId;
            TTenantId tenantId;
            if (!this.TenantIdentificationStrategy.TryIdentifyTenant(out contextTenantId))
            {
                tenantId = default(TTenantId);
            }
            else
            {
                tenantId = (TTenantId)contextTenantId;
            }
            MessageHeader tenantHeader = new MessageHeader<TTenantId>(tenantId).GetUntypedHeader(TenantHeaderName, TenantHeaderNamespace);
            request.Headers.Add(tenantHeader);
            return null;
        }

        /// <summary>
        /// Called after the operation has returned but before the reply message
        /// is sent.
        /// </summary>
        /// <param name="reply">
        /// The reply message. This value is <see langword="null" /> if the
        /// operation is one way.
        /// </param>
        /// <param name="correlationState">
        /// The correlation object returned from the
        /// <see cref="M:System.ServiceModel.Dispatcher.IDispatchMessageInspector.AfterReceiveRequest(System.ServiceModel.Channels.Message@,System.ServiceModel.IClientChannel,System.ServiceModel.InstanceContext)"/>
        /// method.
        /// </param>
        public void BeforeSendReply(ref Message reply, object correlationState)
        {
        }
    }
}
