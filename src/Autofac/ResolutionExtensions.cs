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
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Autofac.Core;
using Autofac.Core.Activators.Reflection;
using Autofac.Core.Registration;

namespace Autofac
{
    /// <summary>
    /// Adds syntactic convenience methods to the <see cref="IComponentContext"/> interface.
    /// </summary>
    public static class ResolutionExtensions
    {
        /// <summary>
        /// The <see cref="NamedParameter"/> name, provided when properties are injected onto an existing instance.
        /// </summary>
        public const string PropertyInjectedInstanceTypeNamedParameter = AutowiringPropertyInjector.InstanceTypeNamedParameter;

        private static readonly IEnumerable<Parameter> NoParameters = Enumerable.Empty<Parameter>();

        /// <summary>
        /// Set any properties on <paramref name="instance"/> that can be
        /// resolved in the context.
        /// </summary>
        /// <typeparam name="TService">Type of instance. Used only to provide method chaining.</typeparam>
        /// <param name="context">The context from which to resolve the service.</param>
        /// <param name="instance">The instance to inject properties into.</param>
        /// <returns><paramref name="instance"/>.</returns>
        public static TService InjectProperties<TService>(this IComponentContext context, TService instance)
        {
            AutowiringPropertyInjector.InjectProperties(context, instance, DefaultPropertySelector.OverwriteSetValueInstance, NoParameters);
            return instance;
        }

        /// <summary>
        /// Set any properties on <paramref name="instance"/> that can be
        /// resolved in the context.
        /// </summary>
        /// <typeparam name="TService">Type of instance. Used only to provide method chaining.</typeparam>
        /// <param name="context">The context from which to resolve the service.</param>
        /// <param name="instance">The instance to inject properties into.</param>
        /// <param name="parameters">Optional parameters to use during the property injection.</param>
        /// <returns><paramref name="instance"/>.</returns>
        public static TService InjectProperties<TService>(this IComponentContext context, TService instance, IEnumerable<Parameter> parameters)
        {
            AutowiringPropertyInjector.InjectProperties(context, instance, DefaultPropertySelector.OverwriteSetValueInstance, parameters);
            return instance;
        }

        /// <summary>
        /// Set any properties on <paramref name="instance"/> that can be
        /// resolved in the context.
        /// </summary>
        /// <typeparam name="TService">Type of instance. Used only to provide method chaining.</typeparam>
        /// <param name="context">The context from which to resolve the service.</param>
        /// <param name="instance">The instance to inject properties into.</param>
        /// <param name="parameters">Optional parameters to use during the property injection.</param>
        /// <returns><paramref name="instance"/>.</returns>
        public static TService InjectProperties<TService>(this IComponentContext context, TService instance, params Parameter[] parameters)
        {
            AutowiringPropertyInjector.InjectProperties(context, instance, DefaultPropertySelector.OverwriteSetValueInstance, parameters);
            return instance;
        }

        /// <summary>
        /// Set any properties on <paramref name="instance"/> that can be resolved by service and that satisfy the
        /// constraints imposed by <paramref name="propertySelector"/>.
        /// </summary>
        /// <typeparam name="TService">Type of instance. Used only to provide method chaining.</typeparam>
        /// <param name="context">The context from which to resolve the service.</param>
        /// <param name="instance">The instance to inject properties into.</param>
        /// <param name="propertySelector">Selector to determine with properties should be injected.</param>
        /// <returns><paramref name="instance"/>.</returns>
        public static TService InjectProperties<TService>(this IComponentContext context, TService instance, IPropertySelector propertySelector)
        {
            AutowiringPropertyInjector.InjectProperties(context, instance, propertySelector, NoParameters);
            return instance;
        }

        /// <summary>
        /// Set any properties on <paramref name="instance"/> that can be resolved by service and that satisfy the
        /// constraints imposed by <paramref name="propertySelector"/>.
        /// </summary>
        /// <typeparam name="TService">Type of instance. Used only to provide method chaining.</typeparam>
        /// <param name="context">The context from which to resolve the service.</param>
        /// <param name="instance">The instance to inject properties into.</param>
        /// <param name="propertySelector">Selector to determine with properties should be injected.</param>
        /// <param name="parameters">Optional parameters to use during the property injection.</param>
        /// <returns><paramref name="instance"/>.</returns>
        public static TService InjectProperties<TService>(this IComponentContext context, TService instance, IPropertySelector propertySelector, IEnumerable<Parameter> parameters)
        {
            AutowiringPropertyInjector.InjectProperties(context, instance, propertySelector, parameters);
            return instance;
        }

