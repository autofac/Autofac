// This software is part of the Autofac IoC container
// Copyright © 2011 Autofac Contributors
// https://autofac.org
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
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using Autofac.Core;
using Autofac.Util;

namespace Autofac.Features.OpenGenerics
{
    /// <summary>
    /// Helper functions for binding open generic implementations to a known implementation type.
    /// </summary>
    internal static class OpenGenericServiceBinder
    {
        /// <summary>
        /// Given a closed generic service (that is being requested), creates a closed generic implementation type
        /// and associated services from the open generic implementation and services.
        /// </summary>
        /// <param name="closedService">The closed generic service to bind.</param>
        /// <param name="configuredOpenGenericServices">The set of configured open generic services.</param>
        /// <param name="openGenericImplementationType">The implementation type of the open generic.</param>
        /// <param name="constructedImplementationType">The built closed generic implementation type.</param>
        /// <param name="constructedServices">The built closed generic services.</param>
        /// <returns>True if the closed generic service can be bound. False otherwise.</returns>
        public static bool TryBindOpenGenericService(
            Service closedService,
            IEnumerable<Service> configuredOpenGenericServices,
            Type openGenericImplementationType,
            [NotNullWhen(returnValue: true)] out Type? constructedImplementationType,
            [NotNullWhen(returnValue: true)] out Service[]? constructedServices)
        {
            if (closedService is IServiceWithType swt)
            {
                return TryBindOpenGenericTypedService(swt, configuredOpenGenericServices, openGenericImplementationType, out constructedImplementationType, out constructedServices);
            }

            constructedImplementationType = null;
            constructedServices = null;
            return false;
        }

        /// <summary>
        /// Given a closed generic service (that is being requested), creates a closed generic implementation type
        /// and associated services from the open generic implementation and services.
        /// </summary>
        /// <param name="closedService">The closed generic service to bind.</param>
        /// <param name="configuredOpenGenericServices">The set of configured open generic services.</param>
        /// <param name="openGenericFactory">Delegate responsible for generating an instance of a closed generic based on the open generic type being registered.</param>
        /// <param name="constructedFactory">The built closed generic implementation type.</param>
        /// <param name="constructedServices">The built closed generic services.</param>
        /// <returns>True if the closed generic service can be bound. False otherwise.</returns>
        public static bool TryBindOpenGenericDelegate(
            Service closedService,
            IEnumerable<Service> configuredOpenGenericServices,
            Func<IComponentContext, Type[], IEnumerable<Parameter>, object> openGenericFactory,
            [NotNullWhen(returnValue: true)] out Func<IComponentContext, IEnumerable<Parameter>, object>? constructedFactory,
            [NotNullWhen(returnValue: true)] out Service[]? constructedServices)
        {
            if (closedService is IServiceWithType swt)
            {
                return TryBindOpenGenericDelegateService(swt, configuredOpenGenericServices, openGenericFactory, out constructedFactory, out constructedServices);
            }

            constructedFactory = null;
            constructedServices = null;
            return false;
        }

