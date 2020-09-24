// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using Autofac.Builder;
using Autofac.Core;
using Autofac.Core.Registration;

namespace Autofac
{
    /// <summary>
    /// Adds registration syntax to the <see cref="ContainerBuilder"/> type.
    /// </summary>
    [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    public static partial class RegistrationExtensions
    {
        /// <summary>
        /// <para>
        /// Register a composite type that should always provide the instance of <typeparamref name="TService"/> when it is resolved,
        /// regardless of what other registrations for <typeparamref name="TService"/> are available.
        /// </para>
        /// <para>
        /// Composite registrations are not included when resolving a collection of <typeparamref name="TService"/>.
        /// </para>
        /// </summary>
        /// <typeparam name="TComposite">
        /// Service type of the composite. Must accept a parameter
        /// of type <see cref="IEnumerable{TService}"/>, <see cref="IList{TService}"/> or <typeparamref name="TService[]"/>,
        /// which will be set to collection of registered implementations.
        /// </typeparam>
        /// <typeparam name="TService">Service type to provide a composite for.</typeparam>
        /// <param name="builder">Container builder.</param>
        public static IRegistrationBuilder<TComposite, ConcreteReflectionActivatorData, SingleRegistrationStyle> RegisterComposite<TComposite, TService>(this ContainerBuilder builder)
            where TComposite : notnull, TService
            where TService : notnull
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            var rb = builder.RegisterType<TComposite>().As<TService>();

            ApplyCompositeConfiguration(builder, rb);

            return rb;
        }

        /// <summary>
        /// <para>
        /// Register a composite type that should always provide the instance of <paramref name="serviceType"/> when it is resolved,
        /// regardless of what other registrations for <paramref name="serviceType"/> are available.
        /// </para>
        /// <para>
        /// Composite registrations are not included when resolving a collection of <paramref name="serviceType"/>.
        /// </para>
        /// </summary>
        /// <param name="builder">Container builder.</param>
        /// <param name="compositeType">
        /// Service type of the composite. Must accept a parameter
        /// of type <see cref="IEnumerable{TService}"/>, <see cref="IList{TService}"/> or an array of <paramref name="serviceType"/>,
        /// which will be set to the collection of registered implementations.
        /// </param>
        /// <param name="serviceType">Service type to provide a composite for.</param>
        public static IRegistrationBuilder<object, ConcreteReflectionActivatorData, SingleRegistrationStyle> RegisterComposite(
            this ContainerBuilder builder,
            Type compositeType,
            Type serviceType)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            var rb = builder.RegisterType(compositeType).As(serviceType);

            ApplyCompositeConfiguration(builder, rb);

            return rb;
        }

        /// <summary>
        /// <para>
        /// Register a delegate that should always provide the composite instance of a service type when it is resolved,
        /// regardless of what other registrations for <typeparamref name="TService"/> are available.
        /// </para>
        /// <para>
        ///  Composite registrations are not included when resolving a collection of <typeparamref name="TService"/>.
        /// </para>
        /// </summary>
        /// <param name="builder">Container builder.</param>
        /// <param name="compositeDelegate">
        /// Callback to create a new instance of the composite, which takes the set of concrete implementations.
        /// </param>
        /// <typeparam name="TService">Service type to provide a composite for.</typeparam>
        public static IRegistrationBuilder<TService, SimpleActivatorData, SingleRegistrationStyle> RegisterComposite<TService>(
            this ContainerBuilder builder,
            Func<IComponentContext, IEnumerable<Parameter>, IEnumerable<TService>, TService> compositeDelegate)
            where TService : notnull
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            var rb = builder.Register((ctxt, p) =>
            {
                var concreteItems = ctxt.Resolve<IEnumerable<TService>>();

                return compositeDelegate(ctxt, p, concreteItems);
            });

            ApplyCompositeConfiguration(builder, rb);

            return rb;
        }

        /// <summary>
        /// <para>
        /// Register a delegate that should always provide the composite instance of a service type when it is resolved,
        /// regardless of what other registrations for <typeparamref name="TService"/> are available.
        /// </para>
        /// <para>
        /// Composite registrations are not included when resolving a collection of <typeparamref name="TService"/>.
        /// </para>
        /// </summary>
        /// <param name="builder">Container builder.</param>
        /// <param name="compositeDelegate">
        /// Callback to create a new instance of the composite, which takes the set of concrete implementations.
        /// </param>
        /// <typeparam name="TService">Service type to provide a composite for.</typeparam>
        public static IRegistrationBuilder<TService, SimpleActivatorData, SingleRegistrationStyle> RegisterComposite<TService>(
            this ContainerBuilder builder,
            Func<IComponentContext, IEnumerable<TService>, TService> compositeDelegate)
            where TService : notnull
        {
            return builder.RegisterComposite<TService>((ctxt, p, concrete) => compositeDelegate(ctxt, concrete));
        }

        /// <summary>
        /// <para>
        /// Register an un-parameterised generic type, e.g. Composite&lt;&gt; to function as a composite
        /// for an open generic service, e.g. IRepository&lt;&gt;. Composites will be made as they are requested,
        /// e.g. with Resolve&lt;IRepository&lt;int&gt;&gt;().
        /// </para>
        /// <para>
        ///  Composite registrations are not included when resolving a collection of <paramref name="serviceType"/>.
        /// </para>
        /// </summary>
        /// <param name="builder">Container builder.</param>
        /// <param name="compositeType">
        /// Service type of the composite. Must accept a parameter
        /// of type <see cref="IEnumerable{TService}"/>, <see cref="IList{TService}"/> or an array of <paramref name="serviceType"/>,
        /// which will be set to the collection of registered implementations.
        /// </param>
        /// <param name="serviceType">Service type to provide a composite for.</param>
        public static IRegistrationBuilder<object, ReflectionActivatorData, DynamicRegistrationStyle> RegisterGenericComposite(
            this ContainerBuilder builder,
            Type compositeType,
            Type serviceType)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            var rb = builder.RegisterGeneric(compositeType).As(serviceType);

            ApplyCompositeConfiguration(builder, rb);

            return rb;
        }

        private static void ApplyCompositeConfiguration<TLimit, TActivatorData, TStyle>(ContainerBuilder builder, IRegistrationBuilder<TLimit, TActivatorData, TStyle> registration)
        {
            // Set the options for a composite.
            registration.RegistrationData.Options |= RegistrationOptions.Composite;

            builder.RegisterCallback(crb =>
            {
                // Validate that we are only behaving as a composite for a single service.
                if (registration.RegistrationData.Services.Count() > 1)
                {
                    // Cannot have a multi-service composite.
                    throw new InvalidOperationException(
                        string.Format(
                            CultureInfo.CurrentCulture,
                            RegistrationExtensionsResources.CompositesCannotProvideMultipleServices,
                            registration));
                }
            });
        }
    }
}
