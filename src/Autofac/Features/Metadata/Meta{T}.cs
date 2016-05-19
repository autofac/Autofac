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

using System.Collections.Generic;

namespace Autofac.Features.Metadata
{
    /// <summary>
    /// Provides a value along with a dictionary of metadata describing the value.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    public class Meta<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Meta{T}"/> class.
        /// </summary>
        /// <param name="value">The value described by the instance.</param>
        /// <param name="metadata">The metadata describing the value.</param>
        public Meta(T value, IDictionary<string, object> metadata)
        {
            Value = value;
            Metadata = metadata;
        }

        /// <summary>
        /// Gets the value described by <see cref="Metadata"/>.
        /// </summary>
        public T Value { get; }

        /// <summary>
        /// Gets the metadata describing the value.
        /// </summary>
        public IDictionary<string, object> Metadata { get; }
    }
}
