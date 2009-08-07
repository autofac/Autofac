using System;
using Autofac;
using Microsoft.Practices.Composite;

namespace AutofacContrib.Prism
{
    /// <summary>
    /// Defines a <seealso cref="IAutofacContainer"/> adapter for
    /// the <see cref="IContainerFacade"/> interface
    /// to be used by the Composite Application Library.
    /// </summary>
    class AutofacContainerAdapter : IContainerFacade
    {
        private readonly IContainer _autofacContainer;

        /// <summary>
        /// Initializes a new instance of <see cref="AutofacContainerAdapter"/>.
        /// </summary>
        /// <param name="AutofacContainer">The <seealso cref="IAutofacContainer"/> that will be used
        /// by the <see cref="Resolve"/> and <see cref="TryResolve"/> methods.</param>
        public AutofacContainerAdapter(IContainer AutofacContainer)
        {
            _autofacContainer = AutofacContainer;
        }

        /// <summary>
        /// Resolve an instance of the requested type from the container.
        /// </summary>
        /// <param name="type">The type of object to get from the container.</param>
        /// <returns>An instance of <paramref name="type"/>.</returns>
        /// <exception cref="ResolutionFailedException"><paramref name="type"/> cannot be resolved by the container.</exception>
        public object Resolve(Type type)
        {
            return _autofacContainer.Resolve(type);
        }

        /// <summary>
        /// Tries to resolve an instance of the requested type from the container.
        /// </summary>
        /// <param name="type">The type of object to get from the container.</param>
        /// <returns>
        /// An instance of <paramref name="type"/>. 
        /// If the type cannot be resolved it will return a <see langword="null"/> value.
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public object TryResolve(Type type)
        {
            object resolved;

            _autofacContainer.TryResolve(type, out resolved);

            return resolved;
        }
    }
}