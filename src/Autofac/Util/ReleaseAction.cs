// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace Autofac.Util
{
    /// <summary>
    /// Adapts an action to the <see cref="IDisposable"/> interface.
    /// </summary>
    internal class ReleaseAction<TLimit> : Disposable
    {
        private readonly Action<TLimit> _action;
        private readonly Func<TLimit> _factory;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReleaseAction{TLimit}"/> class.
        /// </summary>
        /// <param name="action">
        /// The action to execute on disposal.
        /// </param>
        /// <param name="factory">
        /// A factory that retrieves the value on which the <paramref name="action" />
        /// should be executed.
        /// </param>
        public ReleaseAction(Action<TLimit> action, Func<TLimit> factory)
        {
            _action = action ?? throw new ArgumentNullException(nameof(action));
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            // Value retrieval for the disposal is deferred until
            // disposal runs to ensure any calls to, say, .ReplaceInstance()
            // during .OnActivating() will be accounted for.
            if (disposing)
            {
                _action(_factory());
            }

            base.Dispose(disposing);
        }
    }
}
