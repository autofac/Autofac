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
using Autofac.Util;

namespace Autofac.Features.Scanning
{
    static class ScanningRegistrationExtensions
    {
        public static IRegistrationBuilder<object, ScanningActivatorData, DynamicRegistrationStyle>
            RegisterAssemblyTypes(ContainerBuilder builder, params Assembly[] assemblies)
        {
            Enforce.ArgumentNotNull(builder, "builder");
            Enforce.ArgumentNotNull(assemblies, "assemblies");

            var rb = new RegistrationBuilder<object, ScanningActivatorData, DynamicRegistrationStyle>(
                new TypedService(typeof(object)),
                new ScanningActivatorData(),
                new DynamicRegistrationStyle());

            builder.RegisterCallback(cr => ScanAssemblies(assemblies, cr, rb));

            return rb;
        }

        static void ScanAssemblies(IEnumerable<Assembly> assemblies, IComponentRegistry cr, IRegistrationBuilder<object, ScanningActivatorData, DynamicRegistrationStyle> rb)
        {
            rb.ActivatorData.Filters.Add(t =>
                rb.RegistrationData.Services.OfType<IServiceWithType>().All(swt =>
                    swt.ServiceType.IsAssignableFrom(t)));

            foreach (var t in assemblies
                .SelectMany(a => a.GetTypes())
                .Where(t =>
                    t.IsClass &&
                    !t.IsAbstract &&
                    !t.IsGenericTypeDefinition &&
                    !t.IsDelegate() &&
                    rb.ActivatorData.Filters.All(p => p(t))))
            {
                var scanned = RegistrationBuilder.ForType(t)
                    .FindConstructorsWith(rb.ActivatorData.ConstructorFinder)
                    .UsingConstructor(rb.ActivatorData.ConstructorSelector)
                    .WithParameters(rb.ActivatorData.ConfiguredParameters)
                    .WithProperties(rb.ActivatorData.ConfiguredProperties);

                scanned.RegistrationData.CopyFrom(rb.RegistrationData, false);

                foreach (var action in rb.ActivatorData.ConfigurationActions)
                    action(t, scanned);

                RegistrationBuilder.RegisterSingleComponent(cr, scanned);
            }
        }

        public static IRegistrationBuilder<TLimit, TScanningActivatorData, TRegistrationStyle>
            AsClosedTypesOf<TLimit, TScanningActivatorData, TRegistrationStyle>(
                IRegistrationBuilder<TLimit, TScanningActivatorData, TRegistrationStyle> registration, 
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
                .As(candidateType => GetServicesThatClose(candidateType, openGenericServiceType));
        }

        /// <summary>Determines whether the candidate type supports any base or interface that closes the
        /// provided generic service type.</summary>
        /// <param name="candidateType">The type that is being checked for the interface.</param>
        /// <param name="openGenericServiceType">The open generic type to locate.</param>
        /// <returns>True if an interface was found; otherwise false.</returns>
        static bool IsAssignableToClosed(Type candidateType, Type openGenericServiceType)
        {
            return FindAssignableTypesThatClose(candidateType, openGenericServiceType).Any();
        }

        /// <summary>Returns the first concrete interface supported by the candidate type that
        /// closes the provided open generic service type.</summary>
        /// <param name="candidateType">The type that is being checked for the interface.</param>
        /// <param name="openGenericServiceType">The open generic type to locate.</param>
        /// <returns>The type of the interface.</returns>
        static IEnumerable<Service> GetServicesThatClose(Type candidateType, Type openGenericServiceType)
        {
            return FindAssignableTypesThatClose(candidateType, openGenericServiceType)
                .Select(t => new TypedService(t))
// ReSharper disable RedundantEnumerableCastCall
                .Cast<Service>();
// ReSharper restore RedundantEnumerableCastCall
        }

        /// <summary>
        /// Looks for an interface on the candidate type that closes the provided open generic interface type.
        /// </summary>
        /// <param name="candidateType">The type that is being checked for the interface.</param>
        /// <param name="openGenericServiceType">The open generic service type to locate.</param>
        /// <returns>True if a closed implementation was found; otherwise false.</returns>
        static IEnumerable<Type> FindAssignableTypesThatClose(Type candidateType, Type openGenericServiceType)
        {
            if (candidateType.IsAbstract) yield break;

            foreach (var assignableType in TypesAssignableFrom(candidateType)
                .Where(t => t.IsClosingTypeOf(openGenericServiceType)))
            {
                yield return assignableType;
            }
        }

        static IEnumerable<Type> TypesAssignableFrom(Type candidateType)
        {
            return candidateType.GetInterfaces().Concat(
                Traverse.Across(candidateType, t => t.BaseType));
        }

        public static IRegistrationBuilder<TLimit, TScanningActivatorData, TRegistrationStyle>
            AssignableTo<TLimit, TScanningActivatorData, TRegistrationStyle>(
                this IRegistrationBuilder<TLimit, TScanningActivatorData, TRegistrationStyle> registration,
                Type type)
            where TScanningActivatorData : ScanningActivatorData
        {
            Enforce.ArgumentNotNull(registration, "registration");

            registration.ActivatorData.Filters.Add(type.IsAssignableFrom);
            return registration;
        }
    }
}
