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
using System.Globalization;
using System.Linq;
using System.Reflection;
using Autofac.Core;
using Autofac.Core.Activators.Delegate;
using Autofac.Core.Registration;

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
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            return CreateRegistration(
                builder.RegistrationStyle.Id,
                builder.RegistrationData,
                builder.ActivatorData.Activator,
                builder.RegistrationData.Services.ToArray(),
                builder.RegistrationStyle.Target,
                builder.RegistrationStyle.IsAdapterForIndividualComponent);
        }

        /// <summary>
        /// Create an IComponentRegistration from data.
        /// </summary>
        /// <param name="id">Id of the registration.</param>
        /// <param name="data">Registration data.</param>
        /// <param name="activator">Activator.</param>
        /// <param name="services">Services provided by the registration.</param>
        /// <returns>An IComponentRegistration.</returns>
        public static IComponentRegistration CreateRegistration(
            Guid id,
            RegistrationData data,
            IInstanceActivator activator,
            Service[] services)
        {
            return CreateRegistration(id, data, activator, services, null);
        }

        /// <summary>
        /// Create an IComponentRegistration from data.
        /// </summary>
        /// <param name="id">Id of the registration.</param>
        /// <param name="data">Registration data.</param>
        /// <param name="activator">Activator.</param>
        /// <param name="services">Services provided by the registration.</param>
        /// <param name="target">Optional; target registration.</param>
        /// <param name="isAdapterForIndividualComponent">Optional; whether the registration is a 1:1 adapters on top of another component.</param>
        /// <returns>An IComponentRegistration.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="activator" /> or <paramref name="data" /> is <see langword="null" />.
        /// </exception>
        public static IComponentRegistration CreateRegistration(
            Guid id,
            RegistrationData data,
            IInstanceActivator activator,
            Service[] services,
            IComponentRegistration target,
            bool isAdapterForIndividualComponent = false)
        {
            if (activator == null) throw new ArgumentNullException(nameof(activator));
            if (data == null) throw new ArgumentNullException(nameof(data));

            var limitType = activator.LimitType;
            if (limitType != typeof(object))
            {
                foreach (var ts in services)
                {
                    var asServiceWithType = ts as IServiceWithType;
                    if (asServiceWithType == null)
                    {
                        continue;
                    }

                    if (!asServiceWithType.ServiceType.GetTypeInfo().IsAssignableFrom(limitType.GetTypeInfo()))
                    {
                        throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, RegistrationBuilderResources.ComponentDoesNotSupportService, limitType, ts));
                    }
                }
            }

            IComponentRegistration registration;
            if (target == null)
            {
                registration = new ComponentRegistration(
                    id,
                    activator,
                    data.Lifetime,
                    data.Sharing,
                    data.Ownership,
                    services,
                    data.Metadata);
            }
            else
            {
                registration = new ComponentRegistration(
                    id,
                    activator,
                    data.Lifetime,
                    data.Sharing,
                    data.Ownership,
                    services,
                    data.Metadata,
                    target,
                    isAdapterForIndividualComponent);
            }

            foreach (var p in data.PreparingHandlers)
                registration.Preparing += p;

            foreach (var ac in data.ActivatingHandlers)
                registration.Activating += ac;

            foreach (var ad in data.ActivatedHandlers)
                registration.Activated += ad;

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
            IComponentRegistry cr,
            IRegistrationBuilder<TLimit, TActivatorData, TSingleRegistrationStyle> builder)
            where TSingleRegistrationStyle : SingleRegistrationStyle
            where TActivatorData : IConcreteActivatorData
        {
            if (cr == null) throw new ArgumentNullException(nameof(cr));

            var registration = CreateRegistration(builder);

            cr.Register(registration, builder.RegistrationStyle.PreserveDefaults);

            var registeredEventArgs = new ComponentRegisteredEventArgs(cr, registration);
            foreach (var rh in builder.RegistrationStyle.RegisteredHandlers)
                rh(cr, registeredEventArgs);
        }
    }
}
