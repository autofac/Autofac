// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

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
    /// Adds syntactic convenience methods to the <see cref="IComponentContext"/> interface,
    /// specifically to support value types in the nullable reference scenario.
    /// </summary>
    public static class ResolutionValueExtensions
    {
        private static readonly IEnumerable<Parameter> NoParameters = Enumerable.Empty<Parameter>();

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
        public static TService? ResolveOptional<TService>(this IComponentContext context)
            where TService : struct
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
        public static TService? ResolveOptional<TService>(this IComponentContext context, IEnumerable<Parameter> parameters)
            where TService : struct
        {
            return (TService?)context.ResolveOptionalService(new TypedService(typeof(TService)), parameters);
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
        public static TService? ResolveOptional<TService>(this IComponentContext context, params Parameter[] parameters)
            where TService : struct
        {
            return context.ResolveOptional<TService>((IEnumerable<Parameter>)parameters);
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
        public static TService? ResolveOptionalKeyed<TService>(this IComponentContext context, object serviceKey)
            where TService : struct
        {
            return context.ResolveOptionalKeyed<TService>(serviceKey, NoParameters);
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
        public static TService? ResolveOptionalKeyed<TService>(this IComponentContext context, object serviceKey, IEnumerable<Parameter> parameters)
            where TService : struct
        {
            return (TService?)context.ResolveOptionalService(new KeyedService(serviceKey, typeof(TService)), parameters);
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
        public static TService? ResolveOptionalKeyed<TService>(this IComponentContext context, object serviceKey, params Parameter[] parameters)
            where TService : struct
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
        public static TService? ResolveOptionalNamed<TService>(this IComponentContext context, string serviceName)
            where TService : struct
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
        public static TService? ResolveOptionalNamed<TService>(this IComponentContext context, string serviceName, IEnumerable<Parameter> parameters)
            where TService : struct
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
        public static TService? ResolveOptionalNamed<TService>(this IComponentContext context, string serviceName, params Parameter[] parameters)
            where TService : struct
        {
            return context.ResolveOptionalKeyed<TService>(serviceName, parameters);
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
        public static bool TryResolve<T>(this IComponentContext context, [NotNullWhen(returnValue: true)] out T? instance)
            where T : struct
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            // Null annotation attributes only work if placed directly in an if statement.
            if (context.TryResolve(typeof(T), out object? component))
            {
                instance = (T)component;

                return true;
            }
            else
            {
                instance = default;

                return false;
            }
        }
    }
}
