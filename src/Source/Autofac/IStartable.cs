using System;

namespace Autofac
{
    /// <summary>
    /// When implemented by a component, an instance of the component will be resolved
    /// and started as soon as the container is built. Autofac will not call the Start()
    /// method when subsequent instances are resolved. If this behaviour is required, use
    /// an <code>OnActivated()</code> event handler instead.
    /// </summary>
    /// <remarks>
    /// For equivalent "Stop" functionality, implement <see cref="IDisposable"/>. Autofac
    /// will always dispose a component before any of its dependencies.
    /// </remarks>
    public interface IStartable
    {
        /// <summary>
        /// Perform once-off startup processing.
        /// </summary>
        void Start();
    }
}
