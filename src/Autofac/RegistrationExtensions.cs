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
using System.Linq.Expressions;
using System.Reflection;
using Autofac.Builder;
using Autofac.Core;
using Autofac.Core.Activators.ProvidedInstance;
using Autofac.Core.Activators.Reflection;
using Autofac.Core.Lifetime;
using Autofac.Features.Decorators;
using Autofac.Features.LightweightAdapters;
using Autofac.Features.OpenGenerics;
using Autofac.Features.Scanning;
using Autofac.Util;

namespace Autofac
{
    /// <summary>
    /// Adds registration syntax to the <see cref="ContainerBuilder"/> type.
    /// </summary>
    [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    public static class RegistrationExtensions
    {
        /// <summary>
        /// Add a component to the container.
        /// </summary>
        /// <param name="builder">The builder to register the component with.</param>
        /// <param name="registration">The component to add.</param>
        public static void RegisterComponent(this ContainerBuilder builder, IComponentRegistration registration)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (registration == null) throw new ArgumentNullException(nameof(registration));

            builder.RegisterCallback(cr => cr.Register(registration));
        }

        /// <summary>
        /// Add a registration source to the container.
        /// </summary>
        /// <param name="builder">The builder to register the registration source via.</param>
        /// <param name="registrationSource">The registration source to add.</param>
        public static void RegisterSource(this ContainerBuilder builder, IRegistrationSource registrationSource)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (registrationSource == null) throw new ArgumentNullException(nameof(registrationSource));

            builder.RegisterCallback(cr => cr.AddRegistrationSource(registrationSource));
        }

        /// <summary>
        /// Register an instance as a component.
        /// </summary>
        /// <typeparam name="T">The type of the instance.</typeparam>
        /// <param name="builder">Container builder.</param>
        /// <param name="instance">The instance to register.</param>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        /// <remarks>If no services are explicitly specified for the instance, the
        /// static type <typeparamref name="T"/> will be used as the default service (i.e. *not* <c>instance.GetType()</c>).</remarks>
        public static IRegistrationBuilder<T, SimpleActivatorData, SingleRegistrationStyle>
            RegisterInstance<T>(this ContainerBuilder builder, T instance)
            where T : class
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (instance == null) throw new ArgumentNullException(nameof(instance));

            var activator = new ProvidedInstanceActivator(instance);

            var rb = new RegistrationBuilder<T, SimpleActivatorData, SingleRegistrationStyle>(
                new TypedService(typeof(T)),
                new SimpleActivatorData(activator),
                new SingleRegistrationStyle());

            rb.SingleInstance();

            rb.RegistrationData.DeferredCallback = builder.RegisterCallback(cr =>
            {
                if (!(rb.RegistrationData.Lifetime is RootScopeLifetime) ||
                    rb.RegistrationData.Sharing != InstanceSharing.Shared)
                {
                    throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, RegistrationExtensionsResources.InstanceRegistrationsAreSingleInstanceOnly, instance));
                }

                activator.DisposeInstance = rb.RegistrationData.Ownership == InstanceOwnership.OwnedByLifetimeScope;

