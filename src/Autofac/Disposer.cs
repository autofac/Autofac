using System;
using System.Collections.Generic;
using System.Text;

namespace Autofac
{
    class Disposer : Disposable, IDisposer
    {
        /// <summary>
        /// Contents all implement IDisposable.
        /// </summary>
        Stack<WeakReference> _items = new Stack<WeakReference>();

        object _synchRoot = new object();

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
                lock (_synchRoot)
                    while (_items.Count > 0)
                    {
                        WeakReference reference = _items.Pop();
                        IDisposable item = (IDisposable)reference.Target;
                        if (reference.IsAlive)
                            item.Dispose();
                    }
        }

        public void AddInstanceForDisposal(IDisposable instance)
        {
            Enforce.ArgumentNotNull(instance, "instance");
            CheckNotDisposed();

            lock (_synchRoot)
                _items.Push(new WeakReference(instance));
        }
    }
}
