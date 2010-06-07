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
using System.Linq;
using Autofac.Core;

namespace Autofac.Integration.Wcf
{
    /// <summary>
    /// Default resolver for WCF service implementations.
    /// </summary>
    public class DefaultServiceImplementationDataProvider : IServiceImplementationDataProvider
    {
        /// <summary>
        /// Gets data about a service implementation.
        /// </summary>
        /// <param name="constructorString">The constructor string passed in to the service host factory
        /// that is used to determine which type to host/use as a service
        /// implementation.</param>
        /// <returns>
        /// A <see cref="Autofac.Integration.Wcf.ServiceImplementationData"/>
        /// object containing information about which type to use in
        /// the service host and which type to use to resolve the implementation.
        /// </returns>
        /// <remarks>
        /// <para>
        /// This resolver takes the constructor string stored in the .svc file
        /// and resolves a matching typed service from the root
        /// application container. That resolved type is used both for the
        /// service host as well as the implementation type.
        /// </para>
        /// </remarks>
        /// <exception cref="System.InvalidOperationException">
        /// Thrown if the <see cref="Autofac.Integration.Wcf.AutofacHostFactory.ContainerProvider"/>
        /// is <see langword="null" /> or has no application container; or
        /// if the service indicated by <paramref name="constructorString" />
        /// is not registered with the <see cref="Autofac.Integration.Wcf.AutofacHostFactory.ContainerProvider"/>.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="constructorString" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// Thrown if <paramref name="constructorString" /> is empty.
        /// </exception>
        public virtual ServiceImplementationData GetServiceImplementationData(string constructorString)
        {
            if (constructorString == null)
            {
                throw new ArgumentNullException("constructorString");
            }
            if (constructorString.Length == 0)
            {
                throw new ArgumentException(AutofacServiceHostFactoryResources.ConstructorStringEmpty, "constructorString");
            }
            if (AutofacHostFactory.ContainerProvider == null)
            {
                throw new InvalidOperationException(AutofacServiceHostFactoryResources.ContainerProviderIsNull);
            }
            if (AutofacHostFactory.ContainerProvider.ApplicationContainer == null)
            {
                throw new InvalidOperationException(AutofacServiceHostFactoryResources.ContainerIsNull);
            }
            IComponentRegistration registration = null;
            if (!AutofacHostFactory.ContainerProvider.ApplicationContainer.ComponentRegistry.TryGetRegistration(new NamedService(constructorString, typeof(object)), out registration))
            {
                Type serviceType = Type.GetType(constructorString, false);
                if (serviceType != null)
                    AutofacHostFactory.ContainerProvider.ApplicationContainer.ComponentRegistry.TryGetRegistration(new TypedService(serviceType), out registration);
            }

            if (registration == null)
                throw new InvalidOperationException(
                    string.Format(CultureInfo.CurrentCulture, AutofacServiceHostFactoryResources.ServiceNotRegistered, constructorString));

            return new ServiceImplementationData()
            {
                ConstructorString = constructorString,
                ServiceTypeToHost = registration.Activator.LimitType,
                ImplementationResolver = l => l.Resolve(registration, Enumerable.Empty<Parameter>())
            };
        }
    }
}
