// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Autofac.Core;
using Autofac.Core.Activators.Delegate;
using Autofac.Core.Pipeline;
using Autofac.Core.Registration;
using Autofac.Core.Resolving.Pipeline;

namespace Autofac.Builder
{
    /// <summary>
    /// Static factory methods to simplify the creation and handling of IRegistrationBuilder{L,A,R}.
    /// </summary>
    /// <example>
    /// To create an <see cref="IComponentRegistration"/> for a specific type, use:
    /// <code>
    /// var cr = RegistrationBuilder.ForType(t).CreateRegistration();
    /// </code>
    /// The full builder syntax is supported.
    /// <code>
    /// var cr = RegistrationBuilder.ForType(t).Named("foo").ExternallyOwned().CreateRegistration();
    /// </code>
    /// </example>
    public static class RegistrationBuilder
    {
        /// <summary>
        /// Creates a registration builder for the provided delegate.
        /// </summary>
        /// <typeparam name="T">Instance type returned by delegate.</typeparam>
        /// <param name="delegate">Delegate to register.</param>
        /// <returns>A registration builder.</returns>
        public static IRegistrationBuilder<T, SimpleActivatorData, SingleRegistrationStyle> ForDelegate<T>(Func<IComponentContext, IEnumerable<Parameter>, T> @delegate)
            where T : notnull
        {
            return new RegistrationBuilder<T, SimpleActivatorData, SingleRegistrationStyle>(
                new TypedService(typeof(T)),
                new SimpleActivatorData(new DelegateActivator(typeof(T), (c, p) => @delegate(c, p))),
                new SingleRegistrationStyle());
        }

        /// <summary>
        /// Creates a registration builder for the provided delegate.
        /// </summary>
        /// <param name="delegate">Delegate to register.</param>
        /// <param name="limitType">Most specific type return value of delegate can be cast to.</param>
        /// <returns>A registration builder.</returns>
        public static IRegistrationBuilder<object, SimpleActivatorData, SingleRegistrationStyle> ForDelegate(Type limitType, Func<IComponentContext, IEnumerable<Parameter>, object> @delegate)
        {
            return new RegistrationBuilder<object, SimpleActivatorData, SingleRegistrationStyle>(
                new TypedService(limitType),
                new SimpleActivatorData(new DelegateActivator(limitType, @delegate)),
                new SingleRegistrationStyle());
        }

        /// <summary>
        /// Creates a registration builder for the provided type.
        /// </summary>
        /// <typeparam name="TImplementer">Implementation type to register.</typeparam>
        /// <returns>A registration builder.</returns>
        public static IRegistrationBuilder<TImplementer, ConcreteReflectionActivatorData, SingleRegistrationStyle> ForType<TImplementer>()
            where TImplementer : notnull
        {
            return new RegistrationBuilder<TImplementer, ConcreteReflectionActivatorData, SingleRegistrationStyle>(
                new TypedService(typeof(TImplementer)),
                new ConcreteReflectionActivatorData(typeof(TImplementer)),
                new SingleRegistrationStyle());
        }

        /// <summary>
        /// Creates a registration builder for the provided type.
        /// </summary>
        /// <param name="implementationType">Implementation type to register.</param>
        /// <returns>A registration builder.</returns>
        public static IRegistrationBuilder<object, ConcreteReflectionActivatorData, SingleRegistrationStyle> ForType(Type implementationType)
        {
            return new RegistrationBuilder<object, ConcreteReflectionActivatorData, SingleRegistrationStyle>(
                new TypedService(implementationType),
                new ConcreteReflectionActivatorData(implementationType),
                new SingleRegistrationStyle());
        }

        /// <summary>
        /// Create an <see cref='IComponentRegistration'/> from a <see cref='RegistrationBuilder'/>.
        /// There is no need to call this method when registering components through a <see cref="ContainerBuilder"/>.
        /// </summary>
        /// <remarks>
        /// When called on the result of one of the <see cref='ContainerBuilder'/> methods,
        /// the returned registration will be different from the one the builder itself registers
        /// in the container.
        /// </remarks>
        /// <example>
        /// <code>
        /// var registration = RegistrationBuilder.ForType&lt;Foo&gt;().CreateRegistration();
        /// </code>
        /// </example>
        /// <param name="builder">The registration builder.</param>
        /// <returns>An IComponentRegistration.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="builder" /> is <see langword="null" />.
        /// </exception>
        public static IComponentRegistration CreateRegistration<TLimit, TActivatorData, TSingleRegistrationStyle>(
            this IRegistrationBuilder<TLimit, TActivatorData, TSingleRegistrationStyle> builder)
            where TSingleRegistrationStyle : SingleRegistrationStyle
            where TActivatorData : IConcreteActivatorData
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            return CreateRegistration(
                builder.RegistrationStyle.Id,
                builder.RegistrationData,
                builder.ActivatorData.Activator,
                builder.ResolvePipeline,
                builder.RegistrationData.Services.ToArray(),
                builder.RegistrationStyle.Target);
        }