        /// <summary>
        /// Set any properties on <paramref name="instance"/> that can be resolved by service and that satisfy the
        /// constraints imposed by <paramref name="propertySelector"/>.
        /// </summary>
        /// <typeparam name="TService">Type of instance. Used only to provide method chaining.</typeparam>
        /// <param name="context">The context from which to resolve the service.</param>
        /// <param name="instance">The instance to inject properties into.</param>
        /// <param name="propertySelector">Selector to determine with properties should be injected.</param>
        /// <param name="parameters">Optional parameters to use during the property injection.</param>
        /// <returns><paramref name="instance"/>.</returns>
        public static TService InjectProperties<TService>(this IComponentContext context, TService instance, IPropertySelector propertySelector, params Parameter[] parameters)
        {
            AutowiringPropertyInjector.InjectProperties(context, instance, propertySelector, parameters);
            return instance;
        }

        /// <summary>
        /// Set any null-valued properties on <paramref name="instance"/> that can be
        /// resolved by the container.
        /// </summary>
        /// <typeparam name="TService">Type of instance. Used only to provide method chaining.</typeparam>
        /// <param name="context">The context from which to resolve the service.</param>
        /// <param name="instance">The instance to inject properties into.</param>
        /// <returns><paramref name="instance"/>.</returns>
        public static TService InjectUnsetProperties<TService>(this IComponentContext context, TService instance)
        {
            AutowiringPropertyInjector.InjectProperties(context, instance, DefaultPropertySelector.PreserveSetValueInstance, NoParameters);
            return instance;
        }

        /// <summary>
        /// Set any null-valued properties on <paramref name="instance"/> that can be
        /// resolved by the container.
        /// </summary>
        /// <typeparam name="TService">Type of instance. Used only to provide method chaining.</typeparam>
        /// <param name="context">The context from which to resolve the service.</param>
        /// <param name="instance">The instance to inject properties into.</param>
        /// <param name="parameters">Optional parameters to use during the property injection.</param>
        /// <returns><paramref name="instance"/>.</returns>
        public static TService InjectUnsetProperties<TService>(this IComponentContext context, TService instance, IEnumerable<Parameter> parameters)
        {
            AutowiringPropertyInjector.InjectProperties(context, instance, DefaultPropertySelector.PreserveSetValueInstance, parameters);
            return instance;
        }

        /// <summary>
        /// Set any null-valued properties on <paramref name="instance"/> that can be
        /// resolved by the container.
        /// </summary>
        /// <typeparam name="TService">Type of instance. Used only to provide method chaining.</typeparam>
        /// <param name="context">The context from which to resolve the service.</param>
        /// <param name="instance">The instance to inject properties into.</param>
        /// <param name="parameters">Optional parameters to use during the property injection.</param>
        /// <returns><paramref name="instance"/>.</returns>
        public static TService InjectUnsetProperties<TService>(this IComponentContext context, TService instance, params Parameter[] parameters)
        {
            AutowiringPropertyInjector.InjectProperties(context, instance, DefaultPropertySelector.PreserveSetValueInstance, parameters);
            return instance;
        }

        /// <summary>
        /// Determine whether the specified service is available in the context.
        /// </summary>
        /// <param name="context">The context from which to resolve the service.</param>
        /// <typeparam name="TService">The service to test for the registration of.</typeparam>
        /// <returns>True if the service is registered.</returns>
        public static bool IsRegistered<TService>(this IComponentContext context)
        {
            return IsRegistered(context, typeof(TService));
        }

        /// <summary>
        /// Determine whether the specified service is available in the context.
        /// </summary>
        /// <param name="context">The context from which to resolve the service.</param>
        /// <param name="serviceType">The service to test for the registration of.</param>
        /// <returns>True if the service is registered.</returns>
        public static bool IsRegistered(this IComponentContext context, Type serviceType)
        {
            return IsRegisteredService(context, new TypedService(serviceType));
        }

        /// <summary>
        /// Determine whether the specified service is available in the context.
        /// </summary>
        /// <param name="context">The context from which to resolve the service.</param>
        /// <param name="service">The service to test for the registration of.</param>
        /// <returns>True if the service is registered.</returns>
        public static bool IsRegisteredService(this IComponentContext context, Service service)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (service == null)
            {
                throw new ArgumentNullException(nameof(service));
            }

            return context.ComponentRegistry.IsRegistered(service);
        }

        /// <summary>
        /// Determine whether the specified service is available in the context.
        /// </summary>
        /// <param name="context">The context from which to resolve the service.</param>
        /// <param name="serviceKey">The key of the service to test for the registration of.</param>
        /// <typeparam name="TService">Type type of the service to test for the registration of.</typeparam>
        /// <returns>True if the service is registered.</returns>
        public static bool IsRegisteredWithKey<TService>(this IComponentContext context, object serviceKey)
        {
            return IsRegisteredWithKey(context, serviceKey, typeof(TService));
        }

