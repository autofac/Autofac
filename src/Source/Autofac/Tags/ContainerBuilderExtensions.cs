// Contributed by Nicholas Blumhardt 2008-02-10
// Copyright (c) 2008 Autofac Contributors
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
using Autofac.Builder;
using Autofac.Registrars;
using Autofac.Registrars.Delegate;

namespace Autofac.Tags
{
    /// <summary>
    /// Extensions for ContainerBuilder that allow registrations to be targeted to
    /// contexts with a certain tag only.
    /// </summary>
    public static class ContainerBuilderExtensions
    {
        /// <summary>
        /// Extensions for ContainerBuilder that allow registrations to be targeted to
        /// contexts with a certain tag only.
        /// </summary>
        /// <typeparam name="TTag">The type of the tag.</typeparam>
        /// <param name="builder">The builder.</param>
        /// <param name="implementor">The implementation type.</param>
        /// <param name="targetContext">The target context.</param>
        /// <returns>A registrar allowing configuration to continue.</returns>
        public static IReflectiveRegistrar RegisterInContext<TTag>(
            this ContainerBuilder builder,
            Type implementor,
            TTag targetContext)
        {
            if (builder == null)
                throw new ArgumentNullException("builder");

            if (implementor == null)
                throw new ArgumentNullException("implementor");

            return builder.AttachRegistrar<IReflectiveRegistrar>(
            	new TaggedReflectiveRegistrar<TTag>(implementor, targetContext));
        }

        /// <summary>
        /// Extensions for ContainerBuilder that allow registrations to be targeted to
        /// contexts with a certain tag only.
        /// </summary>
        /// <typeparam name="TComponent">The type of the component.</typeparam>
        /// <typeparam name="TTag">The type of the tag.</typeparam>
        /// <param name="builder">The builder.</param>
        /// <param name="activator">The activator.</param>
        /// <param name="targetContext">The target context.</param>
        /// <returns>A registrar allowing configuration to continue.</returns>
        public static IConcreteRegistrar RegisterInContext<TComponent, TTag>(
            this ContainerBuilder builder,
            ComponentActivatorWithParameters<TComponent> activator,
            TTag targetContext)
        {
            if (builder == null)
                throw new ArgumentNullException("builder");

            if (activator == null)
                throw new ArgumentNullException("activator");

            return builder.AttachRegistrar<IConcreteRegistrar>(
            	new TaggedDelegateRegistrar<TTag>(typeof(TComponent), (c, p) => activator(c, p), targetContext));
        }
        
        /// <summary>
        /// Extensions for ContainerBuilder that allow registrations to be targeted to
        /// contexts with a certain tag only.
        /// </summary>
        /// <typeparam name="TActivator">The type of the activator.</typeparam>
        /// <typeparam name="TTag">The type of the tag.</typeparam>
        /// <param name="builder">The builder.</param>
        /// <param name="activator">The activator.</param>
        /// <param name="targetContext">The target context.</param>
        /// <returns>A registrar allowing configuration to continue.</returns>
        public static IConcreteRegistrar RegisterInContext<TActivator, TTag>(
            this ContainerBuilder builder,
            ComponentActivator<TActivator> activator,
            TTag targetContext)
        {
            if (builder == null)
                throw new ArgumentNullException("builder");

            if (activator == null)
                throw new ArgumentNullException("activator");

            return RegisterInContext<TActivator, TTag>(
                builder,
                (c, p) => activator(c),
                targetContext);
        }
    }
}
