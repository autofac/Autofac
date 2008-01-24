using System;
using System.Collections.Generic;
using System.Text;

namespace Autofac
{
	/// <summary>
	/// Base class for disposable objects.
	/// </summary>
	public class Disposable : IDisposable
	{
		bool _isDisposed;
        object _synchRoot = new object();

		public void Dispose()
		{
            lock (_synchRoot)
            {
                if (!_isDisposed)
                {
                    _isDisposed = true;
                    Dispose(true);
                    GC.SuppressFinalize(this);
                }
            }
		}

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
		protected virtual void Dispose(bool disposing)
		{
		}

        /// <summary>
        /// Gets a value indicating whether this instance is disposed.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is disposed; otherwise, <c>false</c>.
        /// </value>
		protected bool IsDisposed
		{
			get
			{
				return _isDisposed;
			}
		}

        /// <summary>
        /// Checks that this instance has not been disposed.
        /// </summary>
		protected virtual void CheckNotDisposed()
		{
			if (IsDisposed)
				throw new ObjectDisposedException(GetType().Name);
		}
	}
}