        /// <summary>
        /// Create an IComponentRegistration from data.
        /// </summary>
        /// <param name="id">Id of the registration.</param>
        /// <param name="data">Registration data.</param>
        /// <param name="activator">Activator.</param>
        /// <param name="pipelineBuilder">The component registration's resolve pipeline builder.</param>
        /// <param name="services">Services provided by the registration.</param>
        /// <returns>An IComponentRegistration.</returns>
        public static IComponentRegistration CreateRegistration(
            Guid id,
            RegistrationData data,
            IInstanceActivator activator,
            IResolvePipelineBuilder pipelineBuilder,
            Service[] services)
        {
            return CreateRegistration(id, data, activator, pipelineBuilder, services, null);
        }

        /// <summary>
        /// Create an IComponentRegistration from data.
        /// </summary>
        /// <param name="id">Id of the registration.</param>
        /// <param name="data">Registration data.</param>
        /// <param name="activator">Activator.</param>
        /// <param name="pipelineBuilder">The component registration's resolve pipeline builder.</param>
        /// <param name="services">Services provided by the registration.</param>
        /// <param name="target">Optional; target registration.</param>
        /// <returns>An IComponentRegistration.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="activator" /> or <paramref name="data" /> is <see langword="null" />.
        /// </exception>
        public static IComponentRegistration CreateRegistration(
            Guid id,
            RegistrationData data,
            IInstanceActivator activator,
            IResolvePipelineBuilder pipelineBuilder,
            Service[] services,
            IComponentRegistration? target)
        {
            if (activator == null)
            {
                throw new ArgumentNullException(nameof(activator));
            }

            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            if (pipelineBuilder is null)
            {
                throw new ArgumentNullException(nameof(pipelineBuilder));
            }

            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            var limitType = activator.LimitType;
            if (limitType != typeof(object))
            {
                foreach (var ts in services)
                {
                    if (!(ts is IServiceWithType asServiceWithType))
                    {
                        continue;
                    }

                    if (!asServiceWithType.ServiceType.IsAssignableFrom(limitType))
                    {
                        throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, RegistrationBuilderResources.ComponentDoesNotSupportService, limitType, ts));
                    }
                }
            }

            // The pipeline builder fed into the registration is a copy, so that the original builder cannot be edited after the registration has been created,
            // and the original does not contain any auto-added items.
            var clonedPipelineBuilder = pipelineBuilder.Clone();

            IComponentRegistration registration;

            if (target == null)
            {
                registration = new ComponentRegistration(
                    id,
                    activator,
                    data.Lifetime,
                    data.Sharing,
                    data.Ownership,
                    clonedPipelineBuilder,
                    services,
                    data.Metadata,
                    data.Options);
            }
            else
            {
                registration = new ComponentRegistration(
                    id,
                    activator,
                    data.Lifetime,
                    data.Sharing,
                    data.Ownership,
                    clonedPipelineBuilder,
                    services,
                    data.Metadata,
                    target,
                    data.Options);
            }

            return registration;
        }

        /// <summary>
        /// Register a component in the component registry. This helper method is necessary
        /// in order to execute OnRegistered hooks and respect PreserveDefaults.
        /// </summary>
        /// <remarks>Hoping to refactor this out.</remarks>
        /// <param name="cr">Component registry to make registration in.</param>
        /// <param name="builder">Registration builder with data for new registration.</param>
        public static void RegisterSingleComponent<TLimit, TActivatorData, TSingleRegistrationStyle>(
            IComponentRegistryBuilder cr,
            IRegistrationBuilder<TLimit, TActivatorData, TSingleRegistrationStyle> builder)
            where TSingleRegistrationStyle : SingleRegistrationStyle
            where TActivatorData : IConcreteActivatorData
        {
            if (cr == null)
            {
                throw new ArgumentNullException(nameof(cr));
            }

            var registration = CreateRegistration(builder);

            cr.Register(registration, builder.RegistrationStyle.PreserveDefaults);

            var registeredEventArgs = new ComponentRegisteredEventArgs(cr, registration);
            foreach (var rh in builder.RegistrationStyle.RegisteredHandlers)
            {
                rh(cr, registeredEventArgs);
            }
        }
    }
}
