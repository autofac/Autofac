using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
        /// <param name="undoer"></param>
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
