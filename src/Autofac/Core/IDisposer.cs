using System;

namespace Autofac.Core
{
    /// <summary>
    /// Provided on an object that will dispose of other objects when it is
    /// itself disposed.
    /// </summary>
    public interface IDisposer : IDisposable
    {
        /// <summary>
        /// Adds an object to the disposer. When the disposer is
        /// disposed, so will the object be.
        /// </summary>
        /// <param name="instance">The instance.</param>
        void AddInstanceForDisposal(IDisposable instance);
    }
}
