// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Autofac.Builder;
using Autofac.Core;
using Autofac.Core.Registration;
using Autofac.Features.OpenGenerics;
using Autofac.Util;

namespace Autofac.Features.Scanning
{
    /// <summary>
    /// Helper methods to assist in scanning open generic registration.
    /// </summary>
    internal static class OpenGenericScanningRegistrationExtensions
    {
        /// <summary>
        /// Register open generic types from the specified assemblies.
        /// </summary>
        /// <param name="builder">The container builder.</param>
        /// <param name="assemblies">The set of assemblies.</param>
        /// <returns>A registration builder.</returns>
        public static IRegistrationBuilder<object, OpenGenericScanningActivatorData, DynamicRegistrationStyle>
            RegisterOpenGenericAssemblyTypes(ContainerBuilder builder, params Assembly[] assemblies)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (assemblies == null)
            {
                throw new ArgumentNullException(nameof(assemblies));
            }

            var rb = new RegistrationBuilder<object, OpenGenericScanningActivatorData, DynamicRegistrationStyle>(
                new TypedService(typeof(object)),
                new OpenGenericScanningActivatorData(),
                new DynamicRegistrationStyle());

            rb.RegistrationData.DeferredCallback = builder.RegisterCallback(cr => ScanAssemblies(assemblies, cr, rb));

            return rb;
        }

        private static void ScanAssemblies(IEnumerable<Assembly> assemblies, IComponentRegistryBuilder cr, IRegistrationBuilder<object, OpenGenericScanningActivatorData, DynamicRegistrationStyle> rb)
        {
            ScanTypes(assemblies.SelectMany(a => a.GetLoadableTypes()), cr, rb);
        }

        private static void ScanTypes(IEnumerable<Type> types, IComponentRegistryBuilder cr, IRegistrationBuilder<object, OpenGenericScanningActivatorData, DynamicRegistrationStyle> rb)
        {
            rb.ActivatorData.Filters.Add(t =>
                rb.RegistrationData.Services.OfType<IServiceWithType>().All(swt =>
                    t.IsOpenGenericTypeOf(swt.ServiceType)));

            // Issue #897: For back compat reasons we can't filter out
            // non-public types here. Folks use assembly scanning on their
            // own stuff, so encapsulation is a tricky thing to manage.
            // If people want only public types, a LINQ Where clause can be used.
            foreach (var t in types
                .Where(t =>
                    t.IsClass &&
                    !t.IsAbstract &&
                    t.IsGenericTypeDefinition &&
                    !t.IsDelegate() &&
                    rb.ActivatorData.Filters.All(p => p(t)) &&
                    !t.IsCompilerGenerated()))
            {
                var scanned = new RegistrationBuilder<object, ReflectionActivatorData, DynamicRegistrationStyle>(
                    new TypedService(t),
                    new ReflectionActivatorData(t),
                    new DynamicRegistrationStyle());

                scanned
                    .FindConstructorsWith(rb.ActivatorData.ConstructorFinder)
                    .UsingConstructor(rb.ActivatorData.ConstructorSelector)
                    .WithParameters(rb.ActivatorData.ConfiguredParameters)
                    .WithProperties(rb.ActivatorData.ConfiguredProperties);

                // Copy middleware from the scanning registration.
                scanned.ResolvePipeline.UseRange(rb.ResolvePipeline.Middleware);

                scanned.RegistrationData.CopyFrom(rb.RegistrationData, false);

                foreach (var action in rb.ActivatorData.ConfigurationActions)
                {
                    action(t, scanned);
                }

                if (scanned.RegistrationData.Services.Any())
                {
                    cr.AddRegistrationSource(new OpenGenericRegistrationSource(scanned.RegistrationData, scanned.ResolvePipeline, scanned.ActivatorData));
                }
            }

            foreach (var postScanningCallback in rb.ActivatorData.PostScanningCallbacks)
            {
                postScanningCallback(cr);
            }
        }

        /// <summary>
        /// Specifies how an open generic type from a scanned assembly is mapped to a service.
        /// </summary>
        /// <typeparam name="TLimit">Registration limit type.</typeparam>
        /// <typeparam name="TRegistrationStyle">Registration style.</typeparam>
        /// <param name="registration">Registration to set service mapping on.</param>
        /// <param name="serviceMapping">Function mapping types to services.</param>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        public static IRegistrationBuilder<TLimit, OpenGenericScanningActivatorData, TRegistrationStyle>
            As<TLimit, TRegistrationStyle>(
                IRegistrationBuilder<TLimit, OpenGenericScanningActivatorData, TRegistrationStyle> registration,
                Func<Type, IEnumerable<Service>> serviceMapping)
        {
            if (registration == null)
            {
                throw new ArgumentNullException(nameof(registration));
            }

            if (serviceMapping == null)
            {
                throw new ArgumentNullException(nameof(serviceMapping));
            }

            registration.ActivatorData.ConfigurationActions.Add((t, rb) =>
            {
                var mapped = serviceMapping(t);
                var impl = rb.ActivatorData.ImplementationType;
                var applied = mapped.Where(s =>
                {
                    if (s is IServiceWithType c)
                    {
                        return impl.IsOpenGenericTypeOf(c.ServiceType);
                    }

                    return s != null;
                });
                rb.As(applied.ToArray());
            });

            return registration;
        }

        /// <summary>
        /// Filters the scanned open generic types to exclude the provided type.
        /// </summary>
        /// <param name="registration">Registration to filter types from.</param>
        /// <param name="openGenericType">The open generic type to exclude.</param>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        public static IRegistrationBuilder<object, OpenGenericScanningActivatorData, DynamicRegistrationStyle>
            Except(this IRegistrationBuilder<object, OpenGenericScanningActivatorData, DynamicRegistrationStyle> registration, Type openGenericType)
        {
            return registration.Where(t => t != openGenericType);
        }

        /// <summary>
        /// Filters the scanned open generic types to exclude the provided type, providing specific configuration for
        /// the excluded type.
        /// </summary>
        /// <param name="registration">Registration to filter types from.</param>
        /// <param name="openGenericType">The concrete type to exclude.</param>
        /// <param name="customizedRegistration">Registration for the excepted type.</param>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        public static IRegistrationBuilder<object, OpenGenericScanningActivatorData, DynamicRegistrationStyle>
            Except(
                this IRegistrationBuilder<object, OpenGenericScanningActivatorData, DynamicRegistrationStyle> registration,
                Type openGenericType,
                Action<IRegistrationBuilder<object, ReflectionActivatorData, DynamicRegistrationStyle>> customizedRegistration)
        {
            var result = registration.Except(openGenericType);

            result.ActivatorData.PostScanningCallbacks.Add(cr =>
            {
                var rb = new RegistrationBuilder<object, ReflectionActivatorData, DynamicRegistrationStyle>(
                    new TypedService(openGenericType),
                    new ReflectionActivatorData(openGenericType),
                    new DynamicRegistrationStyle());

                customizedRegistration(rb);

                cr.AddRegistrationSource(new OpenGenericRegistrationSource(rb.RegistrationData, rb.ResolvePipeline, rb.ActivatorData));
            });

            return result;
        }
    }
}
