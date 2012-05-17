using System;
using System.ServiceModel;
using Autofac;
using Autofac.Util;

namespace Autofac.Extras.Multitenant.Wcf
{
    /// <summary>
    /// Manages instance lifetimes using an Autofac inner container.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This instance context extension creates a child lifetime scope based
    /// on a scope provided - generally a tenant-specific lifetime scope - and
    /// resolves service instances from that child scope.
    /// </para>
    /// <para>
    /// When this instance context is disposed, the lifetime scope it creates
    /// (which contains the resolved service instance) is also disposed.
    /// </para>
    /// </remarks>
    public class AutofacInstanceContext : Disposable, IExtension<InstanceContext>, IDisposable
    {
        /// <summary>
        /// Gets the lifetime scope for the current instance context.
        /// </summary>
        /// <value>
        /// An <see cref="Autofac.ILifetimeScope"/> from which service instances
        /// will be resolved.
        /// </value>
        public ILifetimeScope LifetimeScope { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AutofacInstanceContext"/> class.
        /// </summary>
        /// <param name="parentScope">
        /// The outer container/lifetime scope from which the instance scope
        /// will be created.
        /// </param>
        public AutofacInstanceContext(ILifetimeScope parentScope)
        {
            if (parentScope == null)
            {
                throw new ArgumentNullException("parentScope");
            }

            // With the multitenant container, beginning a lifetime scope is
            // tenant-specific inherently.
            this.LifetimeScope = parentScope.BeginLifetimeScope();
        }

        /// <summary>
        /// Retrieve a service instance from the context.
        /// </summary>
        /// <param name="serviceData">
        /// Data object containing information about how to resolve the service
        /// implementation instance.
        /// </param>
        /// <returns>The service instance.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="serviceData" /> is <see langword="null" />.
        /// </exception>
        public object Resolve(ServiceImplementationData serviceData)
        {
            if (serviceData == null)
            {
                throw new ArgumentNullException("serviceData");
            }
            return serviceData.ImplementationResolver(this.LifetimeScope);
        }

        /// <summary>
        /// Enables an extension object to find out when it has been aggregated.
        /// Called when the extension is added to the
        /// <see cref="P:System.ServiceModel.IExtensibleObject`1.Extensions"/> property.
        /// </summary>
        /// <param name="owner">The extensible object that aggregates this extension.</param>
        public void Attach(InstanceContext owner)
        {
        }

        /// <summary>
        /// Enables an object to find out when it is no longer aggregated.
        /// Called when an extension is removed from the
        /// <see cref="P:System.ServiceModel.IExtensibleObject`1.Extensions"/> property.
        /// </summary>
        /// <param name="owner">The extensible object that aggregates this extension.</param>
        public void Detach(InstanceContext owner)
        {
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing">
        /// <see langword="true" /> to release both managed and unmanaged
        /// resources; <see langword="false" /> to release only unmanaged resources.
        /// </param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.LifetimeScope.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
