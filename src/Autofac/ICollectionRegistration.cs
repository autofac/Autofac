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
using System.Text;

namespace Autofac
{
    /// <summary>
    /// A collection registration is a component registration that allows the
    /// same service type to be registered multiple times and all of these
    /// registrations to be exposed through the collection registration
    /// in various collection classes.
    /// </summary>
    /// <remarks>
    /// Once a collection registration has been made, subsequent registrations
    /// of the ItemType service will be added to the collection registration rather
    /// than overriding the registration for that service (if it exists.)
    /// This is useful for implementing plug-in style systems without resorting
    /// to lists of named components (and in the spirit of taking autowiring as
    /// far as possible.)
    /// </remarks>
    public interface ICollectionRegistration : IComponentRegistration
    {
        /// <summary>
        /// The service type exposed by the individual items in the collection.
        /// </summary>
        Type ItemType { get; }

        /// <summary>
        /// Used by the container to add subsequent component registrations
        /// exposing the ItemType service to the collection.
        /// </summary>
        /// <param name="item">The component registration to add to the
        /// collection. Required.</param>
        void Add(IComponentRegistration item);
    }
}
 