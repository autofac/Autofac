// This software is part of the Autofac IoC container
// Copyright (c) 2010 Autofac Contributors
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
using System.ServiceModel;
using System.ServiceModel.Activation;

namespace Autofac.Integration.Wcf
{
    /// <summary>
    /// Creates ServiceHost instances for WCF.
    /// </summary>
    public abstract class AutofacHostFactory : ServiceHostFactory
    {
        /// <summary>
        /// The container provider from which service instances will be retrieved.
        /// </summary>
        public static IContainerProvider ContainerProvider { get; set; }

        /// <summary>
        /// Creates a <see cref="T:System.ServiceModel.ServiceHost"/> with specific base addresses and initializes it with specified data.
        /// </summary>
        /// <param name="constructorString">The initialization data passed to the <see cref="T:System.ServiceModel.ServiceHostBase"/> instance being constructed by the factory.</param>
        /// <param name="baseAddresses">The <see cref="T:System.Array"/> of type <see cref="T:System.Uri"/> that contains the base addresses for the service hosted.</param>
        /// <returns>
        /// A <see cref="T:System.ServiceModel.ServiceHost"/> with specific base addresses.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="constructorString" /> or <paramref name="baseAddresses"/> is <see langword="null" />.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// Thrown if <paramref name="constructorString" /> is empty.
        /// </exception>
        /// <exception cref="T:System.InvalidOperationException">
        /// <see cref="Autofac.Integration.Wcf.AutofacHostFactory.ContainerProvider"/>
        /// is <see langword="null" />; the application container is <see langword="null" />;
        /// or the <see cref="ServiceImplementationData.ServiceTypeToHost"/>
        /// resolved by the registered <see cref="IServiceImplementationDataProvider"/>
        /// is <see langword="null" /> or not a class.
        /// </exception>
        public override ServiceHostBase CreateServiceHost(string constructorString, Uri[] baseAddresses)
        {
            if (constructorString == null)
            {
                throw new ArgumentNullException("constructorString");
            }
            if (baseAddresses == null)
            {
                throw new ArgumentNullException("baseAddresses");
            }
            if (constructorString.Length == 0)
            {
                throw new ArgumentException(AutofacServiceHostFactoryResources.ConstructorStringEmpty, "constructorString");
            }
            if (ContainerProvider == null)
            {
                throw new InvalidOperationException(AutofacServiceHostFactoryResources.ContainerProviderIsNull);
            }
            if (ContainerProvider.ApplicationContainer == null)
            {
                throw new InvalidOperationException(AutofacServiceHostFactoryResources.ContainerIsNull);
            }

            IServiceImplementationDataProvider dataProvider = null;
            if (!ContainerProvider.ApplicationContainer.TryResolve<IServiceImplementationDataProvider>(out dataProvider))
            {
                dataProvider = new DefaultServiceImplementationDataProvider();
            }

            var data = dataProvider.GetServiceImplementationData(constructorString);

            if (data.ServiceTypeToHost == null)
            {
                throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, AutofacServiceHostFactoryResources.NoServiceHostType, dataProvider.GetType(), constructorString));
            }
            if (!data.ServiceTypeToHost.IsClass)
            {
                throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, AutofacServiceHostFactoryResources.ServiceHostTypeNotClass, dataProvider.GetType(), constructorString, data.ServiceTypeToHost));
            }

            return CreateServiceHost(data, baseAddresses);
        }

        /// <summary>
        /// Creates the service host and attaches a WCF Service behaviour that uses Autofac to resolve the service instance.
        /// </summary>
        /// <param name="implementationData">Data about which service type to host and how to resolve the implementation type.</param>
        /// <param name="baseAddresses">The base addresses.</param>
        /// <returns>A <see cref="T:System.ServiceModel.ServiceHost"/> with specific base addresses.</returns>
        /// <remarks>
        /// <para>
        /// You can add behaviors to the service host by using the
        /// <see cref="AutofacServiceHostBehaviorRegistrationExtensions.RegisterServiceBehaviorForHost"/>
        /// method. For example, if a behavior for processing incoming messages
        /// to determine the tenant is required, that can be added to the
        /// list of behaviors that will be applied during the
        /// <see cref="System.ServiceModel.ICommunicationObject.Opening"/> event.
        /// </para>
        /// </remarks>
        protected virtual ServiceHost CreateServiceHost(ServiceImplementationData implementationData, Uri[] baseAddresses)
        {
            var host = CreateServiceHost(implementationData.ServiceTypeToHost, baseAddresses);
            var behaviors = ContainerProvider.ApplicationContainer.ResolveServiceBehaviorsForHost();
            host.Opening += (sender, args) =>
                {
                    host.Description.Behaviors.Add(new AutofacDependencyInjectionServiceBehavior(implementationData));
                    foreach (var b in behaviors)
                    {
                        host.Description.Behaviors.Add(b);
                    }
                };

            return host;
        }
    }
}