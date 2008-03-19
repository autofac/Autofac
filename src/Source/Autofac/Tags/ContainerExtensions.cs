// Contributed by Nicholas Blumhardt 2008-02-10
// Copyright (c) 2007 - 2008 Autofac Contributors
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
using Autofac.Component;
using Autofac.Component.Activation;
using Autofac.Component.Scope;
using System.Collections.Generic;

namespace Autofac.Tags
{
    /// <summary>
    /// Extends the container so that it and inner containers can be tagged according to a
    /// hierarchy, and component registrations made to resolve in one level of the hierarchy
    /// only.
    /// </summary>
    public static class ContainerExtensions
    {
        /// <summary>
        /// Enables context tagging in the target container.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="container">The container.</param>
        /// <param name="tag">The tag applied to this container and the contexts genrated when
        /// it resolves component dependencies.</param>
        public static void TagContext<T>(this IContainer container, T tag)
        {
            if (container == null)
                throw new ArgumentNullException("container");

            if (!container.IsRegistered<ContextTag<T>>())
            {
                container.RegisterComponent(
                    new Registration(
                        new Descriptor(
                            new UniqueService(),
                            new[] { new TypedService(typeof(ContextTag<T>)) },
                            typeof(ContextTag<T>)),
                        new DelegateActivator((c, p) => new ContextTag<T>()),
                        new ContainerScope(),
                        InstanceOwnership.Container));
            }

            container.Resolve<ContextTag<T>>().Tag = tag;
        }
    }
}
