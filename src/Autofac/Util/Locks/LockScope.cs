// This software is part of the Autofac IoC container
// Copyright © 2011 Autofac Contributors
// http://autofac.org
//
// this code is contribution from https://www.nuget.org/packages/Bnaya.CSharp.AsyncExtensions/

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace System.Threading.Tasks
{
    /// <summary>
    /// Locking scope which can free the lock using Dispose and indicate whether lock is acquired
    /// </summary>
    public sealed class LockScope : IDisposable
    {
        private SemaphoreSlim _gate;

        /// <summary>
        /// Initializes a new instance of the <see cref="LockScope"/> class.
        /// </summary>
        /// <param name="gate">The gate.</param>
        /// <param name="acquired">if set to <c>true</c> [acquired].</param>
        internal LockScope(SemaphoreSlim gate, bool acquired)
        {
            _gate = gate;
            Acquired = acquired;
        }

        /// <summary>
        /// Gets a value indicating whether the lock is acquired.
        /// </summary>
        public bool Acquired { get; }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Dispose can be invoked once</exception>
        public void Dispose()
        {
#pragma warning disable CA1065 // Do not raise exceptions in unexpected locations
            if (_gate == null)
                throw new ObjectDisposedException("Dispose can be invoked once");
#pragma warning restore CA1065 // Do not raise exceptions in unexpected locations

            if (Acquired)
            {
                _gate?.Release();
            }

            _gate = null;
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="LockScope"/> class.
        /// </summary>
        ~LockScope()
        {
            Dispose();
        }
    }
}