        /// <summary>
        /// Determine whether the specified service is available in the context.
        /// </summary>
        /// <param name="context">The context from which to resolve the service.</param>
        /// <param name="serviceKey">The key of the service to test for the registration of.</param>
        /// <param name="serviceType">Type type of the service to test for the registration of.</param>
        /// <returns>True if the service is registered.</returns>
        public static bool IsRegisteredWithKey(this IComponentContext context, object serviceKey, Type serviceType)
        {
            return IsRegisteredService(context, new KeyedService(serviceKey, serviceType));
        }

        /// <summary>
        /// Determine whether the specified service is available in the context.
        /// </summary>
        /// <param name="context">The context from which to resolve the service.</param>
        /// <param name="serviceName">The name of the service to test for the registration of.</param>
        /// <typeparam name="TService">Type type of the service to test for the registration of.</typeparam>
        /// <returns>True if the service is registered.</returns>
        public static bool IsRegisteredWithName<TService>(this IComponentContext context, string serviceName)
        {
            return IsRegisteredWithKey<TService>(context, serviceName);
        }

        /// <summary>
        /// Determine whether the specified service is available in the context.
        /// </summary>
        /// <param name="context">The context from which to resolve the service.</param>
        /// <param name="serviceName">The name of the service to test for the registration of.</param>
        /// <param name="serviceType">Type type of the service to test for the registration of.</param>
        /// <returns>True if the service is registered.</returns>
        public static bool IsRegisteredWithName(this IComponentContext context, string serviceName, Type serviceType)
        {
            return IsRegisteredWithKey(context, serviceName, serviceType);
        }

        /// <summary>
        /// Retrieve a service from the context.
        /// </summary>
        /// <typeparam name="TService">The service to retrieve.</typeparam>
        /// <param name="context">The context from which to resolve the service.</param>
        /// <returns>The component instance that provides the service.</returns>
        /// <exception cref="ComponentNotRegisteredException" />
        /// <exception cref="DependencyResolutionException" />
        public static TService Resolve<TService>(this IComponentContext context)
        {
            return Resolve<TService>(context, NoParameters);
        }

        /// <summary>
        /// Retrieve a service from the context.
        /// </summary>
        /// <typeparam name="TService">The type to which the result will be cast.</typeparam>
        /// <param name="context">The context from which to resolve the service.</param>
        /// <param name="parameters">Parameters for the service.</param>
        /// <returns>
        /// The component instance that provides the service.
        /// </returns>
        /// <exception cref="ComponentNotRegisteredException"/>
        /// <exception cref="DependencyResolutionException"/>
        public static TService Resolve<TService>(this IComponentContext context, IEnumerable<Parameter> parameters)
        {
            return (TService)Resolve(context, typeof(TService), parameters);
        }

        /// <summary>
        /// Retrieve a service from the context.
        /// </summary>
        /// <typeparam name="TService">The type to which the result will be cast.</typeparam>
        /// <param name="context">The context from which to resolve the service.</param>
        /// <param name="parameters">Parameters for the service.</param>
        /// <returns>
        /// The component instance that provides the service.
        /// </returns>
        /// <exception cref="ComponentNotRegisteredException"/>
        /// <exception cref="DependencyResolutionException"/>
        public static TService Resolve<TService>(this IComponentContext context, params Parameter[] parameters)
        {
            return context.Resolve<TService>((IEnumerable<Parameter>)parameters);
        }

        /// <summary>
        /// Retrieve a service from the context.
        /// </summary>
        /// <param name="context">The context from which to resolve the service.</param>
        /// <param name="serviceType">The service type.</param>
        /// <returns>
        /// The component instance that provides the service.
        /// </returns>
        /// <exception cref="ComponentNotRegisteredException"/>
        /// <exception cref="DependencyResolutionException"/>
        public static object Resolve(this IComponentContext context, Type serviceType)
        {
            return Resolve(context, serviceType, NoParameters);
        }

        /// <summary>
        /// Retrieve a service from the context.
        /// </summary>
        /// <param name="context">The context from which to resolve the service.</param>
        /// <param name="parameters">Parameters for the service.</param>
        /// <param name="serviceType">The service type.</param>
        /// <returns>
        /// The component instance that provides the service.
        /// </returns>
        /// <exception cref="ComponentNotRegisteredException"/>
        /// <exception cref="DependencyResolutionException"/>
        public static object Resolve(this IComponentContext context, Type serviceType, IEnumerable<Parameter> parameters)
        {
            return ResolveService(context, new TypedService(serviceType), parameters);
        }

        /// <summary>
        /// Retrieve a service from the context.
        /// </summary>
        /// <param name="context">The context from which to resolve the service.</param>
        /// <param name="parameters">Parameters for the service.</param>
        /// <param name="serviceType">The service type.</param>
        /// <returns>
        /// The component instance that provides the service.
        /// </returns>
        /// <exception cref="ComponentNotRegisteredException"/>
        /// <exception cref="DependencyResolutionException"/>
        public static object Resolve(this IComponentContext context, Type serviceType, params Parameter[] parameters)
        {
            return context.Resolve(serviceType, (IEnumerable<Parameter>)parameters);
        }

