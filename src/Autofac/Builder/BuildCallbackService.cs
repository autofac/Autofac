// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

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
