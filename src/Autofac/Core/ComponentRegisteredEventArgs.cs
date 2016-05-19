// This software is part of the Autofac IoC container
// Copyright � 2011 Autofac Contributors
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

namespace Autofac.Core
{
    /// <summary>
    /// Information about the ocurrence of a component being registered
    /// with a container.
    /// </summary>
    public class ComponentRegisteredEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the container into which the registration was made.
        /// </summary>
        public IComponentRegistry ComponentRegistry { get; }

        /// <summary>
        /// Gets the component registration.
        /// </summary>
        public IComponentRegistration ComponentRegistration { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ComponentRegisteredEventArgs"/> class.
        /// </summary>
        /// <param name="registry">The container into which the registration
        /// was made.</param>
        /// <param name="componentRegistration">The component registration.</param>
        public ComponentRegisteredEventArgs(IComponentRegistry registry, IComponentRegistration componentRegistration)
        {
            if (registry == null) throw new ArgumentNullException(nameof(registry));
            if (componentRegistration == null) throw new ArgumentNullException(nameof(componentRegistration));

            ComponentRegistry = registry;
            ComponentRegistration = componentRegistration;
        }
    }
}
