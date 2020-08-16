// This software is part of the Autofac IoC container
// Copyright © 2019 Autofac Contributors
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
using System.Linq;

namespace Autofac.Builder
{
    /// <summary>
    /// Represent a collection of callbacks that can be run after a container or scope is 'built'.
    /// </summary>
    internal class BuildCallbackService
    {
        private List<Action<ILifetimeScope>> _callbacks = new List<Action<ILifetimeScope>>();

        /// <summary>
        /// Add a callback to the set that will get executed.
        /// </summary>
        /// <param name="callback">The callback to run.</param>
        public void AddCallback(Action<ILifetimeScope> callback)
        {
            if (callback == null)
            {
                throw new ArgumentNullException(nameof(callback));
            }

            _callbacks.Add(callback);
        }

        /// <summary>
        /// Execute the callback for each build callback registered in this service.
        /// </summary>
        /// <param name="scope">The scope that has been built.</param>
        public void Execute(ILifetimeScope scope)
        {
            if (scope == null)
            {
                throw new ArgumentNullException(nameof(scope));
            }

            if (_callbacks == null)
            {
                throw new InvalidOperationException(BuildCallbackServiceResources.BuildCallbacksAlreadyRun);
            }

            try
            {
                foreach (var callback in _callbacks!)
                {
                    callback(scope);
                }
            }
            finally
            {
                // Clear the reference to the callbacks to release any held scopes (and function as a do-not-run flag)
                // This object will be a singleton instance in the container/scope, so the initial function scopes that
                // define the callbacks could be holding onto closure resources. We want the GC to take that back.
                _callbacks = null!;
            }
        }
    }
}
