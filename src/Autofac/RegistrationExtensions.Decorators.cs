// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using Autofac.Builder;
using Autofac.Core;
using Autofac.Core.Resolving.Pipeline;
using Autofac.Features.Decorators;
using Autofac.Features.LightweightAdapters;
using Autofac.Features.OpenGenerics;

namespace Autofac
{
    /// <summary>
    /// Adds registration syntax to the <see cref="ContainerBuilder"/> type.
    /// </summary>
    [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    public static partial class RegistrationExtensions
    {
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
                object? toKey = null)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (decoratorType == null)
            {
                throw new ArgumentNullException(nameof(decoratorType));
            }

            if (decoratedServiceType == null)
            {
                throw new ArgumentNullException(nameof(decoratedServiceType));
            }

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
                object? toKey = null)
            where TService : notnull
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (decorator == null)
            {
                throw new ArgumentNullException(nameof(decorator));
            }

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
                object? toKey = null)
            where TService : notnull
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (decorator == null)
            {
                throw new ArgumentNullException(nameof(decorator));
            }

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
                object? toKey = null)
            where TService : notnull
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (decorator == null)
            {
                throw new ArgumentNullException(nameof(decorator));
            }

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
        public static void RegisterDecorator<TDecorator, TService>(this ContainerBuilder builder, Func<IDecoratorContext, bool>? condition = null)
            where TDecorator : notnull, TService
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            var decoratorService = new DecoratorService(typeof(TService), condition);

            var rb = RegistrationBuilder.ForType<TDecorator>().As(decoratorService);

            var decoratorRegistration = rb.CreateRegistration();

            var middleware = new DecoratorMiddleware(decoratorService, decoratorRegistration);
            builder.RegisterServiceMiddleware<TService>(middleware, MiddlewareInsertionMode.StartOfPhase);

            // Add the decorator to the registry so the pipeline gets built.
            builder.RegisterCallback(crb => crb.Register(decoratorRegistration));
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
            Func<IDecoratorContext, bool>? condition = null)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (decoratorType == null)
            {
                throw new ArgumentNullException(nameof(decoratorType));
            }

            if (serviceType == null)
            {
                throw new ArgumentNullException(nameof(serviceType));
            }

            var decoratorService = new DecoratorService(serviceType, condition);

            var rb = RegistrationBuilder.ForType(decoratorType).As(decoratorService);

            var decoratorRegistration = rb.CreateRegistration();

            var middleware = new DecoratorMiddleware(decoratorService, decoratorRegistration);

            builder.RegisterServiceMiddleware(serviceType, middleware, MiddlewareInsertionMode.StartOfPhase);

            // Add the decorator to the registry so the pipeline gets built.
            builder.RegisterCallback(crb => crb.Register(decoratorRegistration));
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
            Func<IDecoratorContext, bool>? condition = null)
            where TService : class
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (decorator == null)
            {
                throw new ArgumentNullException(nameof(decorator));
            }

            var service = new DecoratorService(typeof(TService), condition);

            var rb = RegistrationBuilder.ForDelegate((c, p) =>
            {
                TService? instance = (TService?)p
                    .OfType<TypedParameter>()
                    .FirstOrDefault(tp => tp.Type == typeof(TService))
                    ?.Value;

                if (instance == null)
                {
                    throw new DependencyResolutionException(string.Format(CultureInfo.CurrentCulture, RegistrationExtensionsResources.DecoratorRequiresInstanceParameter, typeof(TService).Name));
                }

                return decorator(c, p, instance);
            }).As(service);

            var decoratorRegistration = rb.CreateRegistration();

            var middleware = new DecoratorMiddleware(service, decoratorRegistration);

            builder.RegisterServiceMiddleware<TService>(middleware, MiddlewareInsertionMode.StartOfPhase);

            // Add the decorator to the registry so the pipeline gets built.
            builder.RegisterCallback(crb => crb.Register(decoratorRegistration));
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
            Func<IDecoratorContext, bool>? condition = null)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (decoratorType == null)
            {
                throw new ArgumentNullException(nameof(decoratorType));
            }

            if (serviceType == null)
            {
                throw new ArgumentNullException(nameof(serviceType));
            }

            var decoratorService = new DecoratorService(serviceType, condition);

            var genericRegistration = OpenGenericRegistrationExtensions
                .CreateGenericBuilder(decoratorType)
                .As(decoratorService);

            builder.RegisterServiceMiddlewareSource(new OpenGenericDecoratorMiddlewareSource(decoratorService, genericRegistration.RegistrationData, genericRegistration.ActivatorData));
        }
    }
}
