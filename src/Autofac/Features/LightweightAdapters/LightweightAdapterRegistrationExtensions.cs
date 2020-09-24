// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using Autofac.Builder;
using Autofac.Core;

namespace Autofac.Features.LightweightAdapters
{
    /// <summary>
    /// Extension methods for registering adapters.
    /// </summary>
    internal static class LightweightAdapterRegistrationExtensions
    {
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
                ContainerBuilder builder,
                Func<IComponentContext, IEnumerable<Parameter>, TFrom, TTo> adapter)
            where TTo : notnull
        {
            return RegisterAdapter(builder, adapter, new TypedService(typeof(TFrom)), new TypedService(typeof(TTo)));
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
                ContainerBuilder builder,
                Func<IComponentContext, IEnumerable<Parameter>, TService, TService> decorator,
                object fromKey,
                object? toKey)
            where TService : notnull
        {
            return RegisterAdapter(builder, decorator, ServiceWithKey<TService>(fromKey), ServiceWithKey<TService>(toKey));
        }

        private static Service ServiceWithKey<TService>(object? key)
        {
            if (key == null)
            {
                return new TypedService(typeof(TService));
            }

            return new KeyedService(key, typeof(TService));
        }

        private static IRegistrationBuilder<TTo, LightweightAdapterActivatorData, DynamicRegistrationStyle>
            RegisterAdapter<TFrom, TTo>(
                ContainerBuilder builder,
                Func<IComponentContext, IEnumerable<Parameter>, TFrom, TTo> adapter,
                Service fromService,
                Service toService)
            where TTo : notnull
        {
            var rb = new RegistrationBuilder<TTo, LightweightAdapterActivatorData, DynamicRegistrationStyle>(
                toService,
                new LightweightAdapterActivatorData(fromService, (c, p, f) => adapter(c, p, (TFrom)f)),
                new DynamicRegistrationStyle());

            rb.RegistrationData.DeferredCallback = builder.RegisterCallback(cr => cr.AddRegistrationSource(
                new LightweightAdapterRegistrationSource(rb.RegistrationData, rb.ActivatorData)));

            return rb;
        }
    }
}
