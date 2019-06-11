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
using System.Linq;
using System.Reflection;
using Autofac.Builder;
using Autofac.Core;
using Autofac.Util;

namespace Autofac.Features.Scanning
{
    internal static class ScanningRegistrationExtensions
    {
        public static IRegistrationBuilder<object, ScanningActivatorData, DynamicRegistrationStyle>
            RegisterAssemblyTypes(ContainerBuilder builder, params Assembly[] assemblies)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (assemblies == null) throw new ArgumentNullException(nameof(assemblies));

            var rb = new RegistrationBuilder<object, ScanningActivatorData, DynamicRegistrationStyle>(
                new TypedService(typeof(object)),
                new ScanningActivatorData(),
                new DynamicRegistrationStyle());

            rb.RegistrationData.DeferredCallback = builder.RegisterCallback(cr => ScanAssemblies(assemblies, cr, rb));

            return rb;
        }

        public static IRegistrationBuilder<object, ScanningActivatorData, DynamicRegistrationStyle>
            RegisterTypes(ContainerBuilder builder, params Type[] types)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (types == null) throw new ArgumentNullException(nameof(types));

            var rb = new RegistrationBuilder<object, ScanningActivatorData, DynamicRegistrationStyle>(
                new TypedService(typeof(object)),
                new ScanningActivatorData(),
                new DynamicRegistrationStyle());

            rb.RegistrationData.DeferredCallback = builder.RegisterCallback(cr => ScanTypes(types, cr, rb));