        /// <summary>
        /// Retrieve a service from the context.
        /// </summary>
        /// <typeparam name="TService">The type to which the result will be cast.</typeparam>
        /// <param name="context">The context from which to resolve the service.</param>
        /// <param name="serviceKey">Key of the service.</param>
        /// <returns>
        /// The component instance that provides the service.
        /// </returns>
        /// <exception cref="ComponentNotRegisteredException"/>
        /// <exception cref="DependencyResolutionException"/>
        public static TService ResolveKeyed<TService>(this IComponentContext context, object serviceKey)
        {
            return ResolveKeyed<TService>(context, serviceKey, NoParameters);
        }

        /// <summary>
        /// Retrieve a service from the context.
        /// </summary>
        /// <typeparam name="TService">The type to which the result will be cast.</typeparam>
        /// <param name="context">The context from which to resolve the service.</param>
        /// <param name="serviceKey">Key of the service.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns>
        /// The component instance that provides the service.
        /// </returns>
        /// <exception cref="ComponentNotRegisteredException"/>
        /// <exception cref="DependencyResolutionException"/>
        public static TService ResolveKeyed<TService>(this IComponentContext context, object serviceKey, IEnumerable<Parameter> parameters)
        {
            return (TService)ResolveService(context, new KeyedService(serviceKey, typeof(TService)), parameters);
        }

        /// <summary>
        /// Retrieve a service from the context.
        /// </summary>
        /// <typeparam name="TService">The type to which the result will be cast.</typeparam>
        /// <param name="context">The context from which to resolve the service.</param>
        /// <param name="serviceKey">Key of the service.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns>
        /// The component instance that provides the service.
        /// </returns>
        /// <exception cref="ComponentNotRegisteredException"/>
        /// <exception cref="DependencyResolutionException"/>
        public static TService ResolveKeyed<TService>(this IComponentContext context, object serviceKey, params Parameter[] parameters)
        {
            return context.ResolveKeyed<TService>(serviceKey, (IEnumerable<Parameter>)parameters);
        }

        /// <summary>
        /// Retrieve a service from the context.
        /// </summary>
        /// <param name="context">The context from which to resolve the service.</param>
        /// <param name="serviceKey">Key of the service.</param>
        /// <param name="serviceType">Type of the service.</param>
        /// <returns>
        /// The component instance that provides the service.
        /// </returns>
        /// <exception cref="ComponentNotRegisteredException"/>
        /// <exception cref="DependencyResolutionException"/>
        public static object ResolveKeyed(this IComponentContext context, object serviceKey, Type serviceType)
        {
            return ResolveKeyed(context, serviceKey, serviceType, NoParameters);
        }

        /// <summary>
        /// Retrieve a service from the context.
        /// </summary>
        /// <param name="context">The context from which to resolve the service.</param>
        /// <param name="serviceKey">Key of the service.</param>
        /// <param name="serviceType">Type of the service.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns>
        /// The component instance that provides the service.
        /// </returns>
        /// <exception cref="ComponentNotRegisteredException"/>
        /// <exception cref="DependencyResolutionException"/>
        public static object ResolveKeyed(this IComponentContext context, object serviceKey, Type serviceType, IEnumerable<Parameter> parameters)
        {
            return ResolveService(context, new KeyedService(serviceKey, serviceType), parameters);
        }

        /// <summary>
        /// Retrieve a service from the context.
        /// </summary>
        /// <param name="context">The context from which to resolve the service.</param>
        /// <param name="serviceKey">Key of the service.</param>
        /// <param name="serviceType">Type of the service.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns>
        /// The component instance that provides the service.
        /// </returns>
        /// <exception cref="ComponentNotRegisteredException"/>
        /// <exception cref="DependencyResolutionException"/>
        public static object ResolveKeyed(this IComponentContext context, object serviceKey, Type serviceType, params Parameter[] parameters)
        {
            return context.ResolveKeyed(serviceKey, serviceType, (IEnumerable<Parameter>)parameters);
        }

        /// <summary>
        /// Retrieve a service from the context.
        /// </summary>
        /// <typeparam name="TService">The type to which the result will be cast.</typeparam>
        /// <param name="context">The context from which to resolve the service.</param>
        /// <param name="serviceName">Name of the service.</param>
        /// <returns>
        /// The component instance that provides the service.
        /// </returns>
        /// <exception cref="ComponentNotRegisteredException"/>
        /// <exception cref="DependencyResolutionException"/>
        public static TService ResolveNamed<TService>(this IComponentContext context, string serviceName)
        {
            return ResolveNamed<TService>(context, serviceName, NoParameters);
        }

