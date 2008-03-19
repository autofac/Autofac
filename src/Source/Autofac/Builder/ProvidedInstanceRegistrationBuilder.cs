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

using Autofac.Registrars;
using Autofac.Registrars.ProvidedInstance;

namespace Autofac.Builder
{
    /// <summary>
    /// Extends ContainerBuilder to support provided instances.
    /// </summary>
    public static class ProvidedInstanceRegistrationBuilder
    {
        /// <summary>
        /// Register a component using a provided instance.
        /// </summary>
        /// <typeparam name="T">The type of the component.</typeparam>
        /// <param name="builder">The builder.</param>
        /// <param name="instance">The instance.</param>
        /// <returns>
        /// A registrar allowing details of the registration to be customised.
        /// </returns>
        public static IConcreteRegistrar Register<T>(this ContainerBuilder builder, T instance)
        {
            Enforce.ArgumentNotNull(builder, "builder");
            // Scope of instances is always singleton, this will throw an exception
            // if the default is otherwise.
            return builder.AttachRegistrar<IConcreteRegistrar>(
            	new ProvidedInstanceRegistrar(instance, typeof(T)));
        }
    }
}
