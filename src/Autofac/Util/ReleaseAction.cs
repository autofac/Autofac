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
            if (action == null) throw new ArgumentNullException(nameof(action));
            if (factory == null) throw new ArgumentNullException(nameof(factory));

            _action = action;
            _factory = factory;
        }

        protected override void Dispose(bool disposing)
        {
            // Value retrieval for the disposal is deferred until
            // disposal runs to ensure any calls to, say, .ReplaceInstance()
            // during .OnActivating() will be accounted for.
            if (disposing)
                _action(this._factory());

            base.Dispose(disposing);
        }
    }
}