        /// <summary>
        /// Retrieve a service from the context.
        /// </summary>
        /// <typeparam name="TService">The type to which the result will be cast.</typeparam>
        /// <param name="context">The context from which to resolve the service.</param>
        /// <param name="serviceName">Name of the service.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns>
        /// The component instance that provides the service.
        /// </returns>
        /// <exception cref="ComponentNotRegisteredException"/>
        /// <exception cref="DependencyResolutionException"/>
        public static TService ResolveNamed<TService>(this IComponentContext context, string serviceName, IEnumerable<Parameter> parameters)
        {
            return (TService)ResolveService(context, new KeyedService(serviceName, typeof(TService)), parameters);
        }

        /// <summary>
        /// Retrieve a service from the context.
        /// </summary>
        /// <typeparam name="TService">The type to which the result will be cast.</typeparam>
        /// <param name="context">The context from which to resolve the service.</param>
        /// <param name="serviceName">Name of the service.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns>
        /// The component instance that provides the service.
        /// </returns>
        /// <exception cref="ComponentNotRegisteredException"/>
        /// <exception cref="DependencyResolutionException"/>
        public static TService ResolveNamed<TService>(this IComponentContext context, string serviceName, params Parameter[] parameters)
        {
            return context.ResolveNamed<TService>(serviceName, (IEnumerable<Parameter>)parameters);
        }

        /// <summary>
        /// Retrieve a service from the context.
        /// </summary>
        /// <param name="context">The context from which to resolve the service.</param>
        /// <param name="serviceName">The service name.</param>
        /// <param name="serviceType">Type of the service.</param>
        /// <returns>
        /// The component instance that provides the service.
        /// </returns>
        /// <exception cref="ComponentNotRegisteredException"/>
        /// <exception cref="DependencyResolutionException"/>
        public static object ResolveNamed(this IComponentContext context, string serviceName, Type serviceType)
        {
            return ResolveNamed(context, serviceName, serviceType, NoParameters);
        }

        /// <summary>
        /// Retrieve a service from the context.
        /// </summary>
        /// <param name="context">The context from which to resolve the service.</param>
        /// <param name="parameters">Parameters for the service.</param>
        /// <param name="serviceName">The service name.</param>
        /// <param name="serviceType">Type of the service.</param>
        /// <returns>
        /// The component instance that provides the service.
        /// </returns>
        /// <exception cref="ComponentNotRegisteredException"/>
        /// <exception cref="DependencyResolutionException"/>
        public static object ResolveNamed(this IComponentContext context, string serviceName, Type serviceType, IEnumerable<Parameter> parameters)
        {
            return ResolveService(context, new KeyedService(serviceName, serviceType), parameters);
        }

        /// <summary>
        /// Retrieve a service from the context.
        /// </summary>
        /// <param name="context">The context from which to resolve the service.</param>
        /// <param name="parameters">Parameters for the service.</param>
        /// <param name="serviceName">The service name.</param>
        /// <param name="serviceType">Type of the service.</param>
        /// <returns>
        /// The component instance that provides the service.
        /// </returns>
        /// <exception cref="ComponentNotRegisteredException"/>
        /// <exception cref="DependencyResolutionException"/>
        public static object ResolveNamed(this IComponentContext context, string serviceName, Type serviceType, params Parameter[] parameters)
        {
            return context.ResolveNamed(serviceName, serviceType, (IEnumerable<Parameter>)parameters);
        }

        /// <summary>
        /// Retrieve a service from the context, or null if the service is not
        /// registered.
        /// </summary>
        /// <param name="context">The context from which to resolve the service.</param>
        /// <typeparam name="TService">The service to resolve.</typeparam>
        /// <returns>
        /// The component instance that provides the service, or null.
        /// </returns>
        /// <exception cref="DependencyResolutionException"/>
        public static TService ResolveOptional<TService>(this IComponentContext context)
            where TService : class
        {
            return ResolveOptional<TService>(context, NoParameters);
        }

        /// <summary>
        /// Retrieve a service from the context, or null if the service is not
        /// registered.
        /// </summary>
        /// <param name="context">The context from which to resolve the service.</param>
        /// <param name="parameters">Parameters for the service.</param>
        /// <typeparam name="TService">The service to resolve.</typeparam>
        /// <returns>
        /// The component instance that provides the service, or null.
        /// </returns>
        /// <exception cref="DependencyResolutionException"/>
        public static TService ResolveOptional<TService>(this IComponentContext context, IEnumerable<Parameter> parameters)
            where TService : class
        {
            return (TService)ResolveOptionalService(context, new TypedService(typeof(TService)), parameters);
        }

        /// <summary>
        /// Retrieve a service from the context, or null if the service is not
        /// registered.
        /// </summary>
        /// <param name="context">The context from which to resolve the service.</param>
        /// <param name="parameters">Parameters for the service.</param>
        /// <typeparam name="TService">The service to resolve.</typeparam>
        /// <returns>
        /// The component instance that provides the service, or null.
        /// </returns>
        /// <exception cref="DependencyResolutionException"/>
        public static TService ResolveOptional<TService>(this IComponentContext context, params Parameter[] parameters)
            where TService : class
        {
            return context.ResolveOptional<TService>((IEnumerable<Parameter>)parameters);
        }

