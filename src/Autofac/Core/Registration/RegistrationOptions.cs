// This software is part of the Autofac IoC container
// Copyright © 2020 Autofac Contributors
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

namespace Autofac.Core.Registration
{
    /// <summary>
    /// Defines options for a registration.
    /// </summary>
    [Flags]
    public enum RegistrationOptions
    {
        /// <summary>
        /// No special options; default behaviour.
        /// </summary>
        None = 0,

        /// <summary>
        /// Indicates that this registration is 'fixed' as the default, ignoring all other registrations when determining the default registration for
        /// a service.
        /// </summary>
        Fixed = 2,

        /// <summary>
        /// Registrations with this flag will not be decorated.
        /// </summary>
        DisableDecoration = 4,

        /// <summary>
        /// Registrations with this flag will not be included in any collection resolves (i.e. <see cref="IEnumerable{TService}" /> and other collection types).
        /// </summary>
        ExcludeFromCollections = 8,

        /// <summary>
        /// Flag combination for composite registrations.
        /// </summary>
        Composite = Fixed | DisableDecoration | ExcludeFromCollections,
    }
}
