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

using System.Collections.Generic;
using System;

namespace Autofac
{
    /// <summary>
    /// Captures the elements of a component registration that are shared between
    /// all contexts in which the component can be used.
    /// </summary>
    public interface IComponentDescriptor
    {
        /// <summary>
        /// The services (named and typed) exposed by the component.
        /// </summary>
        IEnumerable<Service> Services { get; }

        /// <summary>
        /// A unique identifier for this component (shared in all sub-contexts.)
        /// This value also appears in Services.
        /// </summary>
        Service Id { get; }

        /// <summary>
        /// Additional data associated with the component.
        /// </summary>
        /// <remarks>Note, component registrations are currently copied into
        /// subcontainers: these properties are shared between all instances of the
        /// registration in all subcontainers.</remarks>
        IDictionary<string, object> ExtendedProperties { get; }

        /// <summary>
        /// For registrations that can determine a single implementation
        /// type (or most generic implementation type in a hierarchy) this
        /// method will return true and provide the type through the
        /// <paramref name="implementationType"/> parameter. For registrations
        /// where the implementation type cannot be determined in advance, it
        /// is recommended that the returned instances are inspected as they
        /// are activated.
        /// </summary>
        /// <param name="implementationType">The implementation type.</param>
        /// <returns>True if an implementation type is known.</returns>
        bool KnownImplementationType(out Type implementationType);
    }
}
