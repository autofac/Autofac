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

namespace Autofac.Core.Registration
{
    /// <summary>
    /// Basic implementation of the <see cref="ISourceRegistrar"/>
    /// interface allowing registration of registration sources into a <see cref="ContainerBuilder"/>
    /// in a fluent format.
    /// </summary>
    internal sealed class SourceRegistrar : ISourceRegistrar
    {
        /// <summary>
        /// The <see cref="ContainerBuilder"/> into which registrations will be made.
        /// </summary>
        private readonly ContainerBuilder _builder;

        /// <summary>
        /// Initializes a new instance of the <see cref="SourceRegistrar"/> class.
        /// </summary>
        /// <param name="builder">
        /// The <see cref="ContainerBuilder"/> into which registrations will be made.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="builder" /> is <see langword="null" />.
        /// </exception>
        public SourceRegistrar(ContainerBuilder builder)
        {
            _builder = builder ?? throw new ArgumentNullException(nameof(builder));
        }

        /// <summary>
        /// Add a registration source to the container.
        /// </summary>
        /// <param name="registrationSource">The registration source to add.</param>
        /// <returns>
        /// The <see cref="ISourceRegistrar"/> to allow additional chained registration source registrations.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="registrationSource" /> is <see langword="null" />.
        /// </exception>
        public ISourceRegistrar RegisterSource(IRegistrationSource registrationSource)
        {
            if (registrationSource == null) throw new ArgumentNullException(nameof(registrationSource));

            _builder.RegisterCallback(cr => cr.AddRegistrationSource(registrationSource));
            return this;
        }
    }
}