        /// <summary>
        /// Retrieve a service from the context, or null if the service is not
        /// registered.
        /// </summary>
        /// <param name="context">The context from which to resolve the service.</param>
        /// <param name="serviceType">The type of the service.</param>
        /// <returns>
        /// The component instance that provides the service, or null.
        /// </returns>
        /// <exception cref="DependencyResolutionException"/>
        public static object ResolveOptional(this IComponentContext context, Type serviceType)
        {
            return ResolveOptional(context, serviceType, NoParameters);
        }

        /// <summary>
        /// Retrieve a service from the context, or null if the service is not
        /// registered.
        /// </summary>
        /// <param name="context">The context from which to resolve the service.</param>
        /// <param name="parameters">Parameters for the service.</param>
        /// <param name="serviceType">The type of the service.</param>
        /// <returns>
        /// The component instance that provides the service, or null.
        /// </returns>
        /// <exception cref="DependencyResolutionException"/>
        public static object ResolveOptional(this IComponentContext context, Type serviceType, IEnumerable<Parameter> parameters)
        {
            return ResolveOptionalService(context, new TypedService(serviceType), parameters);
        }

        /// <summary>
        /// Retrieve a service from the context, or null if the service is not
        /// registered.
        /// </summary>
        /// <param name="context">The context from which to resolve the service.</param>
        /// <param name="parameters">Parameters for the service.</param>
        /// <param name="serviceType">The type of the service.</param>
        /// <returns>
        /// The component instance that provides the service, or null.
        /// </returns>
        /// <exception cref="DependencyResolutionException"/>
        public static object ResolveOptional(this IComponentContext context, Type serviceType, params Parameter[] parameters)
        {
            return context.ResolveOptional(serviceType, (IEnumerable<Parameter>)parameters);
        }

        /// <summary>
        /// Retrieve a service from the context, or null if the service is not
        /// registered.
        /// </summary>
        /// <param name="context">The context from which to resolve the service.</param>
        /// <param name="serviceKey">The name of the service.</param>
        /// <typeparam name="TService">The service to resolve.</typeparam>
        /// <returns>
        /// The component instance that provides the service, or null.
        /// </returns>
        /// <exception cref="DependencyResolutionException"/>
        public static TService ResolveOptionalKeyed<TService>(this IComponentContext context, object serviceKey)
            where TService : class
        {
            return ResolveOptionalKeyed<TService>(context, serviceKey, NoParameters);
        }

        /// <summary>
        /// Retrieve a service from the context, or null if the service is not
        /// registered.
        /// </summary>
        /// <param name="context">The context from which to resolve the service.</param>
        /// <param name="parameters">Parameters for the service.</param>
        /// <param name="serviceKey">The name of the service.</param>
        /// <typeparam name="TService">The service to resolve.</typeparam>
        /// <returns>
        /// The component instance that provides the service, or null.
        /// </returns>
        /// <exception cref="DependencyResolutionException"/>
        public static TService ResolveOptionalKeyed<TService>(this IComponentContext context, object serviceKey, IEnumerable<Parameter> parameters)
            where TService : class
        {
            return (TService)ResolveOptionalService(context, new KeyedService(serviceKey, typeof(TService)), parameters);
        }

        /// <summary>
        /// Retrieve a service from the context, or null if the service is not
        /// registered.
        /// </summary>
        /// <param name="context">The context from which to resolve the service.</param>
        /// <param name="parameters">Parameters for the service.</param>
        /// <param name="serviceKey">The key of the service.</param>
        /// <typeparam name="TService">The service to resolve.</typeparam>
        /// <returns>
        /// The component instance that provides the service, or null.
        /// </returns>
        /// <exception cref="DependencyResolutionException"/>
        public static TService ResolveOptionalKeyed<TService>(this IComponentContext context, object serviceKey, params Parameter[] parameters)
            where TService : class
        {
            return context.ResolveOptionalKeyed<TService>(serviceKey, (IEnumerable<Parameter>)parameters);
        }

        /// <summary>
        /// Retrieve a service from the context, or null if the service is not
        /// registered.
        /// </summary>
        /// <param name="context">The context from which to resolve the service.</param>
        /// <param name="serviceName">The name of the service.</param>
        /// <typeparam name="TService">The service to resolve.</typeparam>
        /// <returns>
        /// The component instance that provides the service, or null.
        /// </returns>
        /// <exception cref="DependencyResolutionException"/>
        public static TService ResolveOptionalNamed<TService>(this IComponentContext context, string serviceName)
            where TService : class
        {
            return ResolveOptionalKeyed<TService>(context, serviceName);
        }

