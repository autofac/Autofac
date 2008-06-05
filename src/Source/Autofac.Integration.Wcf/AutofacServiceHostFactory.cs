// This software is part of the Autofac IoC container
// Copyright (c) 2007 - 2008 Autofac Contributors
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
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.Globalization;

namespace Autofac.Integration.Wcf
{
    /// <summary>
    /// Creates ServiceHost instances for WCF.
    /// </summary>
	public class AutofacServiceHostFactory : ServiceHostFactory
	{
        /// <summary>
        /// The container from which service instances will be retrieved.
        /// </summary>
        public static IContainer Container { get; set; }

        /// <summary>
        /// Creates a <see cref="T:System.ServiceModel.ServiceHost"/> with specific base addresses and initializes it with specified data.
        /// </summary>
        /// <param name="constructorString">The initialization data passed to the <see cref="T:System.ServiceModel.ServiceHostBase"/> instance being constructed by the factory.</param>
        /// <param name="baseAddresses">The <see cref="T:System.Array"/> of type <see cref="T:System.Uri"/> that contains the base addresses for the service hosted.</param>
        /// <returns>
        /// A <see cref="T:System.ServiceModel.ServiceHost"/> with specific base addresses.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// 	<paramref name="baseAddress"/> is null.</exception>
        /// <exception cref="T:System.InvalidOperationException">There is no hosting context provided or <paramref name="constructorString"/> is null or empty.</exception>
        public override ServiceHostBase CreateServiceHost(string constructorString, Uri[] baseAddresses)
        {
            if (constructorString == null)
                throw new ArgumentNullException("constructorString");

            if (constructorString == String.Empty)
                throw new ArgumentOutOfRangeException("constructorString");

            IComponentRegistration registration = null;
            if (!Container.TryGetDefaultRegistrationFor(new NamedService(constructorString), out registration))
            {
                Type serviceType = Type.GetType(constructorString, false);
                if (serviceType != null)
                    Container.TryGetDefaultRegistrationFor(new TypedService(serviceType), out registration);
            }

            if (registration == null)
                throw new InvalidOperationException(
                    string.Format(CultureInfo.CurrentCulture, AutofacServiceHostFactoryResources.ServiceNotRegistered, constructorString));

            if (!registration.Descriptor.BestKnownImplementationType.IsClass)
                throw new InvalidOperationException(
                    string.Format(CultureInfo.CurrentCulture, AutofacServiceHostFactoryResources.ImplementationTypeUnknown, constructorString, registration));

            return CreateServiceHost(registration.Descriptor.BestKnownImplementationType, baseAddresses);
        }

        /// <summary>
        /// Creates a <see cref="T:System.ServiceModel.ServiceHost"/> for a specified type of service with a specific base address.
        /// </summary>
        /// <param name="serviceType">Specifies the type of service to host.</param>
        /// <param name="baseAddresses">The <see cref="T:System.Array"/> of type <see cref="T:System.Uri"/> that contains the base addresses for the service hosted.</param>
        /// <returns>
        /// A <see cref="T:System.ServiceModel.ServiceHost"/> for the type of service specified with a specific base address.
        /// </returns>
		protected override ServiceHost CreateServiceHost(Type serviceType, Uri[] baseAddresses)
		{
            if (serviceType == null)
                throw new ArgumentNullException("serviceType");

            if (baseAddresses == null)
                throw new ArgumentNullException("baseAddresses");

            if (Container == null)
                throw new InvalidOperationException(AutofacServiceHostFactoryResources.ContainerIsNull);

			return new AutofacServiceHost(Container, serviceType, baseAddresses);
		}
	}
}