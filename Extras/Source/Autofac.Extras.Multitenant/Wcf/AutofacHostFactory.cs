using System;
using System.Globalization;
using System.Security;
using System.ServiceModel;
using System.ServiceModel.Activation;
using Autofac;

namespace AutofacContrib.Multitenant.Wcf
{
    /// <summary>
    /// Base class for factories that create service host instances for WCF.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This implementation of the Autofac service host factory allows you to change
    /// the strategy by which service implementations are resolved. You do this by
    /// setting the <see cref="AutofacContrib.Multitenant.Wcf.AutofacHostFactory.ServiceImplementationDataProvider"/>
    /// with a strategy implementation.
    /// </para>
    /// <para>
    /// If <see cref="AutofacContrib.Multitenant.Wcf.AutofacHostFactory.ServiceImplementationDataProvider"/>
    /// is <see langword="null" /> a new instance of <see cref="AutofacContrib.Multitenant.Wcf.MultitenantServiceImplementationDataProvider"/>
    /// will be used. This is different behavior than the standard Autofac
    /// service host factory. If you want to use the standard behavior (which is
    /// not multitenant), set <see cref="AutofacContrib.Multitenant.Wcf.AutofacHostFactory.ServiceImplementationDataProvider"/>
    /// to be an instance of <see cref="AutofacContrib.Multitenant.Wcf.SimpleServiceImplementationDataProvider"/>.
    /// </para>
    /// <para>
    /// For multitenancy to work properly, you must set the
    /// <see cref="AutofacContrib.Multitenant.Wcf.AutofacHostFactory.Container"/>
    /// to be a <see cref="AutofacContrib.Multitenant.MultitenantContainer"/>.
    /// </para>
    /// <para>
    /// You may configure additional behaviors or other aspects of generated
    /// service instances by setting the <see cref="AutofacContrib.Multitenant.Wcf.AutofacHostFactory.HostConfigurationAction"/>.
    /// If this value is not <see langword="null" />, generated host instances
    /// will be run through that action.
    /// </para>
    /// </remarks>
    public abstract class AutofacHostFactory : ServiceHostFactory
    {
        /// <summary>
        /// Gets or sets the container from which service instances will be retrieved.
        /// For multitenancy to work properly, you must set this
        /// to be a <see cref="AutofacContrib.Multitenant.MultitenantContainer"/>.
        /// </summary>
        /// <value>
        /// An <see cref="Autofac.IContainer"/> that will be used to resolve service
        /// implementation instances.
        /// </value>
        public static IContainer Container
        {
            get;

            set;
        }

        /// <summary>
        /// Gets or sets an action that can be used to programmatically configure
        /// service host instances this factory generates.
        /// </summary>
        /// <value>
        /// An <see cref="Action{T}"/> that can be used to configure service host
        /// instances that this factory creates. This action can be used to add
        /// behaviors or otherwise modify the host before it gets returned by
        /// the factory.
        /// </value>
        public static Action<ServiceHostBase> HostConfigurationAction { get; set; }

        /// <summary>
        /// Gets or sets the service implementation data strategy.
        /// </summary>
        /// <value>
        /// An <see cref="AutofacContrib.Multitenant.Wcf.IServiceImplementationDataProvider"/>
        /// that will be used to determine the proper service implementation given
        /// a service constructor string.
        /// </value>
        public static IServiceImplementationDataProvider ServiceImplementationDataProvider { get; set; }

