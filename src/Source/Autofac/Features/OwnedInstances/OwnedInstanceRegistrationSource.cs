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
using System.Collections.Generic;
using System.Linq;
using Autofac.Util;
using Autofac.Core;
using Autofac.Core.Registration;
using Autofac.Core.Activators.Delegate;
using Autofac.Core.Lifetime;

namespace Autofac.Features.OwnedInstances
{
    /// <summary>
    /// Generates registrations for services of type <see cref="Owned{T}"/> whenever the service
    /// T is available.
    /// </summary>
    class OwnedInstanceRegistrationSource : IRegistrationSource
    {
        /// <summary>
        /// Retrieve a registration for an unregistered service, to be used
        /// by the container.
        /// </summary>
        /// <param name="service">The service that was requested.</param>
        /// <param name="registeredServicesTest">A predicate that can be queried to determine if a service is already registered.</param>
        /// <param name="registration">A registration providing the service.</param>
        /// <returns>True if the registration could be created.</returns>
        public bool TryGetRegistration(Service service, Func<Service, bool> registeredServicesTest, out IComponentRegistration registration)
        {
            Enforce.ArgumentNotNull(service, "service");
            Enforce.ArgumentNotNull(registeredServicesTest, "registeredServicesTest");

            var ts = service as TypedService;
            if (ts != null &&
                ts.ServiceType.IsGenericType &&
                ts.ServiceType.GetGenericTypeDefinition() == typeof(Owned<>))
            {
                var ownedInstanceType = ts.ServiceType.GetGenericArguments()[0];

                if (registeredServicesTest(new TypedService(ownedInstanceType)))
                {
                    registration = new ComponentRegistration(
                        Guid.NewGuid(),
                        new DelegateActivator(ts.ServiceType, (c, p) =>
                        {
                            var lifetime = c.Resolve<ILifetimeScope>().BeginLifetimeScope();
                            try
                            {
                                var value = lifetime.Resolve(ownedInstanceType, p);
                                return Activator.CreateInstance(ts.ServiceType, new object[] { value, lifetime });
                            }
                            catch
                            {
                                lifetime.Dispose();
                                throw;
                            }
                        }),
                        new CurrentScopeLifetime(),
                        InstanceSharing.None,
                        InstanceOwnership.ExternallyOwned,
                        new Service[] { new TypedService(ts.ServiceType) },
                        new Dictionary<string, object>());

                    return true;
                }
            }

            registration = null;
            return false;
        }
    }
}
