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

using System;

namespace Autofac.Component
{
    /// <summary>
    /// Maintains an association between instances and
    /// their activation scope. This can represent per-call,
    /// global, thread-based, request-based or user-based scope etc.
    /// It is an error to call
    /// GetInstance() if there is no instance available. Likewise,
    /// it is an error to call SetInstance() when an instance
    /// already exists in the current scope.
    /// An activation scope is also responsible for disposing
    /// its managed instances when it is itself disposed.
    /// </summary>
    /// <remarks>
    /// Note that obviously an Instance property could be used
    /// to mimic the same behaviour, however the semantics
    /// would differ from that of a regular property.
    /// </remarks>
    public interface IScope
    {
        /// <summary>
        /// Returns true if there is already an instance available
        /// in the current activation scope.
        /// </summary>
        bool InstanceAvailable { get; }

        /// <summary>
        /// The instance corresponding to this scope.
        /// </summary>
        /// <returns>The instance.</returns>
        /// <exception cref="InvalidOperationException">
        /// There is not instance available.</exception>
        object GetInstance();

        /// <summary>
        /// Sets the instance to be associated with the
        /// current activation scope.
        /// </summary>
        /// <param name="instance">The instance.</param>
        /// <exception cref="InvalidOperationException">There is already an instance available.</exception>
        /// <exception cref="ArgumentNullException"></exception>
        void SetInstance(object instance);

        /// <summary>
        /// Try to create a scope container for a new context.
        /// </summary>
        /// <param name="newScope">The duplicate.</param>
        /// <returns>True if the semantics of the scope model allow for new contexts.</returns>
		bool DuplicateForNewContext(out IScope newScope);
	}
}