        /// <summary>
        /// Retrieve a service from the context, or null if the service is not
        /// registered.
        /// </summary>
        /// <param name="context">The context from which to resolve the service.</param>
        /// <param name="parameters">Parameters for the service.</param>
        /// <param name="serviceName">The name of the service.</param>
        /// <typeparam name="TService">The service to resolve.</typeparam>
        /// <returns>
        /// The component instance that provides the service, or null.
        /// </returns>
        /// <exception cref="DependencyResolutionException"/>
        public static TService ResolveOptionalNamed<TService>(this IComponentContext context, string serviceName, IEnumerable<Parameter> parameters)
            where TService : class
        {
            return ResolveOptionalKeyed<TService>(context, serviceName, parameters);
        }

        /// <summary>
        /// Retrieve a service from the context, or null if the service is not
        /// registered.
        /// </summary>
        /// <param name="context">The context from which to resolve the service.</param>
        /// <param name="parameters">Parameters for the service.</param>
        /// <param name="serviceName">The name of the service.</param>
        /// <typeparam name="TService">The service to resolve.</typeparam>
        /// <returns>
        /// The component instance that provides the service, or null.
        /// </returns>
        /// <exception cref="DependencyResolutionException"/>
        public static TService ResolveOptionalNamed<TService>(this IComponentContext context, string serviceName, params Parameter[] parameters)
            where TService : class
        {
            return context.ResolveOptionalKeyed<TService>(serviceName, parameters);
        }

        /// <summary>
        /// Retrieve a service from the context, or null if the service is not
        /// registered.
        /// </summary>
        /// <param name="context">The context from which to resolve the service.</param>
        /// <param name="service">The service.</param>
        /// <returns>
        /// The component instance that provides the service, or null.
        /// </returns>
        /// <exception cref="DependencyResolutionException"/>
        public static object ResolveOptionalService(this IComponentContext context, Service service)
        {
            return ResolveOptionalService(context, service, NoParameters);
        }

