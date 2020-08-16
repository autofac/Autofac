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
using System.Diagnostics.CodeAnalysis;
using Autofac.Builder;
using Autofac.Core;
using Autofac.Features.LightweightAdapters;

namespace Autofac
{
    /// <summary>
    /// Adds registration syntax to the <see cref="ContainerBuilder"/> type.
    /// </summary>
    [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    public static partial class RegistrationExtensions
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
                this ContainerBuilder builder,
                Func<IComponentContext, IEnumerable<Parameter>, TFrom, TTo> adapter)
            where TFrom : notnull
            where TTo : notnull
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (adapter == null)
            {
                throw new ArgumentNullException(nameof(adapter));
            }

            return LightweightAdapterRegistrationExtensions.RegisterAdapter(builder, adapter);
        }

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
        /// service <typeparamref name="TTo"/>, given the context.</param>
        public static IRegistrationBuilder<TTo, LightweightAdapterActivatorData, DynamicRegistrationStyle>
            RegisterAdapter<TFrom, TTo>(
                this ContainerBuilder builder,
                Func<IComponentContext, TFrom, TTo> adapter)
            where TFrom : notnull
            where TTo : notnull
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (adapter == null)
            {
                throw new ArgumentNullException(nameof(adapter));
            }

            return builder.RegisterAdapter<TFrom, TTo>((c, p, f) => adapter(c, f));
        }

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
        /// service <typeparamref name="TTo"/>.</param>
        public static IRegistrationBuilder<TTo, LightweightAdapterActivatorData, DynamicRegistrationStyle>
            RegisterAdapter<TFrom, TTo>(
                this ContainerBuilder builder,
                Func<TFrom, TTo> adapter)
            where TFrom : notnull
            where TTo : notnull
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (adapter == null)
            {
                throw new ArgumentNullException(nameof(adapter));
            }

            return builder.RegisterAdapter<TFrom, TTo>((c, p, f) => adapter(f));
        }
    }
}
