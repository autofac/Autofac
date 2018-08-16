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
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

using Autofac.Core;
using Autofac.Util;

namespace Autofac.Features.OpenGenerics
{
    internal static class OpenGenericServiceBinder
    {
        public static bool TryBindServiceType(
            Service service,
            IEnumerable<Service> configuredOpenGenericServices,
            Type openGenericImplementationType,
            out Type constructedImplementationType,
            out Service[] constructedServices)
        {
            var swt = service as IServiceWithType;
            if (swt != null && swt.ServiceType.GetTypeInfo().IsGenericType)
            {
                var definitionService = (IServiceWithType)swt.ChangeType(swt.ServiceType.GetGenericTypeDefinition());
                var serviceGenericArguments = swt.ServiceType.GetTypeInfo().GenericTypeArguments;

                if (configuredOpenGenericServices.Cast<IServiceWithType>().Any(s => s.Equals(definitionService)))
                {
                    var implementorGenericArguments = TryMapImplementationGenericArguments(
                        openGenericImplementationType, swt.ServiceType, definitionService.ServiceType, serviceGenericArguments);

                    if (implementorGenericArguments.All(a => a != null) &&
                        openGenericImplementationType.IsCompatibleWithGenericParameterConstraints(implementorGenericArguments))
                    {
                        var constructedImplementationTypeTmp = openGenericImplementationType.MakeGenericType(implementorGenericArguments);
                        var constructedImplementationTypeTmpInfo = constructedImplementationTypeTmp.GetTypeInfo();

                        var implementedServices = configuredOpenGenericServices
                            .Cast<IServiceWithType>()
                            .Where(s => s.ServiceType.GetTypeInfo().GenericTypeParameters.Length == serviceGenericArguments.Length)
                            .Select(s => new { ServiceWithType = s, GenericService = s.ServiceType.MakeGenericType(serviceGenericArguments) })
                            .Where(p => p.GenericService.GetTypeInfo().IsAssignableFrom(constructedImplementationTypeTmpInfo))
                            .Select(p => p.ServiceWithType.ChangeType(p.GenericService))
                            .ToArray();

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

        private static Type[] TryMapImplementationGenericArguments(Type implementationType, Type serviceType, Type serviceTypeDefinition, Type[] serviceGenericArguments)
        {
            if (serviceTypeDefinition == implementationType)
                return serviceGenericArguments;

            var implementationGenericArgumentDefinitions = implementationType.GetTypeInfo().GenericTypeParameters;
            var serviceArgumentDefinitions = serviceType.GetTypeInfo().IsInterface ?
                    GetInterface(implementationType, serviceType).GetTypeInfo().GenericTypeArguments :
                    serviceTypeDefinition.GetTypeInfo().GenericTypeParameters;

            var serviceArgumentDefinitionToArgumentMapping = serviceArgumentDefinitions.Zip(serviceGenericArguments, (a, b) => new KeyValuePair<Type, Type>(a, b));

            return implementationGenericArgumentDefinitions
                .Select(implementationGenericArgumentDefinition => TryFindServiceArgumentForImplementationArgumentDefinition(
                    implementationGenericArgumentDefinition, serviceArgumentDefinitionToArgumentMapping))
                .ToArray();
        }

        private static Type GetInterface(Type implementationType, Type serviceType)
        {
            try
            {
                return implementationType.GetTypeInfo().ImplementedInterfaces
                    .First(i => i.Name == serviceType.Name && i.Namespace == serviceType.Namespace);
            }
            catch (InvalidOperationException)
            {
                var message = string.Format(CultureInfo.CurrentCulture, OpenGenericServiceBinderResources.ImplementorDoesntImplementService, implementationType.FullName, serviceType.FullName);
                throw new InvalidOperationException(message);
            }
        }

        private static Type TryFindServiceArgumentForImplementationArgumentDefinition(Type implementationGenericArgumentDefinition, IEnumerable<KeyValuePair<Type, Type>> serviceArgumentDefinitionToArgument)
        {
            var matchingRegularType = serviceArgumentDefinitionToArgument
                .Where(argdef => !argdef.Key.GetTypeInfo().IsGenericType && implementationGenericArgumentDefinition.Name == argdef.Key.Name)
                .Select(argdef => argdef.Value)
                .FirstOrDefault();

            if (matchingRegularType != null)
                return matchingRegularType;

            return serviceArgumentDefinitionToArgument
                .Where(argdef => argdef.Key.GetTypeInfo().IsGenericType && argdef.Value.GetTypeInfo().GenericTypeArguments.Any())
                .Select(argdef => TryFindServiceArgumentForImplementationArgumentDefinition(
                    implementationGenericArgumentDefinition, argdef.Key.GetTypeInfo().GenericTypeArguments.Zip(
                        argdef.Value.GetTypeInfo().GenericTypeArguments, (a, b) => new KeyValuePair<Type, Type>(a, b))))
                .FirstOrDefault(x => x != null);
        }

        public static void EnforceBindable(Type implementationType, IEnumerable<Service> services)
        {
            if (implementationType == null) throw new ArgumentNullException(nameof(implementationType));
            if (services == null) throw new ArgumentNullException(nameof(services));

            if (!implementationType.GetTypeInfo().IsGenericTypeDefinition)
            {
                throw new ArgumentException(
                    string.Format(CultureInfo.CurrentCulture, OpenGenericServiceBinderResources.ImplementorMustBeOpenGenericTypeDefinition, implementationType));
            }

            foreach (var service in services.OfType<IServiceWithType>())
            {
                if (!service.ServiceType.GetTypeInfo().IsGenericTypeDefinition)
                {
                    throw new ArgumentException(
                        string.Format(CultureInfo.CurrentCulture, OpenGenericServiceBinderResources.ServiceTypeMustBeOpenGenericTypeDefinition, service));
                }

                if (service.ServiceType.GetTypeInfo().IsInterface)
                {
                    if (GetInterface(implementationType, service.ServiceType) == null)
                        throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, OpenGenericServiceBinderResources.InterfaceIsNotImplemented, implementationType, service));
                }
                else
                {
                    if (!Traverse.Across(implementationType, t => t.GetTypeInfo().BaseType).Any(t => IsCompatibleGenericClassDefinition(t, service.ServiceType)))
                        throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, OpenGenericServiceBinderResources.TypesAreNotConvertible, implementationType, service));
                }
            }
        }

        private static bool IsCompatibleGenericClassDefinition(Type implementor, Type serviceType)
        {
            return implementor == serviceType || (implementor.GetTypeInfo().IsGenericType && implementor.GetGenericTypeDefinition() == serviceType);
        }
    }
}
