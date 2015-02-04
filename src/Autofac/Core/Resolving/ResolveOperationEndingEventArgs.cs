// This software is part of the Autofac IoC container
// Copyright © 2011 Autofac Contributors
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

namespace Autofac.Core.Resolving
{
    /// <summary>
    /// Describes the commencement of a new resolve operation.
    /// </summary>
    public class ResolveOperationEndingEventArgs : EventArgs
    {
        readonly IResolveOperation _resolveOperation;
        readonly Exception _exception;

        /// <summary>
        /// Create an instance of the <see cref="ResolveOperationBeginningEventArgs"/> class.
        /// </summary>
        /// <param name="resolveOperation">The resolve operation that is ending.</param>
        /// <param name="exception">If included, the exception causing the operation to end; otherwise, null.</param>
        public ResolveOperationEndingEventArgs(IResolveOperation resolveOperation, Exception exception = null)
        {
            _resolveOperation = resolveOperation;
            _exception = exception;
        }

        /// <summary>
        /// The exception causing the operation to end, or null.
        /// </summary>
        public Exception Exception
        {
            get { return _exception; }
        }

        /// <summary>
        /// The resolve operation that is ending.
        /// </summary>
        public IResolveOperation ResolveOperation
        {
            get { return _resolveOperation; }
        }
    }
}