                RegistrationBuilder.RegisterSingleComponent(cr, rb);
            });

            return rb;
        }

        /// <summary>
        /// Register a component to be created through reflection.
        /// </summary>
        /// <typeparam name="TImplementer">The type of the component implementation.</typeparam>
        /// <param name="builder">Container builder.</param>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        public static IRegistrationBuilder<TImplementer, ConcreteReflectionActivatorData, SingleRegistrationStyle>
            RegisterType<TImplementer>(this ContainerBuilder builder)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            var rb = RegistrationBuilder.ForType<TImplementer>();

            rb.RegistrationData.DeferredCallback = builder.RegisterCallback(cr => RegistrationBuilder.RegisterSingleComponent(cr, rb));

            return rb;
        }

        /// <summary>
        /// Register a component to be created through reflection.
        /// </summary>
        /// <param name="implementationType">The type of the component implementation.</param>
        /// <param name="builder">Container builder.</param>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        public static IRegistrationBuilder<object, ConcreteReflectionActivatorData, SingleRegistrationStyle>
            RegisterType(this ContainerBuilder builder, Type implementationType)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (implementationType == null) throw new ArgumentNullException(nameof(implementationType));

            var rb = RegistrationBuilder.ForType(implementationType);

            rb.RegistrationData.DeferredCallback = builder.RegisterCallback(cr => RegistrationBuilder.RegisterSingleComponent(cr, rb));

            return rb;
        }

        /// <summary>
        /// Register a delegate as a component.
        /// </summary>
        /// <typeparam name="T">The type of the instance.</typeparam>
        /// <param name="builder">Container builder.</param>
        /// <param name="delegate">The delegate to register.</param>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        public static IRegistrationBuilder<T, SimpleActivatorData, SingleRegistrationStyle>
            Register<T>(
                this ContainerBuilder builder,
                Func<IComponentContext, T> @delegate)
        {
            if (@delegate == null) throw new ArgumentNullException(nameof(@delegate));

            return builder.Register((c, p) => @delegate(c));
        }

        /// <summary>
        /// Register a delegate as a component.
        /// </summary>
        /// <typeparam name="T">The type of the instance.</typeparam>
        /// <param name="builder">Container builder.</param>
        /// <param name="delegate">The delegate to register.</param>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        public static IRegistrationBuilder<T, SimpleActivatorData, SingleRegistrationStyle>
            Register<T>(
                this ContainerBuilder builder,
                Func<IComponentContext, IEnumerable<Parameter>, T> @delegate)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (@delegate == null) throw new ArgumentNullException(nameof(@delegate));

            var rb = RegistrationBuilder.ForDelegate(@delegate);

            rb.RegistrationData.DeferredCallback = builder.RegisterCallback(cr => RegistrationBuilder.RegisterSingleComponent(cr, rb));

            return rb;
        }

        /// <summary>
        /// Register an un-parameterised generic type, e.g. Repository&lt;&gt;.
        /// Concrete types will be made as they are requested, e.g. with Resolve&lt;Repository&lt;int&gt;&gt;().
        /// </summary>
        /// <param name="builder">Container builder.</param>
        /// <param name="implementer">The open generic implementation type.</param>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        public static IRegistrationBuilder<object, ReflectionActivatorData, DynamicRegistrationStyle>
            RegisterGeneric(this ContainerBuilder builder, Type implementer)
        {
            return OpenGenericRegistrationExtensions.RegisterGeneric(builder, implementer);
        }

        /// <summary>
        /// Specifies that the component being registered should only be made the default for services
        /// that have not already been registered.
        /// </summary>
        /// <typeparam name="TLimit">Registration limit type.</typeparam>
        /// <typeparam name="TActivatorData">Activator data type.</typeparam>
        /// <typeparam name="TSingleRegistrationStyle">Registration style.</typeparam>
        /// <param name="registration">Registration to set service mapping on.</param>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        public static IRegistrationBuilder<TLimit, TActivatorData, TSingleRegistrationStyle>
            PreserveExistingDefaults<TLimit, TActivatorData, TSingleRegistrationStyle>(
                this IRegistrationBuilder<TLimit, TActivatorData, TSingleRegistrationStyle> registration)
            where TSingleRegistrationStyle : SingleRegistrationStyle
        {
            if (registration == null) throw new ArgumentNullException(nameof(registration));

            registration.RegistrationStyle.PreserveDefaults = true;
            return registration;
        }

        /// <summary>
        /// Specifies that the components being registered should only be made the default for services
        /// that have not already been registered.
        /// </summary>
        /// <typeparam name="TLimit">Registration limit type.</typeparam>
        /// <typeparam name="TRegistrationStyle">Registration style.</typeparam>
        /// <param name="registration">Registration to set service mapping on.</param>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        public static IRegistrationBuilder<TLimit, ScanningActivatorData, TRegistrationStyle>
            PreserveExistingDefaults<TLimit, TRegistrationStyle>(this
            IRegistrationBuilder<TLimit, ScanningActivatorData, TRegistrationStyle> registration)
        {
            return ScanningRegistrationExtensions.PreserveExistingDefaults(registration);
        }

        /// <summary>
        /// Register all types in an assembly.
        /// </summary>
        /// <param name="builder">Container builder.</param>
        /// <param name="assemblies">The assemblies from which to register types.</param>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        public static IRegistrationBuilder<object, ScanningActivatorData, DynamicRegistrationStyle>
            RegisterAssemblyTypes(this ContainerBuilder builder, params Assembly[] assemblies)
        {
            return ScanningRegistrationExtensions.RegisterAssemblyTypes(builder, assemblies);
        }

        /// <summary>
        /// Register the types in a list.
        /// </summary>
        /// <param name="builder">Container builder.</param>
        /// <param name="types">The types to register.</param>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        public static IRegistrationBuilder<object, ScanningActivatorData, DynamicRegistrationStyle>
            RegisterTypes(this ContainerBuilder builder, params Type[] types)
        {
            return ScanningRegistrationExtensions.RegisterTypes(builder, types);
        }

        /// <summary>
        /// Specifies a subset of types to register from a scanned assembly.
        /// </summary>
        /// <typeparam name="TLimit">Registration limit type.</typeparam>
        /// <typeparam name="TScanningActivatorData">Activator data type.</typeparam>
        /// <typeparam name="TRegistrationStyle">Registration style.</typeparam>
        /// <param name="registration">Registration to filter types from.</param>
        /// <param name="predicate">Predicate that returns true for types to register.</param>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        public static IRegistrationBuilder<TLimit, TScanningActivatorData, TRegistrationStyle>
            Where<TLimit, TScanningActivatorData, TRegistrationStyle>(
                this IRegistrationBuilder<TLimit, TScanningActivatorData, TRegistrationStyle> registration,
                Func<Type, bool> predicate)
            where TScanningActivatorData : ScanningActivatorData
        {
            if (registration == null) throw new ArgumentNullException(nameof(registration));

            registration.ActivatorData.Filters.Add(predicate);
            return registration;
        }

        /// <summary>
        /// Specifies how a type from a scanned assembly is mapped to a service.
        /// </summary>
        /// <typeparam name="TLimit">Registration limit type.</typeparam>
        /// <typeparam name="TScanningActivatorData">Activator data type.</typeparam>
        /// <typeparam name="TRegistrationStyle">Registration style.</typeparam>
        /// <param name="registration">Registration to set service mapping on.</param>
        /// <param name="serviceMapping">Function mapping types to services.</param>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        public static IRegistrationBuilder<TLimit, TScanningActivatorData, TRegistrationStyle>
            As<TLimit, TScanningActivatorData, TRegistrationStyle>(
                this IRegistrationBuilder<TLimit, TScanningActivatorData, TRegistrationStyle> registration,
                Func<Type, IEnumerable<Service>> serviceMapping)
            where TScanningActivatorData : ScanningActivatorData
        {
            if (registration == null) throw new ArgumentNullException(nameof(registration));
            if (serviceMapping == null) throw new ArgumentNullException(nameof(serviceMapping));

            return ScanningRegistrationExtensions.As(registration, serviceMapping);
        }

        /// <summary>
        /// Specifies how a type from a scanned assembly is mapped to a service.
        /// </summary>
        /// <typeparam name="TLimit">Registration limit type.</typeparam>
        /// <typeparam name="TScanningActivatorData">Activator data type.</typeparam>
        /// <typeparam name="TRegistrationStyle">Registration style.</typeparam>
        /// <param name="registration">Registration to set service mapping on.</param>
        /// <param name="serviceMapping">Function mapping types to services.</param>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        public static IRegistrationBuilder<TLimit, TScanningActivatorData, TRegistrationStyle>
            As<TLimit, TScanningActivatorData, TRegistrationStyle>(
                this IRegistrationBuilder<TLimit, TScanningActivatorData, TRegistrationStyle> registration,
                Func<Type, Service> serviceMapping)
            where TScanningActivatorData : ScanningActivatorData
        {
            if (registration == null) throw new ArgumentNullException(nameof(registration));
            return registration.As(t => new[] { serviceMapping(t) });
        }

        /// <summary>
        /// Specifies how a type from a scanned assembly is mapped to a service.
        /// </summary>
        /// <typeparam name="TLimit">Registration limit type.</typeparam>
        /// <typeparam name="TScanningActivatorData">Activator data type.</typeparam>
        /// <typeparam name="TRegistrationStyle">Registration style.</typeparam>
        /// <param name="registration">Registration to set service mapping on.</param>
        /// <param name="serviceMapping">Function mapping types to services.</param>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        public static IRegistrationBuilder<TLimit, TScanningActivatorData, TRegistrationStyle>
            As<TLimit, TScanningActivatorData, TRegistrationStyle>(
                this IRegistrationBuilder<TLimit, TScanningActivatorData, TRegistrationStyle> registration,
                Func<Type, Type> serviceMapping)
            where TScanningActivatorData : ScanningActivatorData
        {
            if (registration == null) throw new ArgumentNullException(nameof(registration));

            return registration.As(t => (Service)new TypedService(serviceMapping(t)));
        }

        /// <summary>
        /// Specifies how a type from a scanned assembly is mapped to a service.
        /// </summary>
        /// <typeparam name="TLimit">Registration limit type.</typeparam>
        /// <typeparam name="TScanningActivatorData">Activator data type.</typeparam>
        /// <typeparam name="TRegistrationStyle">Registration style.</typeparam>
        /// <param name="registration">Registration to set service mapping on.</param>
        /// <param name="serviceMapping">Function mapping types to services.</param>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        public static IRegistrationBuilder<TLimit, TScanningActivatorData, TRegistrationStyle>
            As<TLimit, TScanningActivatorData, TRegistrationStyle>(
                this IRegistrationBuilder<TLimit, TScanningActivatorData, TRegistrationStyle> registration,
                Func<Type, IEnumerable<Type>> serviceMapping)
            where TScanningActivatorData : ScanningActivatorData
        {
            if (registration == null) throw new ArgumentNullException(nameof(registration));

            return registration.As(t => serviceMapping(t).Select(s => (Service)new TypedService(s)));
        }

        /// <summary>
        /// Specifies that a type from a scanned assembly provides its own concrete type as a service.
        /// </summary>
        /// <typeparam name="TLimit">Registration limit type.</typeparam>
        /// <param name="registration">Registration to set service mapping on.</param>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        public static IRegistrationBuilder<TLimit, ScanningActivatorData, DynamicRegistrationStyle>
            AsSelf<TLimit>(this IRegistrationBuilder<TLimit, ScanningActivatorData, DynamicRegistrationStyle> registration)
        {
            if (registration == null) throw new ArgumentNullException(nameof(registration));

            return registration.As(t => t);
        }

        /// <summary>
        /// Specifies that a type provides its own concrete type as a service.
        /// </summary>
        /// <typeparam name="TLimit">Registration limit type.</typeparam>
        /// <typeparam name="TConcreteActivatorData">Activator data type.</typeparam>
        /// <param name="registration">Registration to set service mapping on.</param>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        public static IRegistrationBuilder<TLimit, TConcreteActivatorData, SingleRegistrationStyle>
            AsSelf<TLimit, TConcreteActivatorData>(this IRegistrationBuilder<TLimit, TConcreteActivatorData, SingleRegistrationStyle> registration)
            where TConcreteActivatorData : IConcreteActivatorData
        {
            if (registration == null) throw new ArgumentNullException(nameof(registration));

            return registration.As(registration.ActivatorData.Activator.LimitType);
        }

        /// <summary>
        /// Specifies that a type provides its own concrete type as a service.
        /// </summary>
        /// <typeparam name="TLimit">Registration limit type.</typeparam>
        /// <param name="registration">Registration to set service mapping on.</param>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        public static IRegistrationBuilder<TLimit, ReflectionActivatorData, DynamicRegistrationStyle>
            AsSelf<TLimit>(this IRegistrationBuilder<TLimit, ReflectionActivatorData, DynamicRegistrationStyle> registration)
        {
            if (registration == null) throw new ArgumentNullException(nameof(registration));

            return registration.As(registration.ActivatorData.ImplementationType);
        }

        /// <summary>
        /// Specify how a type from a scanned assembly provides metadata.
        /// </summary>
        /// <typeparam name="TLimit">Registration limit type.</typeparam>
        /// <typeparam name="TScanningActivatorData">Activator data type.</typeparam>
        /// <typeparam name="TRegistrationStyle">Registration style.</typeparam>
        /// <param name="registration">Registration to set metadata on.</param>
        /// <param name="metadataMapping">A function mapping the type to a list of metadata items.</param>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        public static IRegistrationBuilder<TLimit, TScanningActivatorData, TRegistrationStyle>
            WithMetadata<TLimit, TScanningActivatorData, TRegistrationStyle>(
                this IRegistrationBuilder<TLimit, TScanningActivatorData, TRegistrationStyle> registration,
                Func<Type, IEnumerable<KeyValuePair<string, object>>> metadataMapping)
            where TScanningActivatorData : ScanningActivatorData
        {
            if (registration == null) throw new ArgumentNullException(nameof(registration));

            registration.ActivatorData.ConfigurationActions.Add((t, rb) => rb.WithMetadata(metadataMapping(t)));
            return registration;
        }

        /// <summary>
        /// Use the properties of an attribute (or interface implemented by an attribute) on the scanned type
        /// to provide metadata values.
        /// </summary>
        /// <remarks>Inherited attributes are supported; however, there must be at most one matching attribute
        /// in the inheritance chain.</remarks>
        /// <typeparam name="TAttribute">The attribute applied to the scanned type.</typeparam>
        /// <param name="registration">Registration to set metadata on.</param>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        public static IRegistrationBuilder<object, ScanningActivatorData, DynamicRegistrationStyle>
            WithMetadataFrom<TAttribute>(
                this IRegistrationBuilder<object, ScanningActivatorData, DynamicRegistrationStyle> registration)
        {
            var attrType = typeof(TAttribute);
            var metadataProperties = attrType
                .GetRuntimeProperties()
                .Where(pi => pi.CanRead);

            return registration.WithMetadata(t =>
            {
                var attrs = t.GetTypeInfo().GetCustomAttributes(true).OfType<TAttribute>().ToArray();

                if (attrs.Length == 0)
                    throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, RegistrationExtensionsResources.MetadataAttributeNotFound, typeof(TAttribute), t));
                if (attrs.Length != 1)
                    throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, RegistrationExtensionsResources.MultipleMetadataAttributesSameType, typeof(TAttribute), t));
                var attr = attrs[0];
                return metadataProperties.Select(p => new KeyValuePair<string, object>(p.Name, p.GetValue(attr, null)));
            });
        }

        /// <summary>
        /// Specify how a type from a scanned assembly provides metadata.
        /// </summary>
        /// <typeparam name="TLimit">Registration limit type.</typeparam>
        /// <typeparam name="TScanningActivatorData">Activator data type.</typeparam>
        /// <typeparam name="TRegistrationStyle">Registration style.</typeparam>
        /// <param name="registration">Registration to set service mapping on.</param>
        /// <param name="metadataKey">Key of the metadata item.</param>
        /// <param name="metadataValueMapping">A function retrieving the value of the item from the component type.</param>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        public static IRegistrationBuilder<TLimit, TScanningActivatorData, TRegistrationStyle>
            WithMetadata<TLimit, TScanningActivatorData, TRegistrationStyle>(
                this IRegistrationBuilder<TLimit, TScanningActivatorData, TRegistrationStyle> registration,
                string metadataKey,
                Func<Type, object> metadataValueMapping)
            where TScanningActivatorData : ScanningActivatorData
        {
            if (registration == null) throw new ArgumentNullException(nameof(registration));

            return registration.WithMetadata(t =>
                new[] { new KeyValuePair<string, object>(metadataKey, metadataValueMapping(t)) });
        }

        /// <summary>
        /// Specifies how a type from a scanned assembly is mapped to a named service.
        /// </summary>
        /// <param name="registration">Registration to set service mapping on.</param>
        /// <typeparam name="TService">Service type provided by the component.</typeparam>
        /// <param name="serviceNameMapping">Function mapping types to service names.</param>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        public static IRegistrationBuilder<object, ScanningActivatorData, DynamicRegistrationStyle>
            Named<TService>(
                this IRegistrationBuilder<object, ScanningActivatorData, DynamicRegistrationStyle> registration,
                Func<Type, string> serviceNameMapping)
        {
            return registration.Named(serviceNameMapping, typeof(TService));
        }

        /// <summary>
        /// Specifies how a type from a scanned assembly is mapped to a named service.
        /// </summary>
        /// <typeparam name="TLimit">Registration limit type.</typeparam>
        /// <typeparam name="TScanningActivatorData">Activator data type.</typeparam>
        /// <typeparam name="TRegistrationStyle">Registration style.</typeparam>
        /// <param name="registration">Registration to set service mapping on.</param>
        /// <param name="serviceType">Service type provided by the component.</param>
        /// <param name="serviceNameMapping">Function mapping types to service names.</param>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        public static IRegistrationBuilder<TLimit, TScanningActivatorData, TRegistrationStyle>
            Named<TLimit, TScanningActivatorData, TRegistrationStyle>(
                this IRegistrationBuilder<TLimit, TScanningActivatorData, TRegistrationStyle> registration,
                Func<Type, string> serviceNameMapping,
                Type serviceType)
            where TScanningActivatorData : ScanningActivatorData
        {
            if (registration == null) throw new ArgumentNullException(nameof(registration));
            if (serviceNameMapping == null) throw new ArgumentNullException(nameof(serviceNameMapping));
            if (serviceType == null) throw new ArgumentNullException(nameof(serviceType));

            return registration.As(t => new KeyedService(serviceNameMapping(t), serviceType));
        }

        /// <summary>
        /// Specifies how a type from a scanned assembly is mapped to a keyed service.
        /// </summary>
        /// <param name="registration">Registration to set service mapping on.</param>
        /// <typeparam name="TService">Service type provided by the component.</typeparam>
        /// <param name="serviceKeyMapping">Function mapping types to service keys.</param>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        public static IRegistrationBuilder<object, ScanningActivatorData, DynamicRegistrationStyle>
            Keyed<TService>(
                this IRegistrationBuilder<object, ScanningActivatorData, DynamicRegistrationStyle> registration,
                Func<Type, object> serviceKeyMapping)
        {
            return Keyed(registration, serviceKeyMapping, typeof(TService));
        }

        /// <summary>
        /// Specifies how a type from a scanned assembly is mapped to a keyed service.
        /// </summary>
        /// <typeparam name="TLimit">Registration limit type.</typeparam>
        /// <typeparam name="TScanningActivatorData">Activator data type.</typeparam>
        /// <typeparam name="TRegistrationStyle">Registration style.</typeparam>
        /// <param name="registration">Registration to set service mapping on.</param>
        /// <param name="serviceType">Service type provided by the component.</param>
        /// <param name="serviceKeyMapping">Function mapping types to service keys.</param>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        public static IRegistrationBuilder<TLimit, TScanningActivatorData, TRegistrationStyle>
            Keyed<TLimit, TScanningActivatorData, TRegistrationStyle>(
                this IRegistrationBuilder<TLimit, TScanningActivatorData, TRegistrationStyle> registration,
                Func<Type, object> serviceKeyMapping,
                Type serviceType)
            where TScanningActivatorData : ScanningActivatorData
        {
            if (registration == null) throw new ArgumentNullException(nameof(registration));
            if (serviceKeyMapping == null) throw new ArgumentNullException(nameof(serviceKeyMapping));
            if (serviceType == null) throw new ArgumentNullException(nameof(serviceType));

            return registration
                .AssignableTo(serviceType)
                .As(t => new KeyedService(serviceKeyMapping(t), serviceType));
        }

        /// <summary>
        /// Specifies that a type from a scanned assembly is registered as providing all of its
        /// implemented interfaces.
        /// </summary>
        /// <typeparam name="TLimit">Registration limit type.</typeparam>
        /// <param name="registration">Registration to set service mapping on.</param>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        public static IRegistrationBuilder<TLimit, ScanningActivatorData, DynamicRegistrationStyle>
            AsImplementedInterfaces<TLimit>(this IRegistrationBuilder<TLimit, ScanningActivatorData, DynamicRegistrationStyle> registration)
        {
            if (registration == null) throw new ArgumentNullException(nameof(registration));

            return registration.As(t => GetImplementedInterfaces(t));
        }

        /// <summary>
        /// Specifies that a type is registered as providing all of its implemented interfaces.
        /// </summary>
        /// <typeparam name="TLimit">Registration limit type.</typeparam>
        /// <typeparam name="TConcreteActivatorData">Activator data type.</typeparam>
        /// <param name="registration">Registration to set service mapping on.</param>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        public static IRegistrationBuilder<TLimit, TConcreteActivatorData, SingleRegistrationStyle>
            AsImplementedInterfaces<TLimit, TConcreteActivatorData>(this IRegistrationBuilder<TLimit, TConcreteActivatorData, SingleRegistrationStyle> registration)
            where TConcreteActivatorData : IConcreteActivatorData
        {
            if (registration == null) throw new ArgumentNullException(nameof(registration));

            return registration.As(GetImplementedInterfaces(registration.ActivatorData.Activator.LimitType));
        }

        /// <summary>
        /// Specifies that a type is registered as providing all of its implemented interfaces.
        /// </summary>
        /// <typeparam name="TLimit">Registration limit type.</typeparam>
        /// <param name="registration">Registration to set service mapping on.</param>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        public static IRegistrationBuilder<TLimit, ReflectionActivatorData, DynamicRegistrationStyle>
            AsImplementedInterfaces<TLimit>(this IRegistrationBuilder<TLimit, ReflectionActivatorData, DynamicRegistrationStyle> registration)
        {
            if (registration == null) throw new ArgumentNullException(nameof(registration));

            var implementationType = registration.ActivatorData.ImplementationType;
            return registration.As(GetImplementedInterfaces(implementationType));
        }

        private static Type[] GetImplementedInterfaces(Type type)
        {
            var interfaces = type.GetTypeInfo().ImplementedInterfaces.Where(i => i != typeof(IDisposable));
            return type.GetTypeInfo().IsInterface ? interfaces.AppendItem(type).ToArray() : interfaces.ToArray();
        }

        /// <summary>
        /// Set the policy used to find candidate constructors on the implementation type.
        /// </summary>
        /// <typeparam name="TLimit">Registration limit type.</typeparam>
        /// <typeparam name="TReflectionActivatorData">Activator data type.</typeparam>
        /// <typeparam name="TStyle">Registration style.</typeparam>
        /// <param name="registration">Registration to set policy on.</param>
        /// <param name="constructorFinder">Policy to be used when searching for constructors.</param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        public static IRegistrationBuilder<TLimit, TReflectionActivatorData, TStyle>
            FindConstructorsWith<TLimit, TReflectionActivatorData, TStyle>(
                this IRegistrationBuilder<TLimit, TReflectionActivatorData, TStyle> registration,
                IConstructorFinder constructorFinder)
            where TReflectionActivatorData : ReflectionActivatorData
        {
            if (registration == null) throw new ArgumentNullException(nameof(registration));
            if (constructorFinder == null) throw new ArgumentNullException(nameof(constructorFinder));

            registration.ActivatorData.ConstructorFinder = constructorFinder;
            return registration;
        }

        /// <summary>
        /// Set the policy used to find candidate constructors on the implementation type.
        /// </summary>
        /// <typeparam name="TLimit">Registration limit type.</typeparam>
        /// <typeparam name="TReflectionActivatorData">Activator data type.</typeparam>
        /// <typeparam name="TStyle">Registration style.</typeparam>
        /// <param name="registration">Registration to set policy on.</param>
        /// <param name="finder">A function that returns the constructors to select from.</param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        public static IRegistrationBuilder<TLimit, TReflectionActivatorData, TStyle>
            FindConstructorsWith<TLimit, TReflectionActivatorData, TStyle>(
                this IRegistrationBuilder<TLimit, TReflectionActivatorData, TStyle> registration,
                Func<Type, ConstructorInfo[]> finder)
            where TReflectionActivatorData : ReflectionActivatorData
        {
            if (registration == null) throw new ArgumentNullException(nameof(registration));

            return registration.FindConstructorsWith(new DefaultConstructorFinder(finder));
        }

        /// <summary>
        /// Configure the component so that any properties whose types are registered in the
        /// container will be wired to instances of the appropriate service.
        /// </summary>
        /// <param name="registration">Registration to auto-wire properties.</param>
        /// <param name="wiringFlags">Set wiring options such as circular dependency wiring support.</param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        public static IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle>
            PropertiesAutowired<TLimit, TActivatorData, TRegistrationStyle>(
                this IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> registration, PropertyWiringOptions wiringFlags = PropertyWiringOptions.None)
        {
            var preserveSetValues = (int)(wiringFlags & PropertyWiringOptions.PreserveSetValues) != 0;
            var allowCircularDependencies = (int)(wiringFlags & PropertyWiringOptions.AllowCircularDependencies) != 0;

            return registration.PropertiesAutowired(new DefaultPropertySelector(preserveSetValues), allowCircularDependencies);
        }

        /// <summary>
        /// Set the policy used to find candidate properties on the implementation type.
        /// </summary>
        /// <typeparam name="TLimit">Registration limit type.</typeparam>
        /// <typeparam name="TActivatorData">Activator data type.</typeparam>
        /// <typeparam name="TStyle">Registration style.</typeparam>
        /// <param name="registration">Registration to set policy on.</param>
        /// <param name="propertySelector">Policy to be used when searching for properties to inject.</param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        public static IRegistrationBuilder<TLimit, TActivatorData, TStyle> PropertiesAutowired<TLimit, TActivatorData, TStyle>(
            this IRegistrationBuilder<TLimit, TActivatorData, TStyle> registration,
            Func<PropertyInfo, object, bool> propertySelector)
        {
            if (registration == null) throw new ArgumentNullException(nameof(registration));

            return registration.PropertiesAutowired(new DelegatePropertySelector(propertySelector));
        }

        /// <summary>
        /// Set the policy used to select from available constructors on the implementation type.
        /// </summary>
        /// <typeparam name="TLimit">Registration limit type.</typeparam>
        /// <typeparam name="TReflectionActivatorData">Activator data type.</typeparam>
        /// <typeparam name="TStyle">Registration style.</typeparam>
        /// <param name="registration">Registration to set policy on.</param>
        /// <param name="signature">Constructor signature to match.</param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        public static IRegistrationBuilder<TLimit, TReflectionActivatorData, TStyle>
            UsingConstructor<TLimit, TReflectionActivatorData, TStyle>(
                this IRegistrationBuilder<TLimit, TReflectionActivatorData, TStyle> registration,
                params Type[] signature)
            where TReflectionActivatorData : ReflectionActivatorData
        {
            if (registration == null) throw new ArgumentNullException(nameof(registration));
            if (signature == null) throw new ArgumentNullException(nameof(signature));

            // Unfortunately this could cause some false positives in rare AOP/dynamic subclassing
            // scenarios. If it becomes a problem we'll address it then.
            if (registration.ActivatorData.ImplementationType.GetMatchingConstructor(signature) == null)
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, RegistrationExtensionsResources.NoMatchingConstructorExists, registration.ActivatorData.ImplementationType));

            return registration.UsingConstructor(new MatchingSignatureConstructorSelector(signature));
        }

        /// <summary>
        /// Set the policy used to select from available constructors on the implementation type.
        /// </summary>
        /// <typeparam name="TLimit">Registration limit type.</typeparam>
        /// <typeparam name="TReflectionActivatorData">Activator data type.</typeparam>
        /// <typeparam name="TStyle">Registration style.</typeparam>
        /// <param name="registration">Registration to set policy on.</param>
        /// <param name="constructorSelector">Policy to be used when selecting a constructor.</param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        public static IRegistrationBuilder<TLimit, TReflectionActivatorData, TStyle>
            UsingConstructor<TLimit, TReflectionActivatorData, TStyle>(
                this IRegistrationBuilder<TLimit, TReflectionActivatorData, TStyle> registration,
                IConstructorSelector constructorSelector)
            where TReflectionActivatorData : ReflectionActivatorData
        {
            if (registration == null) throw new ArgumentNullException(nameof(registration));
            if (constructorSelector == null) throw new ArgumentNullException(nameof(constructorSelector));

            registration.ActivatorData.ConstructorSelector = constructorSelector;
            return registration;
        }

        /// <summary>
        /// Set the policy used to select from available constructors on the implementation type.
        /// </summary>
        /// <typeparam name="TLimit">Registration limit type.</typeparam>
        /// <typeparam name="TReflectionActivatorData">Activator data type.</typeparam>
        /// <typeparam name="TStyle">Registration style.</typeparam>
        /// <param name="registration">Registration to set policy on.</param>
        /// <param name="constructorSelector">Expression demonstrating how the constructor is called.</param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        public static IRegistrationBuilder<TLimit, TReflectionActivatorData, TStyle>
            UsingConstructor<TLimit, TReflectionActivatorData, TStyle>(
                this IRegistrationBuilder<TLimit, TReflectionActivatorData, TStyle> registration,
                Expression<Func<TLimit>> constructorSelector)
            where TReflectionActivatorData : ReflectionActivatorData
        {
            if (registration == null) throw new ArgumentNullException(nameof(registration));
            if (constructorSelector == null) throw new ArgumentNullException(nameof(constructorSelector));

            var constructor = ReflectionExtensions.GetConstructor(constructorSelector);
            var parameterTypes = constructor.GetParameters().Select(p => p.ParameterType).ToArray();
            return UsingConstructor(registration, parameterTypes);
        }

        /// <summary>
        /// Configure an explicit value for a constructor parameter.
        /// </summary>
        /// <typeparam name="TLimit">Registration limit type.</typeparam>
        /// <typeparam name="TReflectionActivatorData">Activator data type.</typeparam>
        /// <typeparam name="TStyle">Registration style.</typeparam>
        /// <param name="registration">Registration to set parameter on.</param>
        /// <param name="parameterName">Name of a constructor parameter on the target type.</param>
        /// <param name="parameterValue">Value to supply to the parameter.</param>0
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        public static IRegistrationBuilder<TLimit, TReflectionActivatorData, TStyle>
            WithParameter<TLimit, TReflectionActivatorData, TStyle>(
                this IRegistrationBuilder<TLimit, TReflectionActivatorData, TStyle> registration,
                string parameterName,
                object parameterValue)
            where TReflectionActivatorData : ReflectionActivatorData
        {
            return registration.WithParameter(new NamedParameter(parameterName, parameterValue));
        }

        /// <summary>
        /// Configure an explicit value for a constructor parameter.
        /// </summary>
        /// <typeparam name="TLimit">Registration limit type.</typeparam>
        /// <typeparam name="TReflectionActivatorData">Activator data type.</typeparam>
        /// <typeparam name="TStyle">Registration style.</typeparam>
        /// <param name="registration">Registration to set parameter on.</param>
        /// <param name="parameter">The parameter to supply to the constructor.</param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        public static IRegistrationBuilder<TLimit, TReflectionActivatorData, TStyle>
            WithParameter<TLimit, TReflectionActivatorData, TStyle>(
                this IRegistrationBuilder<TLimit, TReflectionActivatorData, TStyle> registration,
                Parameter parameter)
            where TReflectionActivatorData : ReflectionActivatorData
        {
            if (registration == null) throw new ArgumentNullException(nameof(registration));
            if (parameter == null) throw new ArgumentNullException(nameof(parameter));

            registration.ActivatorData.ConfiguredParameters.Add(parameter);
            return registration;
        }

        /// <summary>
        /// Configure an explicit value for a constructor parameter.
        /// </summary>
        /// <typeparam name="TLimit">Registration limit type.</typeparam>
        /// <typeparam name="TReflectionActivatorData">Activator data type.</typeparam>
        /// <typeparam name="TStyle">Registration style.</typeparam>
        /// <param name="registration">Registration to set parameter on.</param>
        /// <param name="parameterSelector">A predicate selecting the parameter to set.</param>
        /// <param name="valueProvider">The provider that will generate the parameter value.</param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        public static IRegistrationBuilder<TLimit, TReflectionActivatorData, TStyle>
            WithParameter<TLimit, TReflectionActivatorData, TStyle>(
                this IRegistrationBuilder<TLimit, TReflectionActivatorData, TStyle> registration,
                Func<ParameterInfo, IComponentContext, bool> parameterSelector,
                Func<ParameterInfo, IComponentContext, object> valueProvider)
            where TReflectionActivatorData : ReflectionActivatorData
        {
            if (parameterSelector == null) throw new ArgumentNullException(nameof(parameterSelector));
            if (valueProvider == null) throw new ArgumentNullException(nameof(valueProvider));

            return registration.WithParameter(
                new ResolvedParameter(parameterSelector, valueProvider));
        }

        /// <summary>
        /// Configure explicit values for constructor parameters.
        /// </summary>
        /// <typeparam name="TLimit">Registration limit type.</typeparam>
        /// <typeparam name="TReflectionActivatorData">Activator data type.</typeparam>
        /// <typeparam name="TStyle">Registration style.</typeparam>
        /// <param name="registration">Registration to set parameter on.</param>
        /// <param name="parameters">The parameters to supply to the constructor.</param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        public static IRegistrationBuilder<TLimit, TReflectionActivatorData, TStyle>
            WithParameters<TLimit, TReflectionActivatorData, TStyle>(
                this IRegistrationBuilder<TLimit, TReflectionActivatorData, TStyle> registration,
                IEnumerable<Parameter> parameters)
            where TReflectionActivatorData : ReflectionActivatorData
        {
            if (parameters == null) throw new ArgumentNullException(nameof(parameters));

            foreach (var param in parameters)
                registration.WithParameter(param);

            return registration;
        }

        /// <summary>
        /// Configure an explicit value for a property.
        /// </summary>
        /// <typeparam name="TLimit">Registration limit type.</typeparam>
        /// <typeparam name="TReflectionActivatorData">Activator data type.</typeparam>
        /// <typeparam name="TStyle">Registration style.</typeparam>
        /// <param name="registration">Registration to set property on.</param>
        /// <param name="propertyName">Name of a property on the target type.</param>
        /// <param name="propertyValue">Value to supply to the property.</param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        public static IRegistrationBuilder<TLimit, TReflectionActivatorData, TStyle>
            WithProperty<TLimit, TReflectionActivatorData, TStyle>(
                this IRegistrationBuilder<TLimit, TReflectionActivatorData, TStyle> registration,
                string propertyName,
                object propertyValue)
            where TReflectionActivatorData : ReflectionActivatorData
        {
            return registration.WithProperty(new NamedPropertyParameter(propertyName, propertyValue));
        }

        /// <summary>
        /// Configure an explicit value for a property.
        /// </summary>
        /// <typeparam name="TLimit">Registration limit type.</typeparam>
        /// <typeparam name="TReflectionActivatorData">Activator data type.</typeparam>
        /// <typeparam name="TStyle">Registration style.</typeparam>
        /// <param name="registration">Registration to set parameter on.</param>
        /// <param name="property">The property to supply.</param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        public static IRegistrationBuilder<TLimit, TReflectionActivatorData, TStyle>
            WithProperty<TLimit, TReflectionActivatorData, TStyle>(
                this IRegistrationBuilder<TLimit, TReflectionActivatorData, TStyle> registration,
                Parameter property)
            where TReflectionActivatorData : ReflectionActivatorData
        {
            if (registration == null) throw new ArgumentNullException(nameof(registration));
            if (property == null) throw new ArgumentNullException(nameof(property));

            registration.ActivatorData.ConfiguredProperties.Add(property);
            return registration;
        }

        /// <summary>
        /// Configure explicit values for properties.
        /// </summary>
        /// <typeparam name="TLimit">Registration limit type.</typeparam>
        /// <typeparam name="TReflectionActivatorData">Activator data type.</typeparam>
        /// <typeparam name="TStyle">Registration style.</typeparam>
        /// <param name="registration">Registration to set parameter on.</param>
        /// <param name="properties">The properties to supply.</param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        public static IRegistrationBuilder<TLimit, TReflectionActivatorData, TStyle>
            WithProperties<TLimit, TReflectionActivatorData, TStyle>(
                this IRegistrationBuilder<TLimit, TReflectionActivatorData, TStyle> registration,
                IEnumerable<Parameter> properties)
            where TReflectionActivatorData : ReflectionActivatorData
        {
            if (registration == null) throw new ArgumentNullException(nameof(registration));
            if (properties == null) throw new ArgumentNullException(nameof(properties));

            foreach (var prop in properties)
                registration.WithProperty(prop);

            return registration;
        }

        /// <summary>
        /// Sets the target of the registration (used for metadata generation).
        /// </summary>
        /// <typeparam name="TLimit">The type of the limit.</typeparam>
        /// <typeparam name="TActivatorData">The type of the activator data.</typeparam>
        /// <typeparam name="TSingleRegistrationStyle">Registration style.</typeparam>
        /// <param name="registration">Registration to set target for.</param>
        /// <param name="target">The target.</param>
        /// <returns>
        /// Registration builder allowing the registration to be configured.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="registration" /> or <paramref name="target" /> is <see langword="null" />.
        /// </exception>
        public static IRegistrationBuilder<TLimit, TActivatorData, TSingleRegistrationStyle>
            Targeting<TLimit, TActivatorData, TSingleRegistrationStyle>(
                this IRegistrationBuilder<TLimit, TActivatorData, TSingleRegistrationStyle> registration,
                IComponentRegistration target)
            where TSingleRegistrationStyle : SingleRegistrationStyle
        {
            if (registration == null) throw new ArgumentNullException(nameof(registration));
            if (target == null) throw new ArgumentNullException(nameof(target));

            registration.RegistrationStyle.Target = target.Target;
            return registration;
        }

        /// <summary>
        /// Provide a handler to be called when the component is registered.
        /// </summary>
        /// <typeparam name="TLimit">Registration limit type.</typeparam>
        /// <typeparam name="TActivatorData">Activator data type.</typeparam>
        /// <typeparam name="TSingleRegistrationStyle">Registration style.</typeparam>
        /// <param name="registration">Registration add handler to.</param>
        /// <param name="handler">The handler.</param>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        public static IRegistrationBuilder<TLimit, TActivatorData, TSingleRegistrationStyle>
            OnRegistered<TLimit, TActivatorData, TSingleRegistrationStyle>(
                this IRegistrationBuilder<TLimit, TActivatorData, TSingleRegistrationStyle> registration,
                Action<ComponentRegisteredEventArgs> handler)
            where TSingleRegistrationStyle : SingleRegistrationStyle
        {
            if (registration == null) throw new ArgumentNullException(nameof(registration));
            if (handler == null) throw new ArgumentNullException(nameof(handler));

            registration.RegistrationStyle.RegisteredHandlers.Add((s, e) => handler(e));

            return registration;
        }

        /// <summary>
        /// Provide a handler to be called when the component is registred.
        /// </summary>
        /// <typeparam name="TLimit">Registration limit type.</typeparam>
        /// <typeparam name="TRegistrationStyle">Registration style.</typeparam>
        /// <param name="registration">Registration add handler to.</param>
        /// <param name="handler">The handler.</param>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        public static IRegistrationBuilder<TLimit, ScanningActivatorData, TRegistrationStyle>
            OnRegistered<TLimit, TRegistrationStyle>(
                this IRegistrationBuilder<TLimit, ScanningActivatorData, TRegistrationStyle> registration,
                Action<ComponentRegisteredEventArgs> handler)
        {
            if (registration == null) throw new ArgumentNullException(nameof(registration));
            if (handler == null) throw new ArgumentNullException(nameof(handler));

            registration.ActivatorData.ConfigurationActions.Add((t, rb) => rb.OnRegistered(handler));

            return registration;
        }

        /// <summary>
        /// Specifies that a type from a scanned assembly is registered if it implements an interface
        /// that closes the provided open generic interface type.
        /// </summary>
        /// <typeparam name="TLimit">Registration limit type.</typeparam>
        /// <typeparam name="TScanningActivatorData">Activator data type.</typeparam>
        /// <typeparam name="TRegistrationStyle">Registration style.</typeparam>
        /// <param name="registration">Registration to set service mapping on.</param>
        /// <param name="openGenericServiceType">The open generic interface or base class type for which implementations will be found.</param>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        public static IRegistrationBuilder<TLimit, TScanningActivatorData, TRegistrationStyle>
            AsClosedTypesOf<TLimit, TScanningActivatorData, TRegistrationStyle>(
                this IRegistrationBuilder<TLimit, TScanningActivatorData, TRegistrationStyle> registration, Type openGenericServiceType)
            where TScanningActivatorData : ScanningActivatorData
        {
            return ScanningRegistrationExtensions.AsClosedTypesOf(registration, openGenericServiceType);
        }

        /// <summary>
        /// Specifies that a type from a scanned assembly is registered if it implements an interface
        /// that closes the provided open generic interface type.
        /// </summary>
        /// <typeparam name="TLimit">Registration limit type.</typeparam>
        /// <typeparam name="TScanningActivatorData">Activator data type.</typeparam>
        /// <typeparam name="TRegistrationStyle">Registration style.</typeparam>
        /// <param name="registration">Registration to set service mapping on.</param>
        /// <param name="openGenericServiceType">The open generic interface or base class type for which implementations will be found.</param>
        /// <param name="serviceKey">Key of the service.</param>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        public static IRegistrationBuilder<TLimit, TScanningActivatorData, TRegistrationStyle>
            AsClosedTypesOf<TLimit, TScanningActivatorData, TRegistrationStyle>(
                this IRegistrationBuilder<TLimit, TScanningActivatorData, TRegistrationStyle> registration, Type openGenericServiceType, object serviceKey)
            where TScanningActivatorData : ScanningActivatorData
        {
            return ScanningRegistrationExtensions.AsClosedTypesOf(registration, openGenericServiceType, serviceKey);
        }

        /// <summary>
        /// Specifies that a type from a scanned assembly is registered if it implements an interface
        /// that closes the provided open generic interface type.
        /// </summary>
        /// <typeparam name="TLimit">Registration limit type.</typeparam>
        /// <typeparam name="TScanningActivatorData">Activator data type.</typeparam>
        /// <typeparam name="TRegistrationStyle">Registration style.</typeparam>
        /// <param name="registration">Registration to set service mapping on.</param>
        /// <param name="openGenericServiceType">The open generic interface or base class type for which implementations will be found.</param>
        /// <param name="serviceKeyMapping">Function mapping types to service keys.</param>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        public static IRegistrationBuilder<TLimit, TScanningActivatorData, TRegistrationStyle>
            AsClosedTypesOf<TLimit, TScanningActivatorData, TRegistrationStyle>(
                this IRegistrationBuilder<TLimit, TScanningActivatorData, TRegistrationStyle> registration, Type openGenericServiceType, Func<Type, object> serviceKeyMapping)
            where TScanningActivatorData : ScanningActivatorData
        {
            return ScanningRegistrationExtensions.AsClosedTypesOf(registration, openGenericServiceType, serviceKeyMapping);
        }

        /// <summary>
        /// Filters the scanned types to include only those assignable to the provided
        /// type.
        /// </summary>
        /// <typeparam name="TLimit">Registration limit type.</typeparam>
        /// <typeparam name="TScanningActivatorData">Activator data type.</typeparam>
        /// <typeparam name="TRegistrationStyle">Registration style.</typeparam>
        /// <param name="registration">Registration to filter types from.</param>
        /// <param name="type">The type or interface which all classes must be assignable from.</param>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        public static IRegistrationBuilder<TLimit, TScanningActivatorData, TRegistrationStyle>
            AssignableTo<TLimit, TScanningActivatorData, TRegistrationStyle>(
                this IRegistrationBuilder<TLimit, TScanningActivatorData, TRegistrationStyle> registration,
                Type type)
            where TScanningActivatorData : ScanningActivatorData
        {
            return ScanningRegistrationExtensions.AssignableTo(registration, type);
        }

        /// <summary>
        /// Filters the scanned types to include only those assignable to the provided
        /// type.
        /// </summary>
        /// <param name="registration">Registration to filter types from.</param>
        /// <typeparam name="T">The type or interface which all classes must be assignable from.</typeparam>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        public static IRegistrationBuilder<object, ScanningActivatorData, DynamicRegistrationStyle>
            AssignableTo<T>(this IRegistrationBuilder<object, ScanningActivatorData, DynamicRegistrationStyle> registration)
        {
            return registration.AssignableTo(typeof(T));
        }

        /// <summary>
        /// Filters the scanned types to exclude the provided type.
        /// </summary>
        /// <param name="registration">Registration to filter types from.</param>
        /// <typeparam name="T">The concrete type to exclude.</typeparam>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        public static IRegistrationBuilder<object, ScanningActivatorData, DynamicRegistrationStyle>
            Except<T>(this IRegistrationBuilder<object, ScanningActivatorData, DynamicRegistrationStyle> registration)
        {
            return registration.Where(t => t != typeof(T));
        }

        /// <summary>
        /// Filters the scanned types to exclude the provided type, providing specific configuration for
        /// the excluded type.
        /// </summary>
        /// <param name="registration">Registration to filter types from.</param>
        /// <param name="customizedRegistration">Registration for the excepted type.</param>
        /// <typeparam name="T">The concrete type to exclude.</typeparam>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        public static IRegistrationBuilder<object, ScanningActivatorData, DynamicRegistrationStyle>
            Except<T>(
                this IRegistrationBuilder<object, ScanningActivatorData, DynamicRegistrationStyle> registration,
                Action<IRegistrationBuilder<T, ConcreteReflectionActivatorData, SingleRegistrationStyle>> customizedRegistration)
        {
            var result = registration.Except<T>();

            result.ActivatorData.PostScanningCallbacks.Add(cr =>
            {
                var rb = RegistrationBuilder.ForType<T>();
                customizedRegistration(rb);
                RegistrationBuilder.RegisterSingleComponent(cr, rb);
            });

            return result;
        }

        /// <summary>
        /// Filters the scanned types to include only those in the namespace of the provided type
        /// or one of its sub-namespaces.
        /// </summary>
        /// <param name="registration">Registration to filter types from.</param>
        /// <typeparam name="T">A type in the target namespace.</typeparam>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        public static IRegistrationBuilder<object, ScanningActivatorData, DynamicRegistrationStyle>
            InNamespaceOf<T>(this IRegistrationBuilder<object, ScanningActivatorData, DynamicRegistrationStyle> registration)
        {
            if (registration == null) throw new ArgumentNullException(nameof(registration));

            return registration.InNamespace(typeof(T).Namespace);
        }

        /// <summary>
        /// Filters the scanned types to include only those in the provided namespace
        /// or one of its sub-namespaces.
        /// </summary>
        /// <typeparam name="TLimit">Registration limit type.</typeparam>
        /// <typeparam name="TScanningActivatorData">Activator data type.</typeparam>
        /// <typeparam name="TRegistrationStyle">Registration style.</typeparam>
        /// <param name="registration">Registration to filter types from.</param>
        /// <param name="ns">The namespace from which types will be selected.</param>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        public static IRegistrationBuilder<TLimit, TScanningActivatorData, TRegistrationStyle>
            InNamespace<TLimit, TScanningActivatorData, TRegistrationStyle>(
                this IRegistrationBuilder<TLimit, TScanningActivatorData, TRegistrationStyle> registration,
                string ns)
            where TScanningActivatorData : ScanningActivatorData
        {
            if (registration == null) throw new ArgumentNullException(nameof(registration));
            if (ns == null) throw new ArgumentNullException(nameof(ns));

            return registration.Where(t => t.IsInNamespace(ns));
        }

        /// <summary>
        /// Adapt all components implementing service <typeparamref name="TFrom"/>
        /// to provide <typeparamref name="TTo"/> using the provided <paramref name="adapter"/>
        /// function.
        /// </summary>
        /// <typeparam name="TFrom">Service type to adapt from.</typeparam>
        /// <typeparam name="TTo">Service type to adapt to. Must not be the
        /// same as <typeparamref name="TFrom"/>.</typeparam>
        /// <param name="builder">Container builder.</param>
        /// <param name="adapter">Function adapting <typeparamref name="TFrom"/> to
        /// service <typeparamref name="TTo"/>, given the context and parameters.</param>
        public static IRegistrationBuilder<TTo, LightweightAdapterActivatorData, DynamicRegistrationStyle>
            RegisterAdapter<TFrom, TTo>(
                this ContainerBuilder builder,
                Func<IComponentContext, IEnumerable<Parameter>, TFrom, TTo> adapter)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (adapter == null) throw new ArgumentNullException(nameof(adapter));

            return LightweightAdapterRegistrationExtensions.RegisterAdapter(builder, adapter);
        }

        /// <summary>
        /// Adapt all components implementing service <typeparamref name="TFrom"/>
        /// to provide <typeparamref name="TTo"/> using the provided <paramref name="adapter"/>
        /// function.
        /// </summary>
        /// <typeparam name="TFrom">Service type to adapt from.</typeparam>
        /// <typeparam name="TTo">Service type to adapt to. Must not be the
        /// same as <typeparamref name="TFrom"/>.</typeparam>
        /// <param name="builder">Container builder.</param>
        /// <param name="adapter">Function adapting <typeparamref name="TFrom"/> to
        /// service <typeparamref name="TTo"/>, given the context.</param>
        public static IRegistrationBuilder<TTo, LightweightAdapterActivatorData, DynamicRegistrationStyle>
            RegisterAdapter<TFrom, TTo>(
                this ContainerBuilder builder,
                Func<IComponentContext, TFrom, TTo> adapter)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (adapter == null) throw new ArgumentNullException(nameof(adapter));

            return builder.RegisterAdapter<TFrom, TTo>((c, p, f) => adapter(c, f));
        }

        /// <summary>
        /// Adapt all components implementing service <typeparamref name="TFrom"/>
        /// to provide <typeparamref name="TTo"/> using the provided <paramref name="adapter"/>
        /// function.
        /// </summary>
        /// <typeparam name="TFrom">Service type to adapt from.</typeparam>
        /// <typeparam name="TTo">Service type to adapt to. Must not be the
        /// same as <typeparamref name="TFrom"/>.</typeparam>
        /// <param name="builder">Container builder.</param>
        /// <param name="adapter">Function adapting <typeparamref name="TFrom"/> to
        /// service <typeparamref name="TTo"/>.</param>
        public static IRegistrationBuilder<TTo, LightweightAdapterActivatorData, DynamicRegistrationStyle>
            RegisterAdapter<TFrom, TTo>(
                this ContainerBuilder builder,
                Func<TFrom, TTo> adapter)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (adapter == null) throw new ArgumentNullException(nameof(adapter));

            return builder.RegisterAdapter<TFrom, TTo>((c, p, f) => adapter(f));
        }

        /// <summary>
        /// Decorate all components implementing open generic service <paramref name="decoratedServiceType"/>.
        /// The <paramref name="fromKey"/> and <paramref name="toKey"/> parameters must be different values.
        /// </summary>
        /// <param name="builder">Container builder.</param>
        /// <param name="decoratedServiceType">Service type being decorated. Must be an open generic type.</param>
        /// <param name="fromKey">Service key or name associated with the components being decorated.</param>
        /// <param name="toKey">Service key or name given to the decorated components.</param>
        /// <param name="decoratorType">The type of the decorator. Must be an open generic type, and accept a parameter
        /// of type <paramref name="decoratedServiceType"/>, which will be set to the instance being decorated.</param>
        public static IRegistrationBuilder<object, OpenGenericDecoratorActivatorData, DynamicRegistrationStyle>
            RegisterGenericDecorator(
                this ContainerBuilder builder,
                Type decoratorType,
                Type decoratedServiceType,
                object fromKey,
                object toKey = null)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (decoratorType == null) throw new ArgumentNullException(nameof(decoratorType));
            if (decoratedServiceType == null) throw new ArgumentNullException(nameof(decoratedServiceType));

            return OpenGenericRegistrationExtensions.RegisterGenericDecorator(builder, decoratorType, decoratedServiceType, fromKey, toKey);
        }

        /// <summary>
        /// Decorate all components implementing service <typeparamref name="TService"/>
        /// using the provided <paramref name="decorator"/> function.
        /// The <paramref name="fromKey"/> and <paramref name="toKey"/> parameters must be different values.
        /// </summary>
        /// <typeparam name="TService">Service type being decorated.</typeparam>
        /// <param name="builder">Container builder.</param>
        /// <param name="decorator">Function decorating a component instance that provides
        /// <typeparamref name="TService"/>, given the context and parameters.</param>
        /// <param name="fromKey">Service key or name associated with the components being decorated.</param>
        /// <param name="toKey">Service key or name given to the decorated components.</param>
        public static IRegistrationBuilder<TService, LightweightAdapterActivatorData, DynamicRegistrationStyle>
            RegisterDecorator<TService>(
                this ContainerBuilder builder,
                Func<IComponentContext, IEnumerable<Parameter>, TService, TService> decorator,
                object fromKey,
                object toKey = null)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (decorator == null) throw new ArgumentNullException(nameof(decorator));

            return LightweightAdapterRegistrationExtensions.RegisterDecorator(builder, decorator, fromKey, toKey);
        }

        /// <summary>
        /// Decorate all components implementing service <typeparamref name="TService"/>
        /// using the provided <paramref name="decorator"/> function.
        /// The <paramref name="fromKey"/> and <paramref name="toKey"/> parameters must be different values.
        /// </summary>
        /// <typeparam name="TService">Service type being decorated.</typeparam>
        /// <param name="builder">Container builder.</param>
        /// <param name="decorator">Function decorating a component instance that provides
        /// <typeparamref name="TService"/>, given the context.</param>
        /// <param name="fromKey">Service key or name associated with the components being decorated.</param>
        /// <param name="toKey">Service key or name given to the decorated components.</param>
        public static IRegistrationBuilder<TService, LightweightAdapterActivatorData, DynamicRegistrationStyle>
            RegisterDecorator<TService>(
                this ContainerBuilder builder,
                Func<IComponentContext, TService, TService> decorator,
                object fromKey,
                object toKey = null)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (decorator == null) throw new ArgumentNullException(nameof(decorator));

            return LightweightAdapterRegistrationExtensions.RegisterDecorator<TService>(builder, (c, p, f) => decorator(c, f), fromKey, toKey);
        }

        /// <summary>
        /// Decorate all components implementing service <typeparamref name="TService"/>
        /// using the provided <paramref name="decorator"/> function.
        /// The <paramref name="fromKey"/> and <paramref name="toKey"/> parameters must be different values.
        /// </summary>
        /// <typeparam name="TService">Service type being decorated.</typeparam>
        /// <param name="builder">Container builder.</param>
        /// <param name="decorator">Function decorating a component instance that provides
        /// <typeparamref name="TService"/>.</param>
        /// <param name="fromKey">Service key or name associated with the components being decorated.</param>
        /// <param name="toKey">Service key or name given to the decorated components.</param>
        public static IRegistrationBuilder<TService, LightweightAdapterActivatorData, DynamicRegistrationStyle>
            RegisterDecorator<TService>(
                this ContainerBuilder builder,
                Func<TService, TService> decorator,
                object fromKey,
                object toKey = null)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (decorator == null) throw new ArgumentNullException(nameof(decorator));

            return LightweightAdapterRegistrationExtensions.RegisterDecorator<TService>(builder, (c, p, f) => decorator(f), fromKey, toKey);
        }

        /// <summary>
        /// Decorate all components implementing service <typeparamref name="TService"/>
        /// with decorator service <typeparamref name="TDecorator"/>.
        /// </summary>
        /// <typeparam name="TDecorator">Service type of the decorator. Must accept a parameter
        /// of type <typeparamref name="TService"/>, which will be set to the instance being decorated.</typeparam>
        /// <typeparam name="TService">Service type being decorated.</typeparam>
        /// <param name="builder">Container builder.</param>
        /// <param name="condition">A function that when provided with an <see cref="IDecoratorContext"/>
        /// instance determines if the decorator should be applied.</param>
        public static void RegisterDecorator<TDecorator, TService>(this ContainerBuilder builder, Func<IDecoratorContext, bool> condition = null)
            where TDecorator : TService
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            builder.RegisterType<TDecorator>().As(new DecoratorService(typeof(TService), condition));
        }

        /// <summary>
        /// Decorate all components implementing service <paramref name="serviceType"/>
        /// with decorator service <paramref name="decoratorType"/>.
        /// </summary>
        /// <param name="builder">Container builder.</param>
        /// <param name="decoratorType">Service type of the decorator. Must accept a parameter
        /// of type <paramref name="serviceType"/>, which will be set to the instance being decorated.</param>
        /// <param name="serviceType">Service type being decorated.</param>
        /// <param name="condition">A function that when provided with an <see cref="IDecoratorContext"/>
        /// instance determines if the decorator should be applied.</param>
        public static void RegisterDecorator(
            this ContainerBuilder builder,
            Type decoratorType,
            Type serviceType,
            Func<IDecoratorContext, bool> condition = null)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (decoratorType == null) throw new ArgumentNullException(nameof(decoratorType));
            if (serviceType == null) throw new ArgumentNullException(nameof(serviceType));

            builder.RegisterType(decoratorType).As(new DecoratorService(serviceType, condition));
        }

        /// <summary>
        /// Decorate all components implementing service <typeparamref name="TService"/>
        /// using the provided <paramref name="decorator"/> function.
        /// </summary>
        /// <typeparam name="TService">Service type being decorated.</typeparam>
        /// <param name="builder">Container builder.</param>
        /// <param name="decorator">Function decorating a component instance that provides
        /// <typeparamref name="TService"/>, given the context, parameters and service to decorate.</param>
        /// <param name="condition">A function that when provided with an <see cref="IDecoratorContext"/>
        /// instance determines if the decorator should be applied.</param>
        public static void RegisterDecorator<TService>(
            this ContainerBuilder builder,
            Func<IComponentContext, IEnumerable<Parameter>, TService, TService> decorator,
            Func<IDecoratorContext, bool> condition = null)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (decorator == null) throw new ArgumentNullException(nameof(decorator));

            var service = new DecoratorService(typeof(TService), condition);

            builder.Register((c, p) =>
            {
                var instance = (TService)p
                    .OfType<TypedParameter>()
                    .FirstOrDefault(tp => tp.Type == typeof(TService))
                    ?.Value;

                if (instance == null)
                {
                    throw new DependencyResolutionException(String.Format(CultureInfo.CurrentCulture, RegistrationExtensionsResources.DecoratorRequiresInstanceParameter, typeof(TService).Name));
                }

                return decorator(c, p, instance);
            })
            .As(service);
        }

        /// <summary>
        /// Decorate all components implementing open generic service <paramref name="serviceType"/>.
        /// </summary>
        /// <param name="builder">Container builder.</param>
        /// <param name="decoratorType">The type of the decorator. Must be an open generic type, and accept a parameter
        /// of type <paramref name="serviceType"/>, which will be set to the instance being decorated.</param>
        /// <param name="serviceType">Service type being decorated. Must be an open generic type.</param>
        /// <param name="condition">A function that when provided with an <see cref="IDecoratorContext"/>
        /// instance determines if the decorator should be applied.</param>
        public static void RegisterGenericDecorator(
            this ContainerBuilder builder,
            Type decoratorType,
            Type serviceType,
            Func<IDecoratorContext, bool> condition = null)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (decoratorType == null) throw new ArgumentNullException(nameof(decoratorType));
            if (serviceType == null) throw new ArgumentNullException(nameof(serviceType));

            OpenGenericRegistrationExtensions
                .RegisterGeneric(builder, decoratorType)
                .As(new DecoratorService(serviceType, condition));
        }

        /// <summary>
        /// Run a supplied action instead of disposing instances when they're no
        /// longer required.
        /// </summary>
        /// <typeparam name="TLimit">Registration limit type.</typeparam>
        /// <typeparam name="TActivatorData">Activator data type.</typeparam>
        /// <typeparam name="TRegistrationStyle">Registration style.</typeparam>
        /// <param name="registration">Registration to set release action for.</param>
        /// <param name="releaseAction">An action to perform instead of disposing the instance.</param>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        /// <remarks>Only one release action can be configured per registration.</remarks>
        public static IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle>
            OnRelease<TLimit, TActivatorData, TRegistrationStyle>(
                this IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> registration,
                Action<TLimit> releaseAction)
        {
            if (registration == null) throw new ArgumentNullException(nameof(registration));
            if (releaseAction == null) throw new ArgumentNullException(nameof(releaseAction));

            // Issue #677: We can't use the standard .OnActivating() handler
            // mechanism because it creates a strongly-typed "clone" of the
            // activating event args. Using a clone means a call to .ReplaceInstance()
            // on the args during activation gets lost during .OnRelease() even
            // if you keep a closure over the event args - because a later
            // .OnActivating() handler may call .ReplaceInstance() and we'll
            // have closed over the wrong thing.
            registration.ExternallyOwned();
            registration.RegistrationData.ActivatingHandlers.Add((s, e) =>
            {
                var ra = new ReleaseAction<TLimit>(releaseAction, () => (TLimit)e.Instance);
                e.Context.Resolve<ILifetimeScope>().Disposer.AddInstanceForDisposal(ra);
            });
            return registration;
        }

        /// <summary>
        /// Wraps a registration in an implicit <see cref="Autofac.IStartable"/> and automatically
        /// activates the registration after the container is built.
        /// </summary>
        /// <param name="registration">Registration to set release action for.</param>
        /// <typeparam name="TLimit">Registration limit type.</typeparam>
        /// <typeparam name="TActivatorData">Activator data type.</typeparam>
        /// <typeparam name="TRegistrationStyle">Registration style.</typeparam>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        /// <remarks>
        /// <para>
        /// While you can implement an <see cref="Autofac.IStartable"/> to perform some logic at
        /// container build time, sometimes you need to just activate a registered component and
        /// that's it. This extension allows you to automatically activate a registration on
        /// container build. No additional logic is executed and the resolved instance is not held
        /// so container disposal will end up disposing of the instance.
        /// </para>
        /// <para>
        /// Depending on how you register the lifetime of the component, you may get an exception
        /// when you build the container - components that are scoped to specific lifetimes (like
        /// ASP.NET components scoped to a request lifetime) will fail to resolve because the
        /// appropriate lifetime is not available.
        /// </para>
        /// </remarks>
        public static IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle>
            AutoActivate<TLimit, TActivatorData, TRegistrationStyle>(
            this IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> registration)
        {
            if (registration == null) throw new ArgumentNullException(nameof(registration));

            registration.RegistrationData.AddService(new AutoActivateService());
            return registration;
        }

        /// <summary>
        /// Share one instance of the component within the context of a single
        /// web/HTTP/API request. Only available for integration that supports
        /// per-request dependencies (e.g., MVC, Web API, web forms, etc.).
        /// </summary>
        /// <typeparam name="TLimit">Registration limit type.</typeparam>
        /// <typeparam name="TActivatorData">Activator data type.</typeparam>
        /// <typeparam name="TStyle">Registration style.</typeparam>
        /// <param name="registration">The registration to configure.</param>
        /// <param name="lifetimeScopeTags">Additional tags applied for matching lifetime scopes.</param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="registration" /> is <see langword="null" />.
        /// </exception>
        public static IRegistrationBuilder<TLimit, TActivatorData, TStyle>
            InstancePerRequest<TLimit, TActivatorData, TStyle>(
                this IRegistrationBuilder<TLimit, TActivatorData, TStyle> registration, params object[] lifetimeScopeTags)
        {
            if (registration == null) throw new ArgumentNullException(nameof(registration));

            var tags = new[] { MatchingScopeLifetimeTags.RequestLifetimeScopeTag }.Concat(lifetimeScopeTags).ToArray();
            return registration.InstancePerMatchingLifetimeScope(tags);
        }

        /// <summary>
        /// Attaches a predicate to evaluate prior to executing the registration.
        /// The predicate will run at registration time, not runtime, to determine
        /// whether the registration should execute.
        /// </summary>
        /// <typeparam name="TLimit">Registration limit type.</typeparam>
        /// <typeparam name="TActivatorData">Activator data type.</typeparam>
        /// <typeparam name="TStyle">Registration style.</typeparam>
        /// <param name="registration">The registration to configure.</param>
        /// <param name="predicate">The predicate to run to determine if the registration should be made.</param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="registration" /> or <paramref name="predicate" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="System.NotSupportedException">
        /// Thrown if <paramref name="registration" /> has no reference to the original callback
        /// with which it was associated (e.g., it wasn't made with a standard registration method
        /// as part of a <see cref="ContainerBuilder"/>).
        /// </exception>
        public static IRegistrationBuilder<TLimit, TActivatorData, TStyle>
            OnlyIf<TLimit, TActivatorData, TStyle>(
                this IRegistrationBuilder<TLimit, TActivatorData, TStyle> registration, Predicate<IComponentRegistry> predicate)
        {
            if (registration == null)
            {
                throw new ArgumentNullException(nameof(registration));
            }

            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }

            var c = registration.RegistrationData.DeferredCallback;
            if (c == null)
            {
                throw new NotSupportedException(RegistrationExtensionsResources.OnlyIfRequiresCallbackContainer);
            }

            var original = c.Callback;
            Action<IComponentRegistry> updated = registry =>
            {
                if (predicate(registry))
                {
                    original(registry);
                }
            };

            c.Callback = updated;
            return registration;
        }

        /// <summary>
        /// Attaches a predicate such that a registration will only be made if
        /// a specific service type is not already registered.
        /// The predicate will run at registration time, not runtime, to determine
        /// whether the registration should execute.
        /// </summary>
        /// <typeparam name="TLimit">Registration limit type.</typeparam>
        /// <typeparam name="TActivatorData">Activator data type.</typeparam>
        /// <typeparam name="TStyle">Registration style.</typeparam>
        /// <param name="registration">The registration to configure.</param>
        /// <param name="serviceType">
        /// The service type to check for to determine if the registration should be made.
        /// Note this is the *service type* - the <c>As&lt;T&gt;</c> part.
        /// </param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="registration" /> or <paramref name="serviceType" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="System.NotSupportedException">
        /// Thrown if <paramref name="registration" /> has no reference to the original callback
        /// with which it was associated (e.g., it wasn't made with a standard registration method
        /// as part of a <see cref="ContainerBuilder"/>).
        /// </exception>
        public static IRegistrationBuilder<TLimit, TActivatorData, TStyle>
            IfNotRegistered<TLimit, TActivatorData, TStyle>(
                this IRegistrationBuilder<TLimit, TActivatorData, TStyle> registration, Type serviceType)
        {
            if (registration == null)
            {
                throw new ArgumentNullException(nameof(registration));
            }

            if (serviceType == null)
            {
                throw new ArgumentNullException(nameof(serviceType));
            }

            return registration.OnlyIf(reg => !reg.IsRegistered(new TypedService(serviceType)));
        }
    }
}
