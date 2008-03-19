// This software is part of the Autofac IoC container
// Copyright (c) 2007 - 2008 Autofac Contributors
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
using Autofac.Registrars;
using Autofac.Registrars.Reflective;

namespace Autofac.Builder
{
    /// <summary>
    /// Extends ContainerBuilder to register components by type.
    /// </summary>
    public static class ReflectiveRegistrationBuilder
    {
        /// <summary>
        /// Register a component to be created through reflection.
        /// </summary>
        /// <typeparam name="T">The type of the component.</typeparam>
        /// <returns>A registrar allowing details of the registration to be customised.</returns>
        public static IReflectiveRegistrar Register<T>(this ContainerBuilder builder)
        {
            return Register(builder, typeof(T));
        }

        /// <summary>
        /// Register a component to be created through reflection.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="implementor">The type of the component.</param>
        /// <returns>
        /// A registrar allowing details of the registration to be customised.
        /// </returns>
        public static IReflectiveRegistrar Register(this ContainerBuilder builder, Type implementor)
        {
            Enforce.ArgumentNotNull(implementor, "implementor");
            return builder.AttachRegistrar<IReflectiveRegistrar>(
            	new ReflectiveRegistrar(implementor));
        }
    }
}
