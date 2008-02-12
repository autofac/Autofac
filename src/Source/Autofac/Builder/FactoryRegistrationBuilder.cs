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
using Autofac.Component;
using Autofac.Component.Registration;

namespace Autofac.Builder
{
    /// <summary>
    /// Extends ContainerBuilder to support automatically-generated factories.
    /// </summary>
    public static class FactoryRegistrationBuilder
    {
        /// <summary>
        /// Register a regular component activator (with parameters)
        /// but expose it through a delegate type. The resolved delegate's
        /// parameters will be provided to the activator as named values.
        /// </summary>
        /// <typeparam name="TCreator">Delegate type that will be resolvable.</typeparam>
        /// <param name="builder">The builder.</param>
        /// <param name="creator">Activator that will create the result value of
        /// calls to the delegate.</param>
        public static void RegisterFactory<TCreator>(this ContainerBuilder builder, ComponentActivator creator)
        {
            RegisterFactory(builder, typeof(TCreator), creator);
        }

        /// <summary>
        /// Register a regular component activator (with parameters)
        /// but expose it through a delegate type. The resolved delegate's
        /// parameters will be provided to the activator as named values.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="factoryDelegate">The factory delegate.</param>
        /// <param name="creator">Activator that will create the result value of
        /// calls to the delegate.</param>
        public static void RegisterFactory(this ContainerBuilder builder, Type factoryDelegate, ComponentActivator creator)
        {
            Enforce.ArgumentNotNull(builder, "builder");
            Enforce.ArgumentNotNull(creator, "creator");
            Enforce.ArgumentNotNull(factoryDelegate, "factoryDelegate");

            builder.RegisterComponent(new ContextAwareDelegateRegistration(factoryDelegate, creator));
        }
    }
}
