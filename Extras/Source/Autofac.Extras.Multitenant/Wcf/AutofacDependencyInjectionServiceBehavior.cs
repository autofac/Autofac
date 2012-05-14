using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using Autofac;

namespace AutofacContrib.Multitenant.Wcf
{
	/// <summary>
	/// Sets the instance provider to an AutofacInstanceProvider.
	/// </summary>
	public class AutofacDependencyInjectionServiceBehavior : IServiceBehavior
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
		/// A <see cref="AutofacContrib.Multitenant.Wcf.ServiceImplementationData"/>
		/// containing data about the service type that should be resolved.
		/// </value>
		public ServiceImplementationData ServiceData { get; private set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="AutofacDependencyInjectionServiceBehavior"/> class.
		/// </summary>
		/// <param name="container">
		/// The container from which service implementations should be resolved.
		/// </param>
		/// <param name="serviceData">
		/// Data about which service type should be hosted and how to resolve
		/// the type to use for the service implementation.
		/// </param>
		public AutofacDependencyInjectionServiceBehavior(IContainer container, ServiceImplementationData serviceData)
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
		/// Provides the ability to inspect the service host and the service description to confirm that the service can run successfully.
		/// </summary>
		/// <param name="serviceDescription">The service description.</param>
		/// <param name="serviceHostBase">The service host that is currently being constructed.</param>
		public void Validate(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
		{
		}

		/// <summary>
		/// Provides the ability to pass custom data to binding elements to support the contract implementation.
		/// </summary>
		/// <param name="serviceDescription">The service description of the service.</param>
		/// <param name="serviceHostBase">The host of the service.</param>
		/// <param name="endpoints">The service endpoints.</param>
		/// <param name="bindingParameters">Custom objects to which binding elements have access.</param>
		public void AddBindingParameters(
			ServiceDescription serviceDescription, ServiceHostBase serviceHostBase,
			Collection<ServiceEndpoint> endpoints, BindingParameterCollection bindingParameters)
		{
		}

		/// <summary>
		/// Provides the ability to change run-time property values or insert custom extension objects such as error handlers, message or parameter interceptors, security extensions, and other custom extension objects.
		/// </summary>
		/// <param name="serviceDescription">The service description.</param>
		/// <param name="serviceHostBase">The host that is currently being built.</param>
		/// <exception cref="System.ArgumentNullException">
		/// Thrown if <paramref name="serviceDescription" /> or
		/// <paramref name="serviceHostBase" /> is <see langword="null" />.
		/// </exception>
		public void ApplyDispatchBehavior(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
		{
			if (serviceDescription == null)
			{
				throw new ArgumentNullException("serviceDescription");
			}
			if (serviceHostBase == null)
			{
				throw new ArgumentNullException("serviceHostBase");
			}

			var implementedContracts =
				from ep in serviceDescription.Endpoints
				where ep.Contract.ContractType.IsAssignableFrom(this.ServiceData.ServiceTypeToHost)
				select ep.Contract.Name;

			var instanceProvider = new AutofacInstanceProvider(this.Container, this.ServiceData);

			foreach (ChannelDispatcherBase cdb in serviceHostBase.ChannelDispatchers)
			{
				ChannelDispatcher cd = cdb as ChannelDispatcher;
				if (cd != null)
				{
					foreach (EndpointDispatcher ed in cd.Endpoints)
					{
						if (implementedContracts.Contains(ed.ContractName))
						{
							ed.DispatchRuntime.InstanceProvider = instanceProvider;
						}
					}
				}
			}
		}
	}
}