            return rb;
        }

        private static void ScanAssemblies(IEnumerable<Assembly> assemblies, IComponentRegistry cr, IRegistrationBuilder<object, ScanningActivatorData, DynamicRegistrationStyle> rb)
        {
            ScanTypes(assemblies.SelectMany(a => a.GetLoadableTypes()), cr, rb);
        }

        private static void ScanTypes(IEnumerable<Type> types, IComponentRegistry cr, IRegistrationBuilder<object, ScanningActivatorData, DynamicRegistrationStyle> rb)
        {
            rb.ActivatorData.Filters.Add(t =>
                rb.RegistrationData.Services.OfType<IServiceWithType>().All(swt =>
                    swt.ServiceType.GetTypeInfo().IsAssignableFrom(t.GetTypeInfo())));

            // Issue #897: For back compat reasons we can't filter out
            // non-public types here. Folks use assembly scanning on their
            // own stuff, so encapsulation is a tricky thing to manage.
            // If people want only public types, a LINQ Where clause can be used.
            foreach (var t in types
                .Where(t =>
                    t.GetTypeInfo().IsClass &&
                    !t.GetTypeInfo().IsAbstract &&
                    !t.GetTypeInfo().IsGenericTypeDefinition &&
                    !t.IsDelegate() &&
                    rb.ActivatorData.Filters.All(p => p(t)) &&
                    !t.IsCompilerGenerated()))
            {
                var scanned = RegistrationBuilder.ForType(t)
                    .FindConstructorsWith(rb.ActivatorData.ConstructorFinder)
                    .UsingConstructor(rb.ActivatorData.ConstructorSelector)
                    .WithParameters(rb.ActivatorData.ConfiguredParameters)
                    .WithProperties(rb.ActivatorData.ConfiguredProperties);

                scanned.RegistrationData.CopyFrom(rb.RegistrationData, false);

                foreach (var action in rb.ActivatorData.ConfigurationActions)
                    action(t, scanned);

                if (scanned.RegistrationData.Services.Any())
                    RegistrationBuilder.RegisterSingleComponent(cr, scanned);
            }

            foreach (var postScanningCallback in rb.ActivatorData.PostScanningCallbacks)
                postScanningCallback(cr);
        }

        public static IRegistrationBuilder<TLimit, TScanningActivatorData, TRegistrationStyle>
            AsClosedTypesOf<TLimit, TScanningActivatorData, TRegistrationStyle>(
                IRegistrationBuilder<TLimit, TScanningActivatorData, TRegistrationStyle> registration,
                Type openGenericServiceType)
            where TScanningActivatorData : ScanningActivatorData
        {
            if (openGenericServiceType == null) throw new ArgumentNullException(nameof(openGenericServiceType));

            return registration
                .Where(candidateType => candidateType.IsClosedTypeOf(openGenericServiceType))
                .As(candidateType => candidateType.GetTypesThatClose(openGenericServiceType)
                        .Select(t => (Service)new TypedService(t)));
        }

        public static IRegistrationBuilder<TLimit, TScanningActivatorData, TRegistrationStyle>
            AsClosedTypesOf<TLimit, TScanningActivatorData, TRegistrationStyle>(
                IRegistrationBuilder<TLimit, TScanningActivatorData, TRegistrationStyle> registration,
                Type openGenericServiceType,
                object serviceKey)
            where TScanningActivatorData : ScanningActivatorData
        {
            if (openGenericServiceType == null) throw new ArgumentNullException(nameof(openGenericServiceType));
            if (serviceKey == null) throw new ArgumentNullException(nameof(serviceKey));

            return AsClosedTypesOf(registration, openGenericServiceType, t => serviceKey);
        }

        public static IRegistrationBuilder<TLimit, TScanningActivatorData, TRegistrationStyle>
            AsClosedTypesOf<TLimit, TScanningActivatorData, TRegistrationStyle>(
                IRegistrationBuilder<TLimit, TScanningActivatorData, TRegistrationStyle> registration,
                Type openGenericServiceType,
                Func<Type, object> serviceKeyMapping)
            where TScanningActivatorData : ScanningActivatorData
        {
            if (openGenericServiceType == null) throw new ArgumentNullException(nameof(openGenericServiceType));

            return registration
                .Where(candidateType => candidateType.IsClosedTypeOf(openGenericServiceType))
                .As(candidateType => candidateType.GetTypesThatClose(openGenericServiceType)
                        .Select(t => (Service)new KeyedService(serviceKeyMapping(candidateType), t)));
        }

        public static IRegistrationBuilder<TLimit, TScanningActivatorData, TRegistrationStyle>
            AssignableTo<TLimit, TScanningActivatorData, TRegistrationStyle>(
                this IRegistrationBuilder<TLimit, TScanningActivatorData, TRegistrationStyle> registration,
                Type type)
            where TScanningActivatorData : ScanningActivatorData
        {
            if (registration == null) throw new ArgumentNullException(nameof(registration));

            registration.ActivatorData.Filters.Add(t => type.GetTypeInfo().IsAssignableFrom(t.GetTypeInfo()));
            return registration;
        }

        public static IRegistrationBuilder<TLimit, TScanningActivatorData, TRegistrationStyle>
            As<TLimit, TScanningActivatorData, TRegistrationStyle>(
                IRegistrationBuilder<TLimit, TScanningActivatorData, TRegistrationStyle> registration,
                Func<Type, IEnumerable<Service>> serviceMapping)
            where TScanningActivatorData : ScanningActivatorData
        {
            if (registration == null) throw new ArgumentNullException(nameof(registration));
            if (serviceMapping == null) throw new ArgumentNullException(nameof(serviceMapping));

            registration.ActivatorData.ConfigurationActions.Add((t, rb) =>
            {
                var mapped = serviceMapping(t);
                var impl = rb.ActivatorData.ImplementationType;
                var applied = mapped.Where(s =>
                    {
                        var c = s as IServiceWithType;
                        return
                            (c == null && s != null) || // s is not an IServiceWithType
                            c.ServiceType.GetTypeInfo().IsAssignableFrom(impl.GetTypeInfo());
                    });
                rb.As(applied.ToArray());
            });

            return registration;
        }

        public static IRegistrationBuilder<TLimit, TScanningActivatorData, TRegistrationStyle>
            PreserveExistingDefaults<TLimit, TScanningActivatorData, TRegistrationStyle>(
            IRegistrationBuilder<TLimit, TScanningActivatorData, TRegistrationStyle> registration)
            where TScanningActivatorData : ScanningActivatorData
        {
            if (registration == null) throw new ArgumentNullException(nameof(registration));

            registration.ActivatorData.ConfigurationActions.Add((t, r) => r.PreserveExistingDefaults());

            return registration;
        }
    }
}
