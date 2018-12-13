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
using Autofac.Builder;
using Autofac.Core;

namespace Autofac.Features.LightweightAdapters
{
    internal static class LightweightAdapterRegistrationExtensions
    {
        public static IRegistrationBuilder<TTo, LightweightAdapterActivatorData, DynamicRegistrationStyle>
            RegisterAdapter<TFrom, TTo>(
                ContainerBuilder builder,
                Func<IComponentContext, IEnumerable<Parameter>, TFrom, TTo> adapter)
        {
            return RegisterAdapter(builder, adapter, new TypedService(typeof(TFrom)), new TypedService(typeof(TTo)));
        }

        public static IRegistrationBuilder<TService, LightweightAdapterActivatorData, DynamicRegistrationStyle>
            RegisterDecorator<TService>(
                ContainerBuilder builder,
                Func<IComponentContext, IEnumerable<Parameter>, TService, TService> decorator,
                object fromKey,
                object toKey)
        {
            return RegisterAdapter(builder, decorator, ServiceWithKey<TService>(fromKey), ServiceWithKey<TService>(toKey));
        }

        private static Service ServiceWithKey<TService>(object key)
        {
            if (key == null)
                return new TypedService(typeof(TService));
            return new KeyedService(key, typeof(TService));
        }

        private static IRegistrationBuilder<TTo, LightweightAdapterActivatorData, DynamicRegistrationStyle>
            RegisterAdapter<TFrom, TTo>(
                ContainerBuilder builder,
                Func<IComponentContext, IEnumerable<Parameter>, TFrom, TTo> adapter,
                Service fromService,
                Service toService)
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
