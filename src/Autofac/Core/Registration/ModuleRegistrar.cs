// This software is part of the Autofac IoC container
// Copyright © 2011 Autofac Contributors
// http://autofac.org
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
    /// Basic implementation of the <see cref="IModuleRegistrar"/>
    /// interface allowing registration of modules into a <see cref="ContainerBuilder"/>
    /// in a fluent format.
    /// </summary>
    internal class ModuleRegistrar : IModuleRegistrar
    {
        /// <summary>
        /// The <see cref="ContainerBuilder"/> into which registrations will be made.
        /// </summary>
        private readonly ContainerBuilder _builder;

        /// <summary>
        /// Initializes a new instance of the <see cref="ModuleRegistrar"/> class.
        /// </summary>
        /// <param name="builder">
        /// The <see cref="ContainerBuilder"/> into which registrations will be made.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="builder" /> is <see langword="null" />.
        /// </exception>
        public ModuleRegistrar(ContainerBuilder builder)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            _builder = builder;
        }

        /// <summary>
        /// Add a module to the container.
        /// </summary>
        /// <param name="module">The module to add.</param>
        /// <returns>
        /// The <see cref="IModuleRegistrar"/> to allow
        /// additional chained module registrations.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="module" /> is <see langword="null" />.
        /// </exception>
        public IModuleRegistrar RegisterModule(IModule module)
        {
            if (module == null) throw new ArgumentNullException(nameof(module));

            _builder.RegisterCallback(module.Configure);
            return this;
        }
    }
}
