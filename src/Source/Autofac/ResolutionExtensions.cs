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
using Autofac.Core.Activators.Reflection;
using Autofac.Core.Registration;
using Autofac.Util;

namespace Autofac
{
    /// <summary>
    /// Adds syntactic convenience methods to the <see cref="IComponentContext"/> interface.
    /// </summary>
    public static class ResolutionExtensions
    {
        static readonly IEnumerable<Parameter> NoParameters = Enumerable.Empty<Parameter>();

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
            new AutowiringPropertyInjector().InjectProperties(context, instance, true);
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
            new AutowiringPropertyInjector().InjectProperties(context, instance, false);
            return instance;
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
        public static TService Resolve<TService>(this IComponentContext context, string serviceName)
        {
            return Resolve<TService>(context, serviceName, NoParameters);
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
        public static TService Resolve<TService>(this IComponentContext context, string serviceName, IEnumerable<Parameter> parameters)
        {
            return (TService)Resolve(context, new NamedService(serviceName, typeof(TService)), parameters);
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
        public static TService Resolve<TService>(this IComponentContext context, string serviceName, params Parameter[] parameters)
        {
            return context.Resolve<TService>(serviceName, (IEnumerable<Parameter>)parameters);
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
        public static TService Resolve<TService>(this IComponentContext context, object serviceKey)
        {
            return Resolve<TService>(context, serviceKey, NoParameters);
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
        public static TService Resolve<TService>(this IComponentContext context, object serviceKey, IEnumerable<Parameter> parameters)
        {
            return (TService)Resolve(context, new KeyedService(serviceKey, typeof(TService)), parameters);
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
        public static TService Resolve<TService>(this IComponentContext context, object serviceKey, params Parameter[] parameters)
        {
            return context.Resolve<TService>(serviceKey, (IEnumerable<Parameter>)parameters);
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
            return Resolve(context, new TypedService(serviceType), parameters);
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
        /// <param name="context">The context from which to resolve the service.</param>
        /// <param name="serviceName">The service name.</param>
        /// <param name="serviceType">Type of the service.</param>
        /// <returns>
        /// The component instance that provides the service.
        /// </returns>
        /// <exception cref="ComponentNotRegisteredException"/>
        /// <exception cref="DependencyResolutionException"/>
        public static object Resolve(this IComponentContext context, string serviceName, Type serviceType)
        {
            return Resolve(context, serviceName, serviceType, NoParameters);
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
        public static object Resolve(this IComponentContext context, string serviceName, Type serviceType, IEnumerable<Parameter> parameters)
        {
            return Resolve(context, new NamedService(serviceName, serviceType), parameters);
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
        public static object Resolve(this IComponentContext context, string serviceName, Type serviceType, params Parameter[] parameters)
        {
            return context.Resolve(serviceName, serviceType, (IEnumerable<Parameter>)parameters);
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
        public static object Resolve(this IComponentContext context, Service service)
        {
            return Resolve(context, service, NoParameters);
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
        public static object Resolve(this IComponentContext context, Service service, IEnumerable<Parameter> parameters)
        {
            Enforce.ArgumentNotNull(context, "context");
            Enforce.ArgumentNotNull(service, "service");
            Enforce.ArgumentNotNull(parameters, "parameters");

            object instance;
            var successful = context.TryResolve(service, parameters, out instance);
            if (!successful)
                throw new ComponentNotRegisteredException(service);
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
        public static object Resolve(this IComponentContext context, Service service, params Parameter[] parameters)
        {
            return context.Resolve(service, (IEnumerable<Parameter>)parameters);
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
            return (TService)ResolveOptional(context, new TypedService(typeof(TService)), parameters);
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
        /// <param name="serviceName">The name of the service.</param>
        /// <typeparam name="TService">The service to resolve.</typeparam>
        /// <returns>
        /// The component instance that provides the service, or null.
        /// </returns>
        /// <exception cref="DependencyResolutionException"/>
        public static TService ResolveOptional<TService>(this IComponentContext context, string serviceName)
            where TService : class
        {
            return ResolveOptional<TService>(context, serviceName, NoParameters);
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
        public static TService ResolveOptional<TService>(this IComponentContext context, string serviceName, IEnumerable<Parameter> parameters)
            where TService : class
        {
            return (TService)ResolveOptional(context, new NamedService(serviceName, typeof(TService)), parameters);
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
        public static TService ResolveOptional<TService>(this IComponentContext context, string serviceName, params Parameter[] parameters)
            where TService : class
        {
            return context.ResolveOptional<TService>(serviceName, (IEnumerable<Parameter>)parameters);
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
            return ResolveOptional(context, new TypedService(serviceType), parameters);
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
        /// <param name="service">The service.</param>
        /// <returns>
        /// The component instance that provides the service, or null.
        /// </returns>
        /// <exception cref="DependencyResolutionException"/>
        public static object ResolveOptional(this IComponentContext context, Service service)
        {
            return ResolveOptional(context, service, NoParameters);
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
        public static object ResolveOptional(this IComponentContext context, Service service, IEnumerable<Parameter> parameters)
        {
            Enforce.ArgumentNotNull(context, "context");
            Enforce.ArgumentNotNull(service, "service");
            Enforce.ArgumentNotNull(parameters, "parameters");

            object instance;
            context.TryResolve(service, parameters, out instance);
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
        public static object ResolveOptional(this IComponentContext context, Service service, params Parameter[] parameters)
        {
            return context.ResolveOptional(service, (IEnumerable<Parameter>)parameters);
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
            return IsRegistered(context, new TypedService(serviceType));
        }

        /// <summary>
        /// Determine whether the specified service is available in the context.
        /// </summary>
        /// <param name="context">The context from which to resolve the service.</param>
        /// <param name="serviceName">The name of the service to test for the registration of.</param>
        /// <param name="serviceType">Type type of the service to test for the registration of.</param>
        /// <returns>True if the service is registered.</returns>
        public static bool IsRegistered(this IComponentContext context, string serviceName, Type serviceType)
        {
            return IsRegistered(context, new NamedService(serviceName, serviceType));
        }

        /// <summary>
        /// Determine whether the specified service is available in the context.
        /// </summary>
        /// <param name="context">The context from which to resolve the service.</param>
        /// <param name="serviceName">The name of the service to test for the registration of.</param>
        /// <typeparam name="TService">Type type of the service to test for the registration of.</typeparam>
        /// <returns>True if the service is registered.</returns>
        public static bool IsRegistered<TService>(this IComponentContext context, string serviceName)
        {
            return IsRegistered(context, serviceName, typeof(TService));
        }

        /// <summary>
        /// Determine whether the specified service is available in the context.
        /// </summary>
        /// <param name="context">The context from which to resolve the service.</param>
        /// <param name="service">The service to test for the registration of.</param>
        /// <returns>True if the service is registered.</returns>
        public static bool IsRegistered(this IComponentContext context, Service service)
        {
            Enforce.ArgumentNotNull(context, "context");
            Enforce.ArgumentNotNull(service, "service");

            return context.ComponentRegistry.IsRegistered(service);
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
        public static bool TryResolve(this IComponentContext context, Service service, IEnumerable<Parameter> parameters, out object instance)
        {
            IComponentRegistration registration;
            if (!context.ComponentRegistry.TryGetRegistration(service, out registration))
            {
                instance = null;
                return false;
            }

            instance = context.Resolve(registration, parameters);
            return true;
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
        public static bool TryResolve(this IComponentContext context, Service service, out object instance)
        {
            Enforce.ArgumentNotNull(context, "context");
            return context.TryResolve(service, NoParameters, out instance);
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
        public static bool TryResolve(this IComponentContext context, Type serviceType, out object instance)
        {
            Enforce.ArgumentNotNull(context, "context");
            return context.TryResolve(new TypedService(serviceType), NoParameters, out instance);
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
            Enforce.ArgumentNotNull(context, "context");

            object component;
            bool success = context.TryResolve(typeof(T), out component);

            instance = success ? (T)component : default(T);
            return success;
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
        public static bool TryResolve(this IComponentContext context, string serviceName, Type serviceType, out object instance)
        {
            Enforce.ArgumentNotNull(context, "context");
            return context.TryResolve(new NamedService(serviceName, serviceType), NoParameters, out instance);
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
        public static bool TryResolve(this IComponentContext context, object serviceKey, Type serviceType, out object instance)
        {
            Enforce.ArgumentNotNull(context, "context");
            return context.TryResolve(new KeyedService(serviceKey, serviceType), NoParameters, out instance);
        }
    }
}
