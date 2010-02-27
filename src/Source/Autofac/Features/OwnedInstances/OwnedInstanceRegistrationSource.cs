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
using System.Collections.Generic;
using System.Linq;
using Autofac.Builder;
using Autofac.Core;
using Autofac.Util;

namespace Autofac.Features.OwnedInstances
{
    /// <summary>
    /// Generates registrations for services of type <see cref="Owned{T}"/> whenever the service
    /// T is available.
    /// </summary>
    class OwnedInstanceRegistrationSource : IRegistrationSource
    {
        /// <summary>
        /// Retrieve registrations for an unregistered service, to be used
        /// by the container.
        /// </summary>
        /// <param name="service">The service that was requested.</param>
        /// <param name="registrationAccessor">A function that will return existing registrations for a service.</param>
        /// <returns>Registrations providing the service.</returns>
        public IEnumerable<IComponentRegistration> RegistrationsFor(Service service, Func<Service, IEnumerable<IComponentRegistration>> registrationAccessor)
        {
            Enforce.ArgumentNotNull(service, "service");
            Enforce.ArgumentNotNull(registrationAccessor, "registrationAccessor");

            var ts = service as IServiceWithType;
            if (ts == null || !ts.ServiceType.IsClosingTypeOf(typeof(Owned<>)))
                return Enumerable.Empty<IComponentRegistration>();

            var ownedInstanceType = ts.ServiceType.GetGenericArguments()[0];
            var ownedInstanceService = ts.ChangeType(ownedInstanceType);

            return registrationAccessor(ownedInstanceService)
                .Select(r =>
                {
                    var rb = RegistrationBuilder.ForDelegate(ts.ServiceType, (c, p) =>
                        {
                            var lifetime = c.Resolve<ILifetimeScope>().BeginLifetimeScope(ownedInstanceService);
                            try
                            {
                                var value = lifetime.Resolve(r, p);
                                return Activator.CreateInstance(ts.ServiceType, new [] { value, lifetime });
                            }
                            catch
                            {
                                lifetime.Dispose();
                                throw;
                            }
                        })
                        .ExternallyOwned()
                        .As(service)
                        .Targeting(r);

                    return RegistrationBuilder.CreateRegistration(rb);
                });
        }
    }
}
