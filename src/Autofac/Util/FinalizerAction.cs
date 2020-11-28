using System;

namespace Autofac.Util
{
    /// <summary>
    /// This class used for registering Action delegates to be run when GC runs finalizers.
    /// </summary>
    public class FinalizerAction
    {
        private readonly Action _action;

        /// <summary>
        /// Initializes a new instance of the <see cref="FinalizerAction"/> class.
        /// </summary>
        /// <param name="action">Action to run.</param>
        public FinalizerAction(Action action)
        {
            _action = action;
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="FinalizerAction"/> class.
        /// </summary>
        ~FinalizerAction() => _action();

        /// <summary>
        /// Registers an action to be run during finalizer.
        /// </summary>
        /// <param name="action">Action to run.</param>
        public static void Register(Action action)
        {
            _ = new FinalizerAction(action);
        }
    }
}
