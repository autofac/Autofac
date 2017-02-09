using System;
using System.Linq;
using Autofac.Core;

namespace Autofac.Builder
{
    /// <summary>
    /// Reference object allowing location and update of a registration callback.
    /// </summary>
    public class DeferredCallback
    {
        private Action<IComponentRegistry> _callback;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeferredCallback"/> class.
        /// </summary>
        /// <param name="callback">
        /// An <see cref="Action{T}"/> that executes a registration action
        /// against an <see cref="IComponentRegistry"/>.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="callback" /> is <see langword="null" />.
        /// </exception>
        public DeferredCallback(Action<IComponentRegistry> callback)
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
        /// against an <see cref="IComponentRegistry"/>.
        /// </value>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="value" /> is <see langword="null" />.
        /// </exception>
        public Action<IComponentRegistry> Callback
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
