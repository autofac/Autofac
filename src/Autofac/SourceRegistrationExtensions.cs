// This software is part of the Autofac IoC container
// Copyright © 2018 Autofac Contributors
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
using Autofac.Core;
using Autofac.Core.Registration;

namespace Autofac
{
    /// <summary>
    /// Extension methods for registering <see cref="IRegistrationSource"/> instances with a container.
    /// </summary>
    public static class SourceRegistrationExtensions
    {
        /// <summary>
        /// Add a registration source to the container.
        /// </summary>
        /// <param name="builder">The builder to register the registration source with.</param>
        /// <typeparam name="TRegistrationSource">The registration source to add.</typeparam>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="builder"/> is <see langword="null"/>.
        /// </exception>
        /// <returns>
        /// The <see cref="ISourceRegistrar"/> to allow additional chained registration source registrations.
        /// </returns>
        public static ISourceRegistrar RegisterSource<TRegistrationSource>(this ContainerBuilder builder)
            where TRegistrationSource : IRegistrationSource, new()
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            var registrar = new SourceRegistrar(builder);
            return registrar.RegisterSource<TRegistrationSource>();
        }

        /// <summary>
        /// Add a registration source to the container.
        /// </summary>
        /// <param name="registrar">The source registrar that will make the registration into the container.</param>
        /// <typeparam name="TRegistrationSource">The registration source to add.</typeparam>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="registrar"/> is <see langword="null"/>.
        /// </exception>
        /// <returns>
        /// The <see cref="ISourceRegistrar"/> to allow additional chained registration source registrations.
        /// </returns>
        public static ISourceRegistrar RegisterSource<TRegistrationSource>(this ISourceRegistrar registrar)
            where TRegistrationSource : IRegistrationSource, new()
        {
            if (registrar == null) throw new ArgumentNullException(nameof(registrar));

            return registrar.RegisterSource(new TRegistrationSource());
        }
    }
}