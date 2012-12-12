// This software is part of the Autofac IoC container
// Copyright © 2011 Autofac Contributors
// http://autofac.org
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.

using System;
using System.Globalization;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Activation;

namespace Autofac.Integration.Wcf
{
    /// <summary>
    /// Creates service host instances for WCF.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The Autofac service host factory allows you to change
    /// the strategy by which service implementations are resolved. You do this by
    /// setting the <see cref="Autofac.Integration.Wcf.AutofacHostFactory.ServiceImplementationDataProvider"/>
    /// with a strategy implementation.
    /// </para>
    /// <para>
    /// If <see cref="Autofac.Integration.Wcf.AutofacHostFactory.ServiceImplementationDataProvider"/>
    /// is <see langword="null" /> a new instance of <see cref="Autofac.Integration.Wcf.DefaultServiceImplementationDataProvider"/>
    /// will be used.
    /// </para>
    /// <para>
    /// You may configure additional behaviors or other aspects of generated
    /// service instances by setting the <see cref="Autofac.Integration.Wcf.AutofacHostFactory.HostConfigurationAction"/>.
    /// If this value is not <see langword="null" />, generated host instances
    /// will be run through that action.
    /// </para>
    /// </remarks>
    public abstract class AutofacHostFactory : ServiceHostFactory
    {
        /// <summary>
        /// Gets or sets the container or lifetime scope from which service instances will be retrieved.
        /// </summary>
        /// <value>
        /// An <see cref="Autofac.ILifetimeScope"/> that will be used to resolve service
        /// implementation instances.
        /// </value>
        public static ILifetimeScope Container { get; set; }

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
        /// An <see cref="Autofac.Integration.Wcf.IServiceImplementationDataProvider"/>
        /// that will be used to determine the proper service implementation given
        /// a service constructor string.
        /// </value>
        public static IServiceImplementationDataProvider ServiceImplementationDataProvider { get; set; }

        /// <summary>
        /// Creates a <see cref="System.ServiceModel.ServiceHost"/> with specific base addresses and initializes it with specified data.
        /// </summary>
        /// <param name="constructorString">The initialization data passed to the <see cref="System.ServiceModel.ServiceHostBase"/> instance being constructed by the factory.</param>
        /// <param name="baseAddresses">The <see cref="System.Array"/> of type <see cref="System.Uri"/> that contains the base addresses for the service hosted.</param>
        /// <returns>
        /// A <see cref="System.ServiceModel.ServiceHost"/> with specific base addresses.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="constructorString" /> or <paramref name="baseAddresses"/> is <see langword="null" />.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// Thrown if <paramref name="constructorString" /> is empty.
        /// </exception>
        /// <exception cref="System.InvalidOperationException">
        /// Thrown if the <see cref="Autofac.Integration.Wcf.AutofacHostFactory.Container"/>
        /// is <see langword="null" />.
        /// </exception>
        /// <remarks>
        /// <para>
        /// If <see cref="Autofac.Integration.Wcf.AutofacHostFactory.HostConfigurationAction"/>
        /// is not <see langword="null" />, the new service host instance is run
        /// through the configuration action prior to being returned. This allows
        /// you to programmatically configure behaviors or other aspects of the
        /// host.
        /// </para>
        /// </remarks>
        public override ServiceHostBase CreateServiceHost(string constructorString, Uri[] baseAddresses)
        {
            if (constructorString == null)
            {
                throw new ArgumentNullException("constructorString");
            }
            if (constructorString.Length == 0)
            {
                throw new ArgumentException(String.Format(CultureInfo.CurrentCulture, Properties.Resources.ArgumentException_StringEmpty, "constructorString"), "constructorString");
            }
            if (Container == null)
            {
                throw new InvalidOperationException(AutofacHostFactoryResources.ContainerIsNull);
            }

            var dataProvider = AutofacHostFactory.ServiceImplementationDataProvider;
            if (dataProvider == null)
            {
                dataProvider = new DefaultServiceImplementationDataProvider();
            }

            var data = dataProvider.GetServiceImplementationData(constructorString);

            if (data.ServiceTypeToHost == null)
            {
                throw new InvalidOperationException(String.Format(CultureInfo.CurrentUICulture, AutofacHostFactoryResources.NoServiceTypeToHost, dataProvider.GetType(), constructorString));
            }
            if (!data.ServiceTypeToHost.IsClass)
            {
                throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, AutofacHostFactoryResources.ImplementationTypeUnknown, constructorString, data.ServiceTypeToHost));
            }

            ServiceHost host;
            if (data.HostAsSingleton)
            {
                object singletonInstance = data.ImplementationResolver(Container);
                host = CreateSingletonServiceHost(singletonInstance, baseAddresses);
            }
            else
            {
                host = CreateServiceHost(data.ServiceTypeToHost, baseAddresses);
                host.Opening += (sender, args) => host.Description.Behaviors.Add(new AutofacDependencyInjectionServiceBehavior(Container, data));
            }

            ApplyHostConfigurationAction(host);

            return host;
        }

        /// <summary>
        /// Creates a <see cref="System.ServiceModel.ServiceHost"/> for a specified type of service with a specific base address.
        /// </summary>
        /// <param name="singletonInstance">Specifies the singleton service instance to host.</param>
        /// <param name="baseAddresses">The <see cref="System.Array"/> of type <see cref="System.Uri"/> that contains the base addresses for the service hosted.</param>
        /// <returns>
        /// A <see cref="System.ServiceModel.ServiceHost"/> for the singleton service instance specified with a specific base address.
        /// </returns>
        protected abstract ServiceHost CreateSingletonServiceHost(object singletonInstance, Uri[] baseAddresses);

        static void ApplyHostConfigurationAction(ServiceHostBase host)
        {
            var action = HostConfigurationAction;
            if (action != null)
                action(host);
        }
    }
}