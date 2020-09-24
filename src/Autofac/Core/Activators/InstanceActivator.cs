// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Runtime.CompilerServices;
using Autofac.Util;

namespace Autofac.Core.Activators
{
    /// <summary>
    /// Base class for instance activators.
    /// </summary>
    public abstract class InstanceActivator : Disposable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InstanceActivator"/> class.
        /// </summary>
        /// <param name="limitType">Most derived type to which instances can be cast.</param>
        protected InstanceActivator(Type limitType)
        {
            LimitType = limitType ?? throw new ArgumentNullException(nameof(limitType));
        }

        /// <summary>
        /// Gets the most specific type that the component instances are known to be castable to.
        /// </summary>
        public Type LimitType { get; }

        /// <summary>
        /// Gets a string representation of the activator.
        /// </summary>
        /// <returns>A string describing the activator.</returns>
        public override string ToString()
        {
            return LimitType.Name + " (" + GetType().Name + ")";
        }

        /// <summary>
        /// Asserts that the activator has not been disposed.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void CheckNotDisposed()
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(InstanceActivatorResources.InstanceActivatorDisposed, innerException: null);
            }
        }
    }
}
