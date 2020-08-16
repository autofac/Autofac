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
using Autofac.Core.Resolving.Pipeline;

namespace Autofac.Core.Resolving
{
    /// <summary>
    /// Fired when a resolve request is starting.
    /// </summary>
    public class ResolveRequestCompletingEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ResolveRequestCompletingEventArgs"/> class.
        /// </summary>
        /// <param name="requestContext">The resolve request context that is completing.</param>
        public ResolveRequestCompletingEventArgs(ResolveRequestContext requestContext)
        {
            if (requestContext is null)
            {
                throw new ArgumentNullException(nameof(requestContext));
            }

            RequestContext = requestContext;
        }

        /// <summary>
        /// Gets the instance lookup operation that is beginning.
        /// </summary>
        public ResolveRequestContext RequestContext { get; }
    }
}
