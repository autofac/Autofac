// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading;

namespace Autofac.Util
{
    /// <summary>
    /// Provides access to a unique sequenced number.
    /// </summary>
    internal static class SequenceGenerator
    {
        private static long _lastSequence;

        /// <summary>
        /// Get the next unique sequence value.
        /// </summary>
        /// <returns>A new sequence value.</returns>
        internal static long GetNextUniqueSequence()
        {
            while (true)
            {
                var last = Interlocked.Read(ref _lastSequence);
                var next = DateTime.UtcNow.Ticks;
                if (next <= last)
                {
                    next = last + 1;
                }

                var replaced = Interlocked.CompareExchange(ref _lastSequence, next, last);
                if (replaced == last)
                {
                    return next;
                }
            }
        }
    }
}
