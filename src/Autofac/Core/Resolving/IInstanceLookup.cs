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
using System.Collections.Generic;

namespace Autofac.Core.Resolving
{
    /// <summary>
    /// Represents the process of finding a component during a resolve operation.
    /// </summary>
    public interface IInstanceLookup
    {
        /// <summary>
        /// Gets the component for which an instance is to be looked up.
        /// </summary>
        IComponentRegistration ComponentRegistration { get; }

        /// <summary>
        /// Gets the scope in which the instance will be looked up.
        /// </summary>
        ILifetimeScope ActivationScope { get; }

        /// <summary>
        /// Gets the parameters provided for new instance creation.
        /// </summary>
        IEnumerable<Parameter> Parameters { get; }

        /// <summary>
        /// Raised when the lookup phase of the operation is ending.
        /// </summary>
        event EventHandler<InstanceLookupEndingEventArgs> InstanceLookupEnding;

        /// <summary>
        /// Raised when the completion phase of an instance lookup operation begins.
        /// </summary>
        event EventHandler<InstanceLookupCompletionBeginningEventArgs> CompletionBeginning;

        /// <summary>
        /// Raised when the completion phase of an instance lookup operation ends.
        /// </summary>
        event EventHandler<InstanceLookupCompletionEndingEventArgs> CompletionEnding;
    }
}