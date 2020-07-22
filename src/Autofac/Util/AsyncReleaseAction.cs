// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading.Tasks;

namespace Autofac.Util
{
    /// <summary>
    /// Adapts an async action to the <see cref="IAsyncDisposable"/> interface.
    /// </summary>
    internal class AsyncReleaseAction<TLimit> : Disposable
    {
        private readonly Func<TLimit, Task> _action;
        private readonly Func<TLimit> _factory;

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncReleaseAction{TLimit}"/> class.
        /// </summary>
        /// <param name="action">
        /// The async action to execute on disposal.
        /// </param>
        /// <param name="factory">
        /// A factory that retrieves the value on which the <paramref name="action" />
        /// should be executed.
        /// </param>
        public AsyncReleaseAction(Func<TLimit, Task> action, Func<TLimit> factory)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
            if (factory == null) throw new ArgumentNullException(nameof(factory));

            _action = action;
            _factory = factory;
        }

        /// <inheritdoc/>
        protected override async ValueTask DisposeAsync(bool disposing)
        {
            // Value retrieval for the disposal is deferred until
            // disposal runs to ensure any calls to, say, .ReplaceInstance()
            // during .OnActivating() will be accounted for.
            if (disposing)
            {
              await _action(_factory()).ConfigureAwait(false);
            }

            base.Dispose(disposing);
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            // Value retrieval for the disposal is deferred until
            // disposal runs to ensure any calls to, say, .ReplaceInstance()
            // during .OnActivating() will be accounted for.
            if (disposing)
            {
                _action(_factory()).ConfigureAwait(false).GetAwaiter().GetResult();
            }

            base.Dispose(disposing);
        }
    }
}
