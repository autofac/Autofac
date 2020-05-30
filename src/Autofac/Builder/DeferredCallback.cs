using System;
using Autofac.Core.Registration;

namespace Autofac.Builder
{
    /// <summary>
    /// Reference object allowing location and update of a registration callback.
    /// </summary>
    public class DeferredCallback
    {
        // _callback set to default! to get around initialization detection problem in roslyn.
        private Action<IComponentRegistryBuilder> _callback = default!;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeferredCallback"/> class.
        /// </summary>
        /// <param name="callback">
        /// An <see cref="Action{T}"/> that executes a registration action
        /// against an <see cref="IComponentRegistryBuilder"/>.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="callback" /> is <see langword="null" />.
        /// </exception>
        public DeferredCallback(Action<IComponentRegistryBuilder> callback)
        {
            if (callback == null)
            {
                throw new ArgumentNullException(nameof(callback));
            }

            this.Id = Guid.NewGuid();
            this.Callback = callback;
        }

        /// <summary>
        /// Gets or sets the callback to execute during registration.
        /// </summary>
        /// <value>
        /// An <see cref="Action{T}"/> that executes a registration action
        /// against an <see cref="IComponentRegistryBuilder"/>.
        /// </value>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="value" /> is <see langword="null" />.
        /// </exception>
        public Action<IComponentRegistryBuilder> Callback
        {
            get
            {
                return this._callback;
            }

            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }

                this._callback = value;
            }
        }

        /// <summary>
        /// Gets the callback identifier.
        /// </summary>
        /// <value>
        /// A <see cref="Guid"/> that uniquely identifies the callback action
        /// in a set of callbacks.
        /// </value>
        public Guid Id { get; }
    }
}
