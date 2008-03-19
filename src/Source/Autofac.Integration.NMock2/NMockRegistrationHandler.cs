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

using Autofac;
using Autofac.Component;
using Autofac.Component.Activation;
using Autofac.Component.Scope;
using NMock2;
using System;

namespace Autofac.Integration.NMock2
{
	/// <summary> Resolves unknown interfaces and Mocks using the <see cref="Mockery"/> from the scope. </summary>
	class NMockRegistrationHandler : IRegistrationSource
	{
        /// <summary>
        /// Retrieve a registration for an unregistered service, to be used
        /// by the container.
        /// </summary>
        /// <param name="service">The service that was requested.</param>
        /// <param name="registration">A registration providing the service.</param>
        /// <returns>
        /// True if the registration could be created.
        /// </returns>
		public bool TryGetRegistration(Service service, out IComponentRegistration registration)
		{
            if (service == null)
                throw new ArgumentNullException("service");

			registration = null;

			var typedService = service as TypedService;
			if ((typedService == null) || (!typedService.ServiceType.IsInterface))
				return false;

			var descriptor = new Descriptor(
				new UniqueService(),
				new[] { service },
				typedService.ServiceType);
			
			registration = new Registration(
				descriptor,
				new DelegateActivator((c, p) => c.Resolve<Mockery>().NewMock(typedService.ServiceType)),
				new ContainerScope(),
				InstanceOwnership.Container);

			return true;
		}
	}
}