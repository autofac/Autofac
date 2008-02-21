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
using Autofac.Component.Activation;
using Autofac.Component.Scope;
using Autofac.Component;

namespace Autofac.TaggedContexts
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
        /// <param name="rootTag">The tag applied to this container.</param>
        /// <param name="defaultTag">The default tag applied to new contexts.</param>
        public static void EnableTaggedContexts<T>(this IContainer container, T rootTag, T defaultTag)
        {
            if (container == null)
                throw new ArgumentNullException("container");

            container.RegisterComponent(
                new Registration(
                    new[] { new TypedService(typeof(ContextTag<T>)) },
                    new DelegateActivator((c, p) => new ContextTag<T>() { Tag = defaultTag }),
                    new ContainerScope()));

            container.Resolve<ContextTag<T>>().Tag = rootTag;
        }

        /// <summary>
        /// Enables context tagging in the target container.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="container">The container.</param>
        /// <param name="rootTag">The tag applied to this container.</param>
        public static void EnableTaggedContexts<T>(this IContainer container, T rootTag)
        {
            EnableTaggedContexts<T>(container, rootTag, default(T));
        }

        /// <summary>
        /// Creates an inner container with the provided tag.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="outer">The outer container.</param>
        /// <param name="tag">The tag.</param>
        /// <returns>A new inner container.</returns>
        /// <exception cref="Autofac.ComponentNotRegisteredException">Tags are not enabled for the
        /// outer container.</exception>
        public static IContainer CreateTaggedInnerContainer<T>(this IContainer outer, T tag)
        {
            if (outer == null)
                throw new ArgumentNullException("outer");

            var inner = outer.CreateInnerContainer();
            
            var contextTag = inner.Resolve<ContextTag<T>>();
            contextTag.Parent = outer;
            contextTag.Tag = tag;
            
            return inner;
        }
    }
}
