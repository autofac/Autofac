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
using System.Globalization;
using System.Reflection;
using Autofac.Builder;
using Autofac.Core;

namespace Autofac.Features.OpenGenerics
{
    internal static class OpenGenericRegistrationExtensions
    {
        public static IRegistrationBuilder<object, ReflectionActivatorData, DynamicRegistrationStyle>
            RegisterGeneric(ContainerBuilder builder, Type implementor)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (implementor == null) throw new ArgumentNullException(nameof(implementor));

            if (!implementor.GetTypeInfo().IsGenericTypeDefinition)
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, OpenGenericRegistrationExtensionsResources.ImplementorMustBeOpenGenericType, implementor));

            var rb = new RegistrationBuilder<object, ReflectionActivatorData, DynamicRegistrationStyle>(
                new TypedService(implementor),
                new ReflectionActivatorData(implementor),
                new DynamicRegistrationStyle());

            rb.RegistrationData.DeferredCallback = builder.RegisterCallback(cr => cr.AddRegistrationSource(
                new OpenGenericRegistrationSource(rb.RegistrationData, rb.ActivatorData)));

            return rb;
        }

        public static IRegistrationBuilder<object, OpenGenericDecoratorActivatorData, DynamicRegistrationStyle>
            RegisterGenericDecorator(ContainerBuilder builder, Type decoratorType, Type decoratedServiceType, object fromKey, object toKey)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (decoratorType == null) throw new ArgumentNullException(nameof(decoratorType));
            if (decoratedServiceType == null) throw new ArgumentNullException(nameof(decoratedServiceType));

            var rb = new RegistrationBuilder<object, OpenGenericDecoratorActivatorData, DynamicRegistrationStyle>(
                (Service)GetServiceWithKey(decoratedServiceType, toKey),
                new OpenGenericDecoratorActivatorData(decoratorType, GetServiceWithKey(decoratedServiceType, fromKey)),
                new DynamicRegistrationStyle());

            rb.RegistrationData.DeferredCallback = builder.RegisterCallback(cr => cr.AddRegistrationSource(
                new OpenGenericDecoratorRegistrationSource(rb.RegistrationData, rb.ActivatorData)));

            return rb;
        }

        private static IServiceWithType GetServiceWithKey(Type serviceType, object key)
        {
            if (key == null)
                return new TypedService(serviceType);
            return new KeyedService(key, serviceType);
        }
    }
}
