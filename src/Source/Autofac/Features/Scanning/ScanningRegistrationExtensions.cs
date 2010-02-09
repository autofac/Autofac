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
using System.Reflection;
using Autofac.Builder;
using Autofac.Core;
using Autofac.Core.Activators.Reflection;
using Autofac.Util;

namespace Autofac.Features.Scanning
{
    static class ScanningRegistrationExtensions
    {
        /// <summary>
        /// Register the types in an assembly.
        /// </summary>
        /// <param name="builder">Container builder.</param>
        /// <param name="assemblies">The assemblies from which to register types.</param>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        public static RegistrationBuilder<object, ScanningActivatorData, DynamicRegistrationStyle>
            RegisterAssemblyTypes(ContainerBuilder builder, params Assembly[] assemblies)
        {
            Enforce.ArgumentNotNull(builder, "builder");
            Enforce.ArgumentNotNull(assemblies, "assemblies");

            var rb = new RegistrationBuilder<object, ScanningActivatorData, DynamicRegistrationStyle>(
                new ScanningActivatorData(),
                new DynamicRegistrationStyle());

            builder.RegisterCallback(cr =>
            {
                var tempBuilder = new ContainerBuilder();
                foreach (var t in assemblies.SelectMany(a => a.GetTypes())
                    .Where(t => !t.IsAbstract && rb.ActivatorData.Filters.All(p => p(t))))
                {
                    var activator = new ReflectionActivator(
                        t,
                        rb.ActivatorData.ConstructorFinder,
                        rb.ActivatorData.ConstructorSelector,
                        rb.ActivatorData.ConfiguredParameters,
                        rb.ActivatorData.ConfiguredProperties);

                    var services = new HashSet<Service>(rb.RegistrationData.Services);

                    if (!services.Any() && !rb.ActivatorData.ServiceMappings.Any())
                        services.Add(new TypedService(t));

                    foreach (var mapping in rb.ActivatorData.ServiceMappings)
                        foreach (var s in mapping(t))
                            services.Add(s);

                    var r = RegistrationBuilder.CreateRegistration(Guid.NewGuid(), rb.RegistrationData, activator, services);
                    cr.Register(r);
                }
            });

            return rb;
        }

        /// <summary>
        /// Specifies that a type from a scanned assembly is registered if it implements an interface
        /// that closes the provided open generic interface type.
        /// </summary>
        /// <typeparam name="TLimit">Registration limit type.</typeparam>
        /// <typeparam name="TRegistrationStyle">Registration style.</typeparam>
        /// <typeparam name="TScanningActivatorData">Activator data type.</typeparam>
        /// <param name="registration">Registration to set service mapping on.</param>
        /// <param name="openGenericServiceType">The open generic interface or base class type for which implementations will be found.</param>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        public static RegistrationBuilder<TLimit, TScanningActivatorData, TRegistrationStyle>
            AsClosedTypesOf<TLimit, TScanningActivatorData, TRegistrationStyle>(
                RegistrationBuilder<TLimit, TScanningActivatorData, TRegistrationStyle> registration, 
                Type openGenericServiceType)
            where TScanningActivatorData : ScanningActivatorData
        {
            Enforce.ArgumentNotNull(openGenericServiceType, "openGenericServiceType");

            if (!(openGenericServiceType.IsGenericTypeDefinition || openGenericServiceType.ContainsGenericParameters))
            {
                throw new ArgumentException(
                    string.Format(ScanningRegistrationExtensionsResources.NotOpenGenericType, openGenericServiceType.FullName));
            }

            return registration
                .Where(candidateType => IsAssignableToClosed(candidateType, openGenericServiceType))
                .As(candidateType => GetServiceThatCloses(candidateType, openGenericServiceType));
        }

        /// <summary>Determines whether the candidate type supports any base or interface that closes the
        /// provided generic service type.</summary>
        /// <param name="candidateType">The type that is being checked for the interface.</param>
        /// <param name="openGenericServiceType">The open generic type to locate.</param>
        /// <returns>True if an interface was found; otherwise false.</returns>
        static bool IsAssignableToClosed(Type candidateType, Type openGenericServiceType)
        {
            Type unused;
            return FindAssignableTypeThatCloses(candidateType, openGenericServiceType, out unused);
        }

        /// <summary>Returns the first concrete interface supported by the candidate type that
        /// closes the provided open generic service type.</summary>
        /// <param name="candidateType">The type that is being checked for the interface.</param>
        /// <param name="openGenericServiceType">The open generic type to locate.</param>
        /// <returns>The type of the interface.</returns>
        static Type GetServiceThatCloses(Type candidateType, Type openGenericServiceType)
        {
            Type closed;
            FindAssignableTypeThatCloses(candidateType, openGenericServiceType, out closed);
            return Enforce.NotNull(closed);
        }

        /// <summary>
        /// Looks for an interface on the candidate type that closes the provided open generic interface type.
        /// </summary>
        /// <param name="candidateType">The type that is being checked for the interface.</param>
        /// <param name="openGenericServiceType">The open generic service type to locate.</param>
        /// <param name="closedServiceType">The type of the closed service if found.</param>
        /// <returns>True if a closed implementation was found; otherwise false.</returns>
        static bool FindAssignableTypeThatCloses(Type candidateType, Type openGenericServiceType, out Type closedServiceType)
        {
            closedServiceType = null;

            if (candidateType.IsAbstract) return false;

            foreach (Type interfaceType in TypesAssignableFrom(candidateType))
            {
                if (interfaceType.IsGenericType && interfaceType.GetGenericTypeDefinition() == openGenericServiceType)
                {
                    closedServiceType = interfaceType;
                    return true;
                }
            }

            return false;
        }

        static IEnumerable<Type> TypesAssignableFrom(Type candidateType)
        {
            foreach (var iface in candidateType.GetInterfaces())
                yield return iface;

            var next = candidateType;
            while (next != typeof(object))
            {
                yield return next;
                next = next.BaseType;
            }
        }
    }
}
