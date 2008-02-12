// This software is part of the Autofac IoC container
// Copyright (c) 2007 Nicholas Blumhardt
// nicholas.blumhardt@gmail.com
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

namespace Autofac
{
    /// <summary>
    /// An UndoAction cleans up when a guard is not dismissed (e.g. in the
    /// event of an exception.)
    /// </summary>
    delegate void UndoAction();

    /// <summary>
    /// A Guard stores a stack of <see cref="UndoAction"/>s that it will execute
    /// if it is disposed before it is dismissed.
    /// </summary>
    /// <example>
    /// var i = 2;
    /// using (var guard = new Guard())
    /// {
    ///   var oldI = i;
    ///   guard.Add(() =&gt; i = oldI);
    ///   
    ///   i = 3;
    ///   
    ///   // guard.Dismiss() should be called here
    /// }
    /// 
    /// // i will be reset to 2 by the guard.
    /// </example>
    class Guard : Disposable
    {
        bool _dismissed;
        Stack<UndoAction> _undoActions = new Stack<UndoAction>();

        /// <summary>
        /// Create a guard with an empty undo stack.
        /// </summary>
        public Guard()
        {
        }

        /// <summary>
        /// Create a guard with an initial undo action.
        /// </summary>
        /// <param name="undoAction">The undo action.</param>
        public Guard(UndoAction undoAction)
            : this()
        {
            if (undoAction == null)
                throw new ArgumentNullException("undoAction");

            Add(undoAction);
        }

        /// <summary>
        /// Dismiss the guard, so that it can be disposed
        /// without the undo actions being executed.
        /// </summary>
        public void Dismiss()
        {
            CheckNotDisposedOrDismissed();
            _dismissed = true;
        }

        /// <summary>
        /// Add an action to the undo stack.
        /// </summary>
        /// <param name="undoAction">The action to perform in the event of an undo.</param>
        public void Add(UndoAction undoAction)
        {
            if (undoAction == null)
                throw new ArgumentNullException("undoAction");

            CheckNotDisposedOrDismissed();

            _undoActions.Push(undoAction);
        }

        private void CheckNotDisposedOrDismissed()
        {
            if (_dismissed)
                throw new InvalidOperationException(GuardResources.AlreadyDismissed);

            if (IsDisposed)
                throw new ObjectDisposedException("Guard");
        }

        /// <summary>
        /// Dispose of managed and unmanaged resources.
        /// </summary>
        /// <param name="disposing">If false, this object is being garbage collected.</param>
        protected override void Dispose(bool disposing)
        {
            if (!_dismissed)
            {
                while (_undoActions.Count != 0)
                    _undoActions.Pop()();
            }
        }
    }
}
