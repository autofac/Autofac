// This software is part of the Autofac IoC container
// Copyright (c) 2007 Nicholas Blumhardt
// nicholas.blumhardt@gmail.com
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
using System.Linq;
using System.Text;

namespace Autofac.Builder
{
    /// <summary>
    /// Extends ContainerBuilder to register creation delegates.
    /// </summary>
    public static class DelegateRegistrationBuilder
    {
        /// <summary>
        /// Register a component that will be created using a provided delegate.
        /// </summary>
        /// <typeparam name="T">The type of the component.</typeparam>
        /// <param name="builder">The builder.</param>
        /// <param name="creator">The creator.</param>
        /// <returns>
        /// A registrar allowing details of the registration to be customised.
        /// </returns>
        public static IConcreteRegistrar Register<T>(this ContainerBuilder builder, ComponentActivator<T> creator)
        {
            Enforce.ArgumentNotNull(builder, "builder");
            Enforce.ArgumentNotNull(creator, "creator");
            return Register<T>(builder, (c, p) => creator(c));
        }

        /// <summary>
        /// Register a component that will be created using a provided delegate.
        /// </summary>
        /// <typeparam name="T">The type of the component.</typeparam>
        /// <param name="builder">The builder.</param>
        /// <param name="creator">The creator.</param>
        /// <returns>
        /// A registrar allowing details of the registration to be customised.
        /// </returns>
        public static IConcreteRegistrar Register<T>(this ContainerBuilder builder, ComponentActivatorWithParameters<T> creator)
        {
            Enforce.ArgumentNotNull(builder, "builder");
            Enforce.ArgumentNotNull(creator, "creator");
            var result = new DelegateRegistrar(typeof(T), (c, p) => creator(c, p));
            builder.RegisterModule(result);
            return result
                .WithOwnership(builder.DefaultOwnership)
                .WithScope(builder.DefaultScope);
        }
    }
}