        /// <summary>
        /// Given a closed generic service (that is being requested), creates a closed generic implementation type
        /// and associated services from the open generic implementation and services.
        /// </summary>
        /// <param name="serviceWithType">The closed generic service to bind.</param>
        /// <param name="configuredOpenGenericServices">The set of configured open generic services.</param>
        /// <param name="openGenericImplementationType">The implementation type of the open generic.</param>
        /// <param name="constructedImplementationType">The built closed generic implementation type.</param>
        /// <param name="constructedServices">The built closed generic services.</param>
        /// <returns>True if the closed generic service can be bound. False otherwise.</returns>
        public static bool TryBindOpenGenericTypedService(
            IServiceWithType serviceWithType,
            IEnumerable<Service> configuredOpenGenericServices,
            Type openGenericImplementationType,
            [NotNullWhen(returnValue: true)] out Type? constructedImplementationType,
            [NotNullWhen(returnValue: true)] out Service[]? constructedServices)
        {
            if (serviceWithType.ServiceType.IsGenericType && !serviceWithType.ServiceType.IsGenericTypeDefinition)
            {
                var definitionService = (IServiceWithType)serviceWithType.ChangeType(serviceWithType.ServiceType.GetGenericTypeDefinition());
                var serviceGenericArguments = serviceWithType.ServiceType.GetGenericArguments();

                if (configuredOpenGenericServices.Cast<IServiceWithType>().Any(s => s.Equals(definitionService)))
                {
                    var implementorGenericArguments = TryMapImplementationGenericArguments(
                        openGenericImplementationType, serviceWithType.ServiceType, definitionService.ServiceType, serviceGenericArguments);

                    if (implementorGenericArguments.All(a => a != null) &&
                        openGenericImplementationType.IsCompatibleWithGenericParameterConstraints(implementorGenericArguments))
                    {
                        var constructedImplementationTypeTmp = openGenericImplementationType.MakeGenericType(implementorGenericArguments);

                        var implementedServices = configuredOpenGenericServices
                            .Cast<IServiceWithType>()
                            .Where(s => s.ServiceType.GetGenericArguments().Length == serviceGenericArguments.Length)
                            .Select(s => new { ServiceWithType = s, GenericService = s.ServiceType.MakeGenericType(serviceGenericArguments) })
                            .Where(p => p.GenericService.IsAssignableFrom(constructedImplementationTypeTmp))
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

        /// <summary>
        /// Given a closed generic service (that is being requested), creates a regular delegate callback
        /// and associated services from the open generic delegate and services.
        /// </summary>
        /// <param name="serviceWithType">The closed generic service to bind.</param>
        /// <param name="configuredOpenGenericServices">The set of configured open generic services.</param>
        /// <param name="openGenericFactory">Delegate responsible for generating an instance of a closed generic based on the open generic type being registered.</param>
        /// <param name="constructedFactory">The built closed generic implementation type.</param>
        /// <param name="constructedServices">The built closed generic services.</param>
        /// <returns>True if the closed generic service can be bound. False otherwise.</returns>
        public static bool TryBindOpenGenericDelegateService(
            IServiceWithType serviceWithType,
            IEnumerable<Service> configuredOpenGenericServices,
            Func<IComponentContext, Type[], IEnumerable<Parameter>, object> openGenericFactory,
            [NotNullWhen(returnValue: true)] out Func<IComponentContext, IEnumerable<Parameter>, object>? constructedFactory,
            [NotNullWhen(returnValue: true)] out Service[]? constructedServices)
        {
            if (serviceWithType.ServiceType.IsGenericType && !serviceWithType.ServiceType.IsGenericTypeDefinition)
            {
                var definitionService = (IServiceWithType)serviceWithType.ChangeType(serviceWithType.ServiceType.GetGenericTypeDefinition());
                var serviceGenericArguments = serviceWithType.ServiceType.GetGenericArguments();

                if (configuredOpenGenericServices.Cast<IServiceWithType>().Any(s => s.Equals(definitionService)))
                {
                    constructedFactory = (ctx, parameters) => openGenericFactory(ctx, serviceGenericArguments, parameters);

                    var implementedServices = configuredOpenGenericServices
                        .OfType<IServiceWithType>()
                        .Where(s => s.ServiceType.GetGenericArguments().Length == serviceGenericArguments.Length)
                        .Select(s => new { ServiceWithType = s, GenericService = s.ServiceType.MakeGenericType(serviceGenericArguments) })
                        .Select(p => p.ServiceWithType.ChangeType(p.GenericService))
                        .ToArray();

                    constructedServices = implementedServices;
                    return true;
                }
            }

            constructedFactory = null;
            constructedServices = null;
            return false;
        }

        private static Type[] TryMapImplementationGenericArguments(Type implementationType, Type serviceType, Type serviceTypeDefinition, Type[] serviceGenericArguments)
        {
            if (serviceTypeDefinition == implementationType)
            {
                return serviceGenericArguments;
            }

            var implementationGenericArgumentDefinitions = implementationType.GetGenericArguments();
            var serviceArgumentDefinitions = serviceType.IsInterface ?
                    GetInterface(implementationType, serviceType).GenericTypeArguments :
                    serviceTypeDefinition.GetGenericArguments();

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
                return implementationType.GetInterfaces()
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
                .Where(argdef => !argdef.Key.IsGenericType && implementationGenericArgumentDefinition.Name == argdef.Key.Name)
                .Select(argdef => argdef.Value)
                .FirstOrDefault();

            if (matchingRegularType != null)
            {
                return matchingRegularType;
            }

            return serviceArgumentDefinitionToArgument
                .Where(argdef => argdef.Key.IsGenericType && argdef.Value.GenericTypeArguments.Length > 0)
                .Select(argdef => TryFindServiceArgumentForImplementationArgumentDefinition(
                    implementationGenericArgumentDefinition, argdef.Key.GenericTypeArguments.Zip(
                        argdef.Value.GenericTypeArguments, (a, b) => new KeyValuePair<Type, Type>(a, b))))
                .FirstOrDefault(x => x != null);
        }

        /// <summary>
        /// Throws an exception if an open generic implementation type cannot implement the set of specified open services.
        /// </summary>
        /// <param name="implementationType">The open generic implementation type.</param>
        /// <param name="services">The set of open generic services.</param>
        public static void EnforceBindable(Type implementationType, IEnumerable<Service> services)
        {
            if (implementationType == null)
            {
                throw new ArgumentNullException(nameof(implementationType));
            }

            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (!implementationType.IsGenericTypeDefinition)
            {
                throw new ArgumentException(
                    string.Format(CultureInfo.CurrentCulture, OpenGenericServiceBinderResources.ImplementorMustBeOpenGenericTypeDefinition, implementationType));
            }

            foreach (var service in services.OfType<IServiceWithType>())
            {
                if (!service.ServiceType.IsGenericTypeDefinition)
                {
                    throw new ArgumentException(
                        string.Format(CultureInfo.CurrentCulture, OpenGenericServiceBinderResources.ServiceTypeMustBeOpenGenericTypeDefinition, service));
                }

                if (service.ServiceType.IsInterface)
                {
                    if (GetInterface(implementationType, service.ServiceType) == null)
                    {
                        throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, OpenGenericServiceBinderResources.InterfaceIsNotImplemented, implementationType, service));
                    }
                }
                else
                {
                    if (!Traverse.Across(implementationType, t => t.BaseType).Any(t => IsCompatibleGenericClassDefinition(t, service.ServiceType)))
                    {
                        throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, OpenGenericServiceBinderResources.TypesAreNotConvertible, implementationType, service));
                    }
                }
            }
        }

        private static bool IsCompatibleGenericClassDefinition(Type implementor, Type serviceType)
        {
            return implementor == serviceType || (implementor.IsGenericType && implementor.GetGenericTypeDefinition() == serviceType);
        }
    }
}