        /// <summary>
        /// Creates a <see cref="T:System.ServiceModel.ServiceHost"/> with specific base addresses and initializes it with specified data.
        /// </summary>
        /// <param name="constructorString">The initialization data passed to the <see cref="T:System.ServiceModel.ServiceHostBase"/> instance being constructed by the factory.</param>
        /// <param name="baseAddresses">The <see cref="T:System.Array"/> of type <see cref="T:System.Uri"/> that contains the base addresses for the service hosted.</param>
        /// <returns>
        /// A <see cref="T:System.ServiceModel.ServiceHost"/> with specific base addresses.
        /// </returns>
        /// <remarks>
        /// <para>
        /// If <see cref="AutofacContrib.Multitenant.Wcf.AutofacHostFactory.ServiceImplementationDataProvider"/>
        /// is <see langword="null" /> a new instance of <see cref="AutofacContrib.Multitenant.Wcf.MultitenantServiceImplementationDataProvider"/>
        /// will be used. This is different behavior than the standard Autofac
        /// service host factory. If you want to use the standard behavior (which is
        /// not multitenant), set <see cref="AutofacContrib.Multitenant.Wcf.AutofacHostFactory.ServiceImplementationDataProvider"/>
        /// to be an instance of <see cref="AutofacContrib.Multitenant.Wcf.SimpleServiceImplementationDataProvider"/>.
        /// </para>
        /// </remarks>
        /// <exception cref="T:System.ArgumentNullException">
        /// Thrown if <paramref name="constructorString" /> or <paramref name="baseAddresses"/> is <see langword="null" />.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// Thrown if <paramref name="constructorString" /> is empty.
        /// </exception>
        /// <exception cref="T:System.InvalidOperationException">
        /// Thrown if the <see cref="AutofacContrib.Multitenant.Wcf.AutofacHostFactory.Container"/>
        /// is <see langword="null" />.
        /// </exception>
        [SecuritySafeCritical]
        public override ServiceHostBase CreateServiceHost(string constructorString, Uri[] baseAddresses)
        {
            if (constructorString == null)
            {
                throw new ArgumentNullException("constructorString");
            }
            if (constructorString.Length == 0)
            {
                throw new ArgumentException(String.Format(CultureInfo.CurrentUICulture, Properties.Resources.ArgumentException_StringEmpty, "constructorString"));
            }
            if (AutofacHostFactory.Container == null)
            {
                throw new InvalidOperationException(Properties.Resources.AutofacHostFactory_ContainerIsNull);
            }

            var dataProvider = AutofacHostFactory.ServiceImplementationDataProvider;
            if (dataProvider == null)
            {
                dataProvider = new MultitenantServiceImplementationDataProvider();
            }

            var data = dataProvider.GetServiceImplementationData(constructorString);

            if (data.ServiceTypeToHost == null)
            {
                throw new InvalidOperationException(String.Format(CultureInfo.CurrentUICulture, Properties.Resources.AutofacHostFactory_NoServiceHostType, dataProvider.GetType(), constructorString));
            }
            if (!data.ServiceTypeToHost.IsClass)
            {
                throw new InvalidOperationException(String.Format(CultureInfo.CurrentUICulture, Properties.Resources.AutofacHostFactory_ServiceHostTypeNotClass, dataProvider.GetType(), constructorString, data.ServiceTypeToHost));
            }

            return CreateServiceHost(data, baseAddresses);
        }

        /// <summary>
        /// Creates the service host and attaches a WCF Service behavior that uses Autofac to resolve the service instance.
        /// </summary>
        /// <param name="serviceData">Data about which service type to host and how to resolve the implementation type.</param>
        /// <param name="baseAddresses">The base addresses.</param>
        /// <returns>A <see cref="T:System.ServiceModel.ServiceHost"/> with specific base addresses.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="serviceData" /> is <see langword="null" />.
        /// </exception>
        /// <remarks>
        /// <para>
        /// If <see cref="AutofacContrib.Multitenant.Wcf.AutofacHostFactory.HostConfigurationAction"/>
        /// is not <see langword="null" />, the new service host instance is run
        /// through the configuration action prior to being returned. This allows
        /// you to programmatically configure behaviors or other aspects of the
        /// host.
        /// </para>
        /// </remarks>
        protected virtual ServiceHost CreateServiceHost(ServiceImplementationData serviceData, Uri[] baseAddresses)
        {
            if (serviceData == null)
            {
                throw new ArgumentNullException("serviceData");
            }
            var host = CreateServiceHost(serviceData.ServiceTypeToHost, baseAddresses);
            host.Opening += (sender, args) => host.Description.Behaviors.Add(
                new AutofacDependencyInjectionServiceBehavior(AutofacHostFactory.Container, serviceData));

            var action = AutofacHostFactory.HostConfigurationAction;
            if (action != null)
            {
                action(host);
            }

            return host;
        }
    }
}