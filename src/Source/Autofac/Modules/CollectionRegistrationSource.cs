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
using System.Text;
using Autofac.Registration;
using Autofac.Lifetime;
using Autofac.Activators;

namespace Autofac.Modules
{
    class CollectionRegistrationSource : IRegistrationSource
    {
        public bool TryGetRegistration(Service service, Func<Service, bool> registeredServicesTest, out IComponentRegistration registration)
        {
            var ts = service as TypedService;
            if (ts != null)
            {
                var serviceType = ts.ServiceType;
                Type elementType = null;

                if (serviceType.IsGenericType &&
                    serviceType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                {
                    elementType = serviceType.GetGenericArguments()[0];
                }
                else if (serviceType.IsArray)
                {
                    elementType = serviceType.GetElementType();
                }

                if (elementType != null)
                {
                    var elementTypeService = new TypedService(elementType);
                    var elementArrayType = elementType.MakeArrayType();

                    // Note, the maintenance of this item must be a lock-free algorithm.
                    IEnumerable<IComponentRegistration> elements = null;

                    registration = new ComponentRegistration(
                        Guid.NewGuid(),
                        new DelegateActivator(elementArrayType, (c, p) =>
                        {
                            if (elements == null)
                            {
                                c.ComponentRegistry.Registered += (s, e) =>
                                {
                                    if (e.ComponentRegistration.Services.Contains(elementTypeService))
                                        elements = c.ComponentRegistry.RegistrationsFor(elementTypeService);
                                };
                                elements = c.ComponentRegistry.RegistrationsFor(elementTypeService);
                            }

                            var items = elements.Select(cr => c.Resolve(cr, p)).ToArray();

                            var result = Array.CreateInstance(elementType, items.Length);
                            items.CopyTo(result, 0);
                            return result;
                        }),
                        new CurrentScopeLifetime(),
                        InstanceSharing.None,
                        InstanceOwnership.ExternallyOwned,
                        new[] { service },
                        new Dictionary<string, object>());

                    return true;
                }
            }

            registration = null;
            return false;
        }
    }
}
