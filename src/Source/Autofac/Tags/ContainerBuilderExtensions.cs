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
using System.Globalization;
using System.Linq;
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
        static readonly Parameter[] EmptyParameters = new Parameter[0];

        /// <summary>
        /// Extensions for ContainerBuilder that allow registrations to be targeted to
        /// contexts with a certain tag only.
        /// </summary>
        /// <typeparam name="TComponent">The type of the component.</typeparam>
        /// <typeparam name="TTag">The type of the tag.</typeparam>
        /// <param name="builder">The builder.</param>
        /// <param name="activator">The activator.</param>
        /// <param name="targetContext">The target context.</param>
        /// <returns></returns>
        public static IConcreteRegistrar RegisterInContext<TComponent, TTag>(
            this ContainerBuilder builder,
            ComponentActivatorWithParameters<TComponent> activator,
            TTag targetContext)
        {
            if (builder == null)
                throw new ArgumentNullException("builder");

            if (activator == null)
                throw new ArgumentNullException("activator");

            // Use a unique service name because the caller may change the way they
            // identify the component.
            var uniqueService = new UniqueService();

            return builder.Register(new ComponentActivatorWithParameters<TComponent>((c, p) =>
            {
                var ct = c.Resolve<ContextTag<TTag>>();
                if (ct.HasTag && object.Equals(ct.Tag, targetContext))
                {
                    return (TComponent)activator(c, p);
                }
                else
                {
                    var container = c.Resolve<IContainer>();
                    if (container.OuterContainer != null)
                        return (TComponent)container.OuterContainer.Resolve(uniqueService, MakeParameters(p));
                    else
                        throw new DependencyResolutionException(string.Format(CultureInfo.CurrentCulture,
                            ContainerBuilderExtensionsResources.TaggedContextNotFound,
                            targetContext));
                }
            }))
            .As(new TypedService(typeof(TComponent)), uniqueService)
            .WithScope(InstanceScope.Container);
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
        /// <returns></returns>
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

        private static Parameter[] MakeParameters(IActivationParameters p)
        {
            if (p == null)
                throw new ArgumentNullException("p");

            if (p.Count == 0)
                return EmptyParameters;

            return p.Select(kvp => new Parameter(kvp.Key, kvp.Value)).ToArray();
        }
    }
}
