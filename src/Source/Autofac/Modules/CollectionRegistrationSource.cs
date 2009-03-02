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
using Autofac.Component;
using Autofac.Component.Activation;
using Autofac.Component.Scope;

namespace Autofac.Modules
{
    class CollectionRegistrationSource : IRegistrationSource
    {
        public bool TryGetRegistration(Service service, out IComponentRegistration registration)
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

                    registration = new Registration(
                        new Descriptor(
                            new UniqueService(),
                            new[] { service },
                            elementArrayType),
                        new DelegateActivator((c, p) =>
                        {
                            var ctr = c.Resolve<IContainer>();
                            var items = AllRegistrationsInHierarchy(ctr)
                                .Where(cr => cr.Descriptor.Services.Contains(elementTypeService))
                                .Select(cr => ctr.Resolve(cr.Descriptor.Id, p))
                                .ToArray();
                            var result = Array.CreateInstance(elementType, items.Length);
                            items.CopyTo(result, 0);
                            return result;
                        }),
                        new ContainerScope(),
                        InstanceOwnership.External);

                    return true;
                }
            }

            registration = null;
            return false;
        }

        IEnumerable<IComponentRegistration> AllRegistrationsInHierarchy(IContainer container)
        {
            var registrations = container.ComponentRegistrations;
            while (container.OuterContainer != null)
            {
                container = container.OuterContainer;
                registrations = registrations.Union(container.ComponentRegistrations);
            }
            return registrations;
        }
    }
}
