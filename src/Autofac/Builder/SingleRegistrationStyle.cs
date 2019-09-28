// This software is part of the Autofac IoC container
// Copyright © 2011 Autofac Contributors
// https://autofac.org
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
using Autofac.Core;

namespace Autofac.Builder
{
    /// <summary>
    /// Registration style for individual components.
    /// </summary>
    public class SingleRegistrationStyle
    {
        /// <summary>
        /// Gets or sets the ID used for the registration.
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Gets the handlers to notify of the component registration event.
        /// </summary>
        public ICollection<EventHandler<ComponentRegisteredEventArgs>> RegisteredHandlers { get; } = new List<EventHandler<ComponentRegisteredEventArgs>>();

        /// <summary>
        /// Gets or sets a value indicating whether default registrations should be preserved.
        /// By default, new registrations override existing registrations as defaults.
        /// If set to true, new registrations will not change existing defaults.
        /// </summary>
        public bool PreserveDefaults { get; set; }

        /// <summary>
        /// Gets or sets the component upon which this registration is based.
        /// </summary>
        public IComponentRegistration Target { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the registration is a 1:1 adapters on top
        /// of another component (e.g., Meta, Func, or Owned).
        /// </summary>
        public bool IsAdapterForIndividualComponent { get; set; }
    }
}
