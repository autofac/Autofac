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
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using Autofac.Builder;
using Autofac.Core;
using Autofac.Core.Activators.Delegate;

namespace Autofac.Features.OpenGenerics
{
    /// <summary>
    /// Extension methods to support open generic registrations.
    /// </summary>
    internal static class OpenGenericRegistrationExtensions
    {
        /// <summary>
        /// Register an un-parameterised generic type, e.g. Repository&lt;&gt;.
        /// Concrete types will be made as they are requested, e.g. with Resolve&lt;Repository&lt;int&gt;&gt;().
        /// </summary>
        /// <param name="builder">Container builder.</param>
        /// <param name="implementer">The open generic implementation type.</param>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        public static IRegistrationBuilder<object, ReflectionActivatorData, DynamicRegistrationStyle>
            RegisterGeneric(ContainerBuilder builder, Type implementer)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            var rb = CreateGenericBuilder(implementer);

            rb.RegistrationData.DeferredCallback = builder.RegisterCallback(cr => cr.AddRegistrationSource(
                new OpenGenericRegistrationSource(rb.RegistrationData, rb.ResolvePipeline.Clone(), rb.ActivatorData)));

            return rb;
        }

        /// <summary>
        /// Register an un-parameterised generic type, e.g. Repository&lt;&gt;.
        /// Concrete types will be made as they are requested, e.g. with Resolve&lt;Repository&lt;int&gt;&gt;().
        /// </summary>
        /// <param name="builder">Container builder.</param>
        /// <param name="factory">Delegate responsible for generating an instance of a closed generic based on the open generic type being registered.</param>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        public static IRegistrationBuilder<object, OpenGenericDelegateActivatorData, DynamicRegistrationStyle>
            RegisterGeneric(ContainerBuilder builder, Func<IComponentContext, Type[], IEnumerable<Parameter>, object> factory)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            var rb = CreateGenericBuilder(factory);

            rb.RegistrationData.DeferredCallback = builder.RegisterCallback(cr => cr.AddRegistrationSource(
                new OpenGenericDelegateRegistrationSource(rb.RegistrationData, rb.ResolvePipeline.Clone(), rb.ActivatorData)));

            return rb;
        }

        /// <summary>
        /// Creates an un-parameterised generic type, e.g. Repository&lt;&gt;, without registering it.
        /// Concrete types will be made as they are requested, e.g. with Resolve&lt;Repository&lt;int&gt;&gt;().
        /// </summary>
        /// <param name="implementer">The open generic implementation type.</param>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        public static IRegistrationBuilder<object, ReflectionActivatorData, DynamicRegistrationStyle>
            CreateGenericBuilder(Type implementer)
        {
            if (implementer == null)
            {
                throw new ArgumentNullException(nameof(implementer));
            }

            if (!implementer.IsGenericTypeDefinition)
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, OpenGenericRegistrationExtensionsResources.ImplementorMustBeOpenGenericType, implementer));
            }

            var rb = new RegistrationBuilder<object, ReflectionActivatorData, DynamicRegistrationStyle>(
                new TypedService(implementer),
                new ReflectionActivatorData(implementer),
                new DynamicRegistrationStyle());

            return rb;
        }

        /// <summary>
        /// Creates a registration builder for an un-parameterised generic type, e.g. Repository&lt;&gt;, without registering it.
        /// Concrete types will be made as they are requested, e.g. with Resolve&lt;Repository&lt;int&gt;&gt;().
        /// </summary>
        /// <param name="factory">Delegate responsible for generating an instance of a closed generic based on the open generic type being registered.</param>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        public static IRegistrationBuilder<object, OpenGenericDelegateActivatorData, DynamicRegistrationStyle>
            CreateGenericBuilder(Func<IComponentContext, Type[], IEnumerable<Parameter>, object> factory)
        {
            if (factory == null)
            {
                throw new ArgumentNullException(nameof(factory));
            }

            var rb = new RegistrationBuilder<object, OpenGenericDelegateActivatorData, DynamicRegistrationStyle>(
                new TypedService(typeof(object)),
                new OpenGenericDelegateActivatorData(factory),
                new DynamicRegistrationStyle());

            return rb;
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
            RegisterGenericDecorator(ContainerBuilder builder, Type decoratorType, Type decoratedServiceType, object fromKey, object? toKey)
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

            var rb = new RegistrationBuilder<object, OpenGenericDecoratorActivatorData, DynamicRegistrationStyle>(
                (Service)GetServiceWithKey(decoratedServiceType, toKey),
                new OpenGenericDecoratorActivatorData(decoratorType, GetServiceWithKey(decoratedServiceType, fromKey)),
                new DynamicRegistrationStyle());

            rb.RegistrationData.DeferredCallback = builder.RegisterCallback(cr => cr.AddRegistrationSource(
                new OpenGenericDecoratorRegistrationSource(rb.RegistrationData, rb.ResolvePipeline.Clone(), rb.ActivatorData)));

            return rb;
        }

        private static IServiceWithType GetServiceWithKey(Type serviceType, object? key)
        {
            if (key == null)
            {
                return new TypedService(serviceType);
            }

            return new KeyedService(key, serviceType);
        }
    }
}
