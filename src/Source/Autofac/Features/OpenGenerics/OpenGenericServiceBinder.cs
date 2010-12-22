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
using Autofac.Core;
using Autofac.Util;

namespace Autofac.Features.OpenGenerics
{
    static class OpenGenericServiceBinder
    {
        public static bool TryBindServiceType(
            Service service,
            IEnumerable<Service> configuredOpenGenericServices,
            Type openGenericImplementationType,
            out Type constructedImplementationType,
            out IEnumerable<Service> constructedServices)
        {
            var swt = service as IServiceWithType;
            if (swt != null && swt.ServiceType.IsGenericType)
            {
                var definitionService = (IServiceWithType)swt.ChangeType(swt.ServiceType.GetGenericTypeDefinition());
                var serviceGenericArguments = swt.ServiceType.GetGenericArguments();

                if (configuredOpenGenericServices.Cast<IServiceWithType>().Any(s => s.Equals(definitionService)))
                {
                    var implementorGenericArguments = TryMapImplementationGenericArguments(
                        openGenericImplementationType, swt.ServiceType, definitionService.ServiceType, serviceGenericArguments);

                    if (!implementorGenericArguments.Any(a => a == null) &&
                        openGenericImplementationType.IsCompatibleWithGenericParameterConstraints(implementorGenericArguments))
                    {
                        var constructedImplementationTypeTmp = openGenericImplementationType.MakeGenericType(implementorGenericArguments);

                        // This needs looking at
                        var implementedServices = (from IServiceWithType s in configuredOpenGenericServices
                                                   let genericService = s.ServiceType.MakeGenericType(serviceGenericArguments)
                                                   where genericService.IsAssignableFrom(constructedImplementationTypeTmp)
                                                   select s.ChangeType(genericService)).ToArray();

                        if (implementedServices.Length > 0)
                        {
                            constructedImplementationType = constructedImplementationTypeTmp;
                            constructedServices = implementedServices;
                            return true;
                        }
                    }
                }
            }

            constructedImplementationType = null;
            constructedServices = null;
            return false;
        }

        static Type[] TryMapImplementationGenericArguments(Type implementationType, Type serviceType, Type serviceTypeDefinition, Type[] serviceGenericArguments)
        {
            if (serviceTypeDefinition == implementationType)
                return serviceGenericArguments;

            var implementationGenericArgumentDefinitions = implementationType.GetGenericArguments();
            var serviceArgumentDefinitions = serviceType.IsInterface ?
                    GetInterface(implementationType, serviceType).GetGenericArguments() :
                    serviceTypeDefinition.GetGenericArguments();

            var serviceArgumentDefinitionToArgumentMapping = serviceArgumentDefinitions.Zip(serviceGenericArguments, Tuple.Create);

            return implementationGenericArgumentDefinitions
                .Select(implementationGenericArgumentDefinition => TryFindServiceArgumentForImplementationArgumentDefinition(
                    implementationGenericArgumentDefinition, serviceArgumentDefinitionToArgumentMapping))
                .ToArray();
        }

        static Type GetInterface(Type implementationType, Type serviceType)
        {
            return
#if SILVERLIGHT     
                implementationType.GetInterfaces().Single(i => i.Name == serviceType.Name);
#else
                implementationType.GetInterface(serviceType.Name);
#endif
        }

        static Type TryFindServiceArgumentForImplementationArgumentDefinition(Type implementationGenericArgumentDefinition, IEnumerable<Tuple<Type, Type>> serviceArgumentDefinitionToArgument)
        {
            var matchingRegularType = serviceArgumentDefinitionToArgument
                .Where(argdef => !argdef.Item1.IsGenericType && implementationGenericArgumentDefinition.Name == argdef.Item1.Name)
                .Select(argdef => argdef.Item2)
                .FirstOrDefault();

            if (matchingRegularType != null)
                return matchingRegularType;

            return serviceArgumentDefinitionToArgument
                .Where(argdef => argdef.Item1.IsGenericType && argdef.Item2.GetGenericArguments().Length > 0)
                .Select(argdef => TryFindServiceArgumentForImplementationArgumentDefinition(
                    implementationGenericArgumentDefinition, argdef.Item1.GetGenericArguments().Zip(argdef.Item2.GetGenericArguments(), Tuple.Create)))
                .FirstOrDefault();
        }

        public static void EnforceBindable(Type implementationType, IEnumerable<Service> services)
        {
            if (implementationType == null) throw new ArgumentNullException("implementationType");
            if (services == null) throw new ArgumentNullException("services");

            if (!implementationType.IsGenericTypeDefinition)
                throw new ArgumentException(
                    string.Format(OpenGenericServiceBinderResources.ImplementorMustBeOpenGenericTypeDefinition, implementationType));

            foreach (IServiceWithType service in services)
            {
                if (!service.ServiceType.IsGenericTypeDefinition)
                    throw new ArgumentException(
                        string.Format(OpenGenericServiceBinderResources.ServiceTypeMustBeOpenGenericTypeDefinition, service));

                if (service.ServiceType.IsInterface)
                {
                    if (GetInterface(implementationType, service.ServiceType) == null)
                        throw new ArgumentException(string.Format(OpenGenericServiceBinderResources.InterfaceIsNotImplemented, implementationType, service));
                }
                else
                {
                    if (!Traverse.Across(implementationType, t => t.BaseType).Any(t => IsCompatibleGenericClassDefinition(t, service.ServiceType)))
                        throw new ArgumentException(string.Format(OpenGenericServiceBinderResources.TypesAreNotConvertible, implementationType, service));
                }
            }
        }

        static bool IsCompatibleGenericClassDefinition(Type implementor, Type serviceType)
        {
            return implementor == serviceType || implementor.IsGenericType && implementor.GetGenericTypeDefinition() == serviceType;
        }
    }
}
