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

namespace Autofac
{
    /// <summary>
    /// Registered in the container to support tagged extensions. Stores the
    /// tag for a single container, and allows traversal of the context tree.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    class ContextTag<T>
    {
        bool _hasTag;
        T _tag;

        /// <summary>
        /// The tag for the current context. Can only be accessed when HasTag is true.
        /// </summary>
        public T Tag
        {
            get
            {
                if (!_hasTag)
                    throw new InvalidOperationException(ContextTagResources.ContextNotTagged);
                return _tag;
            }
            set
            {
                _tag = value;
                _hasTag = true;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance has a tag.
        /// </summary>
        /// <value><c>true</c> if this instance has a tag; otherwise, <c>false</c>.</value>
        public bool HasTag
        {
            get
            {
                return _hasTag;
            }
        }
    }
}