        /// <summary>
        /// Retrieve a service from the context, or null if the service is not
        /// registered.
        /// </summary>
        /// <param name="context">The context from which to resolve the service.</param>
        /// <param name="service">The service.</param>
        /// <param name="parameters">Parameters for the service.</param>
        /// <returns>
        /// The component instance that provides the service, or null.
        /// </returns>
        /// <exception cref="DependencyResolutionException"/>
        public static object ResolveOptionalService(this IComponentContext context, Service service, IEnumerable<Parameter> parameters)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (service == null)
            {
                throw new ArgumentNullException(nameof(service));
            }

            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            object instance;
            context.TryResolveService(service, parameters, out instance);
            return instance;
        }

        /// <summary>
        /// Retrieve a service from the context, or null if the service is not
        /// registered.
        /// </summary>
        /// <param name="context">The context from which to resolve the service.</param>
        /// <param name="service">The service.</param>
        /// <param name="parameters">Parameters for the service.</param>
        /// <returns>
        /// The component instance that provides the service, or null.
        /// </returns>
        /// <exception cref="DependencyResolutionException"/>
        public static object ResolveOptionalService(this IComponentContext context, Service service, params Parameter[] parameters)
        {
            return context.ResolveOptionalService(service, (IEnumerable<Parameter>)parameters);
        }

        /// <summary>
        /// Retrieve a service from the context.
        /// </summary>
        /// <param name="context">The context from which to resolve the service.</param>
        /// <param name="service">The service to resolve.</param>
        /// <returns>
        /// The component instance that provides the service.
        /// </returns>
        /// <exception cref="ComponentNotRegisteredException"/>
        /// <exception cref="DependencyResolutionException"/>
        public static object ResolveService(this IComponentContext context, Service service)
        {
            return ResolveService(context, service, NoParameters);
        }

        /// <summary>
        /// Retrieve a service from the context.
        /// </summary>
        /// <param name="context">The context from which to resolve the service.</param>
        /// <param name="parameters">Parameters for the service.</param>
        /// <param name="service">The service to resolve.</param>
        /// <returns>
        /// The component instance that provides the service.
        /// </returns>
        /// <exception cref="ComponentNotRegisteredException"/>
        /// <exception cref="DependencyResolutionException"/>
        public static object ResolveService(this IComponentContext context, Service service, IEnumerable<Parameter> parameters)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (service == null)
            {
                throw new ArgumentNullException(nameof(service));
            }

            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            object instance;
            var successful = context.TryResolveService(service, parameters, out instance);
            if (!successful)
            {
                throw new ComponentNotRegisteredException(service);
            }

            return instance;
        }

        /// <summary>
        /// Retrieve a service from the context.
        /// </summary>
        /// <param name="context">The context from which to resolve the service.</param>
        /// <param name="parameters">Parameters for the service.</param>
        /// <param name="service">The service to resolve.</param>
        /// <returns>
        /// The component instance that provides the service.
        /// </returns>
        /// <exception cref="ComponentNotRegisteredException"/>
        /// <exception cref="DependencyResolutionException"/>
        public static object ResolveService(this IComponentContext context, Service service, params Parameter[] parameters)
        {
            return context.ResolveService(service, (IEnumerable<Parameter>)parameters);
        }

        /// <summary>
        /// Try to retrieve a service from the context.
        /// </summary>
        /// <typeparam name="T">The service type to resolve.</typeparam>
        /// <param name="context">The context from which to resolve the service.</param>
        /// <param name="instance">The resulting component instance providing the service, or default(T).</param>
        /// <returns>
        /// True if a component providing the service is available.
        /// </returns>
        /// <exception cref="DependencyResolutionException"/>
        public static bool TryResolve<T>(this IComponentContext context, out T instance)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            object component;
            var success = context.TryResolve(typeof(T), out component);

            instance = success ? (T)component : default(T);
            return success;
        }

        /// <summary>
        /// Try to retrieve a service from the context.
        /// </summary>
        /// <param name="context">The context from which to resolve the service.</param>
        /// <param name="serviceType">The service type to resolve.</param>
        /// <param name="instance">The resulting component instance providing the service, or null.</param>
        /// <returns>
        /// True if a component providing the service is available.
        /// </returns>
        /// <exception cref="DependencyResolutionException"/>
        [SuppressMessage("Microsoft.Design", "CA1007:UseGenericsWhereAppropriate")]
        public static bool TryResolve(this IComponentContext context, Type serviceType, out object instance)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            return context.TryResolveService(new TypedService(serviceType), NoParameters, out instance);
        }

        /// <summary>
        /// Try to retrieve a service from the context.
        /// </summary>
        /// <param name="context">The context from which to resolve the service.</param>
        /// <param name="serviceKey">The key of the service to resolve.</param>
        /// <param name="serviceType">The type of the service to resolve.</param>
        /// <param name="instance">The resulting component instance providing the service, or null.</param>
        /// <returns>
        /// True if a component providing the service is available.
        /// </returns>
        /// <exception cref="DependencyResolutionException"/>
        [SuppressMessage("Microsoft.Design", "CA1007:UseGenericsWhereAppropriate")]
        public static bool TryResolveKeyed(this IComponentContext context, object serviceKey, Type serviceType, out object instance)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            return context.TryResolveService(new KeyedService(serviceKey, serviceType), NoParameters, out instance);
        }

        /// <summary>
        /// Try to retrieve a service from the context.
        /// </summary>
        /// <param name="context">The context from which to resolve the service.</param>
        /// <param name="serviceName">The name of the service to resolve.</param>
        /// <param name="serviceType">The type of the service to resolve.</param>
        /// <param name="instance">The resulting component instance providing the service, or null.</param>
        /// <returns>
        /// True if a component providing the service is available.
        /// </returns>
        /// <exception cref="DependencyResolutionException"/>
        [SuppressMessage("Microsoft.Design", "CA1007:UseGenericsWhereAppropriate")]
        public static bool TryResolveNamed(this IComponentContext context, string serviceName, Type serviceType, out object instance)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            return context.TryResolveService(new KeyedService(serviceName, serviceType), NoParameters, out instance);
        }

        /// <summary>
        /// Try to retrieve a service from the context.
        /// </summary>
        /// <param name="context">The context from which to resolve the service.</param>
        /// <param name="service">The service to resolve.</param>
        /// <param name="instance">The resulting component instance providing the service, or null.</param>
        /// <returns>
        /// True if a component providing the service is available.
        /// </returns>
        /// <exception cref="DependencyResolutionException"/>
        [SuppressMessage("Microsoft.Design", "CA1007:UseGenericsWhereAppropriate")]
        public static bool TryResolveService(this IComponentContext context, Service service, out object instance)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            return context.TryResolveService(service, NoParameters, out instance);
        }

        /// <summary>
        /// Try to retrieve a service from the context.
        /// </summary>
        /// <param name="context">The context from which to resolve the service.</param>
        /// <param name="service">The service to resolve.</param>
        /// <param name="instance">The resulting component instance providing the service, or null.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns>
        /// True if a component providing the service is available.
        /// </returns>
        /// <exception cref="DependencyResolutionException"/>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="context" /> is <see langword="null" />.
        /// </exception>
        [SuppressMessage("Microsoft.Design", "CA1007:UseGenericsWhereAppropriate")]
        public static bool TryResolveService(this IComponentContext context, Service service, IEnumerable<Parameter> parameters, out object instance)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            IComponentRegistration registration;
            if (!context.ComponentRegistry.TryGetRegistration(service, out registration))
            {
                instance = null;
                return false;
            }

            instance = context.ResolveComponent(registration, parameters);
            return true;
        }
    }
}
