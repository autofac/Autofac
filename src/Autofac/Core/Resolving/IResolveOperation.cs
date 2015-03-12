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
    /// An <see cref="IResolveOperation"/> is a component context that sequences and monitors the multiple
    /// activations that go into producing a single requested object graph.
    /// </summary>
    public interface IResolveOperation
    {
        /// <summary>
        /// Get or create and share an instance of <paramref name="registration"/> in the <paramref name="currentOperationScope"/>.
        /// </summary>
        /// <param name="currentOperationScope">The scope in the hierarchy in which the operation will begin.</param>
        /// <param name="registration">The component to resolve.</param>
        /// <param name="parameters">Parameters for the component.</param>
        /// <returns>The component instance.</returns>
        object GetOrCreateInstance(ISharingLifetimeScope currentOperationScope, IComponentRegistration registration, IEnumerable<Parameter> parameters);

        /// <summary>
        /// Raised when the entire operation is complete.
        /// </summary>
        event EventHandler<ResolveOperationEndingEventArgs> CurrentOperationEnding;

        /// <summary>
        /// Raised when an instance is looked up within the operation.
        /// </summary>
        event EventHandler<InstanceLookupBeginningEventArgs> InstanceLookupBeginning;
    }
}
