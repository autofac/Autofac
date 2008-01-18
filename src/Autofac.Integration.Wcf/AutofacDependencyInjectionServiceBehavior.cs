// Contributed 2008-01-15 by Petar Andrijasevic
// Copyright (c) 2008 Autofac Contributors
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

using System.Collections.ObjectModel;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System;

namespace Autofac.Integration.Wcf
{
    /// <summary>
    /// Sets the instance provider to an AutofacInstanceProvider.
    /// </summary>
	public class AutofacDependencyInjectionServiceBehavior : IServiceBehavior
	{
		private readonly Container _container;

        /// <summary>
        /// Initializes a new instance of the <see cref="AutofacDependencyInjectionServiceBehavior"/> class.
        /// </summary>
        /// <param name="container">The container.</param>
		public AutofacDependencyInjectionServiceBehavior(Container container)
		{
            if (container == null)
                throw new ArgumentNullException("container");

			this._container = container;
		}

		#region IServiceBehavior Members

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
		public void ApplyDispatchBehavior(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
		{
            if (serviceDescription == null)
                throw new ArgumentNullException("serviceDescription");

            if (serviceHostBase == null)
                throw new ArgumentNullException("serviceHostBase");

            var contractTypes = serviceDescription.Endpoints.ToDictionary(
                ep => ep.Contract.Name,
                ep => ep.Contract.ContractType);

            foreach (ChannelDispatcherBase cdb in serviceHostBase.ChannelDispatchers)
            {
                ChannelDispatcher cd = cdb as ChannelDispatcher;
                if (cd != null)
                {
                    foreach (EndpointDispatcher ed in cd.Endpoints)
                    {
                        if (contractTypes.ContainsKey(ed.ContractName))
                            ed.DispatchRuntime.InstanceProvider = 
                                new AutofacInstanceProvider(_container, contractTypes[ed.ContractName]);
                    }
                }
            }
		}

		#endregion
	}
}