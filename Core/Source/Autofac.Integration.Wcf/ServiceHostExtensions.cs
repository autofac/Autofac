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
using System.ServiceModel;
using Autofac.Core;

namespace Autofac.Integration.Wcf
{
    /// <summary>
    ///		Adds dependency injection related methods to service hosts.
    /// </summary>
    public static class ServiceHostExtensions
    {
        /// <summary>
        /// Adds the custom service behavior required for dependency injection.
        /// </summary>
        /// <typeparam name="T">The web service contract type.</typeparam>
        /// <param name="serviceHost">The service host.</param>
        /// <param name="container">The container.</param>
        public static void AddDependencyInjectionBehavior<T>(this ServiceHostBase serviceHost, ILifetimeScope container)
        {
            if (container == null) throw new ArgumentNullException("container");

            AddDependencyInjectionBehavior(serviceHost, typeof(T), container);
        }

        /// <summary>
        /// Adds the custom service behavior required for dependency injection.
        /// </summary>
        /// <param name="serviceHost">The service host.</param>
        /// <param name="contractType">The web service contract type.</param>
        /// <param name="container">The container.</param>
        public static void AddDependencyInjectionBehavior(this ServiceHostBase serviceHost, Type contractType, ILifetimeScope container)
        {
            if (serviceHost == null)
            {
                throw new ArgumentNullException("serviceHost");
            }
            if (contractType == null)
            {
                throw new ArgumentNullException("contractType");
            }
            if (container == null)
            {
                throw new ArgumentNullException("container");
            }

            var serviceBehavior = serviceHost.Description.Behaviors.Find<ServiceBehaviorAttribute>();
            if (serviceBehavior != null && serviceBehavior.InstanceContextMode == InstanceContextMode.Single)
                return;

            IComponentRegistration registration;
            if (!container.ComponentRegistry.TryGetRegistration(new TypedService(contractType), out registration))
            {
                var message = string.Format(CultureInfo.CurrentCulture, ServiceHostExtensionsResources.ContractTypeNotRegistered, contractType.FullName);
                throw new ArgumentException(message, "contractType");
            }

            var behavior = new AutofacDependencyInjectionServiceBehavior(container, serviceHost.Description.ServiceType, registration);
            serviceHost.Description.Behaviors.Add(behavior);
        }
    }
}
