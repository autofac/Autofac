// This software is part of the Autofac IoC container
// Copyright © 2015 Autofac Contributors
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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Autofac.Extensions.DependencyInjection
{
    internal class AutofacServiceProvider : IServiceProvider, ISupportRequiredService
    {
        private readonly IComponentContext _componentContext;
        private List<OrderedServiceDescriptor> _originalDescriptors;
        private HashSet<Type> _serviceTypes;
        private MethodInfo _castMethodInfo;

        public AutofacServiceProvider(IComponentContext componentContext, List<ServiceDescriptor> originalDescriptors)
        {
            _componentContext = componentContext;
            _originalDescriptors = originalDescriptors.Select((d, i) => new OrderedServiceDescriptor
            {
                Index = i,
                ImplementationInstance = d.ImplementationInstance,
                ImplementationType = d.ImplementationType
            }).ToList();
            _serviceTypes = new HashSet<Type>(originalDescriptors.Select(d => d.ServiceType));
            _castMethodInfo = typeof(Enumerable).GetTypeInfo().GetDeclaredMethod("Cast");
        }

        public object GetService(Type serviceType)
        {
            return GetOrdered(serviceType, _componentContext.ResolveOptional(serviceType));
        }

        public object GetRequiredService(Type serviceType)
        {
            return GetOrdered(serviceType, _componentContext.Resolve(serviceType));
        }

        private object GetOrdered(Type serviceType, object resolutionResult)
        {
            if (serviceType.Name == "IEnumerable`1" && _serviceTypes.Contains(serviceType.GenericTypeArguments[0]))
            {
                var actualServiceType = serviceType.GenericTypeArguments[0];
                var resulutionResultEnumerable = ((IEnumerable)resolutionResult).Cast<object>();

                resulutionResultEnumerable = resulutionResultEnumerable
                    .Select(rr => new { ResolutionResult = rr, Index = _originalDescriptors.First(original => MatchedServiceDescriptor(rr, original)).Index })
                    .OrderBy(rr => rr.Index)
                    .Select(rr => rr.ResolutionResult).ToList();

                var res = _castMethodInfo.MakeGenericMethod(actualServiceType).Invoke(null, new object[] { resulutionResultEnumerable });
                return res;
            }
            else
            {
                return resolutionResult;
            }
        }

        private static bool MatchedServiceDescriptor(object a, OrderedServiceDescriptor f)
        {
            return f.ImplementationInstance == a || (f.ImplementationInstance == null && a.GetType() == f.ImplementationType);
        }
    }

    internal class OrderedServiceDescriptor
    {
        public int Index { get; set; }
        public object ImplementationInstance { get; set; }
        public Type ImplementationType { get; set; }

    }
}