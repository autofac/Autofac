using System;
using System.Security;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using Autofac;

namespace Autofac.Extras.Multitenant.Wcf
{
    /// <summary>
    /// Retrieves service instances from an Autofac container.
    /// </summary>
    public class AutofacInstanceProvider : IInstanceProvider
    {
        /// <summary>
        /// Gets the container from which service instances should be resolved.
        /// </summary>
        /// <value>
        /// An <see cref="Autofac.IContainer"/> from which a lifetime scope will
        /// be spawned and service instances will be resolved.
        /// </value>
        public IContainer Container
        {
            get;

            private set;
        }

        /// <summary>
        /// Gets the service data for which instances will be resolved.
        /// </summary>
        /// <value>
        /// A <see cref="Autofac.Extras.Multitenant.Wcf.ServiceImplementationData"/>
        /// containing data about the service type that should be resolved.
        /// </value>
        public ServiceImplementationData ServiceData { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AutofacInstanceProvider"/> class.
        /// </summary>
        /// <param name="container">
        /// The container from which service instances should be resolved.
        /// </param>
        /// <param name="serviceData">
        /// Data object containing information about how to resolve the service
        /// implementation instance.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="container" /> or <paramref name="serviceData" />
        /// is <see langword="null" />.
        /// </exception>
        public AutofacInstanceProvider(IContainer container, ServiceImplementationData serviceData)
        {
            if (container == null)
            {
                throw new ArgumentNullException("container");
            }

            if (serviceData == null)
            {
                throw new ArgumentNullException("serviceData");
            }

            this.Container = container;
            this.ServiceData = serviceData;
        }

        /// <summary>
        /// Returns a service object given the specified <see cref="T:System.ServiceModel.InstanceContext"/> object.
        /// </summary>
        /// <param name="instanceContext">The current <see cref="T:System.ServiceModel.InstanceContext"/> object.</param>
        /// <returns>A user-defined service object.</returns>
        public object GetInstance(InstanceContext instanceContext)
        {
            return GetInstance(instanceContext, null);
        }

        /// <summary>
        /// Returns a service object given the specified <see cref="T:System.ServiceModel.InstanceContext"/> object.
        /// </summary>
        /// <param name="instanceContext">The current <see cref="T:System.ServiceModel.InstanceContext"/> object.</param>
        /// <param name="message">The message that triggered the creation of a service object.</param>
        /// <returns>The service object.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="instanceContext" /> is <see langword="null" />.
        /// </exception>
        public object GetInstance(InstanceContext instanceContext, Message message)
        {
            if (instanceContext == null)
            {
                throw new ArgumentNullException("instanceContext");
            }
            var extension = new AutofacInstanceContext(this.Container);
            instanceContext.Extensions.Add(extension);
            return extension.Resolve(this.ServiceData);
        }

        /// <summary>
        /// Called when an <see cref="T:System.ServiceModel.InstanceContext"/> object recycles a service object.
        /// </summary>
        /// <param name="instanceContext">The service's instance context.</param>
        /// <param name="instance">The service object to be recycled.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="instanceContext" /> is <see langword="null" />.
        /// </exception>
        public void ReleaseInstance(InstanceContext instanceContext, object instance)
        {
            if (instanceContext == null)
            {
                throw new ArgumentNullException("instanceContext");
            }
            var extension = instanceContext.Extensions.Find<AutofacInstanceContext>();
            if (extension != null)
            {
                extension.Dispose();
            }
        }
    }
}