// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Autofac.Core
{
    /// <summary>
    /// Fired when the activation process for a new instance is complete.
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix")]
    public interface IActivatedEventArgs<out T>
    {
        /// <summary>
        /// Gets the service being resolved.
        /// </summary>
        Service Service { get; }

        /// <summary>
        /// Gets the context in which the activation occurred.
        /// </summary>
        IComponentContext Context { get; }

        /// <summary>
        /// Gets the component providing the instance.
        /// </summary>
        IComponentRegistration Component { get; }

        /// <summary>
        /// Gets the paramters provided when resolved.
        /// </summary>
        IEnumerable<Parameter> Parameters { get; }

        /// <summary>
        /// Gets the instance that will be used to satisfy the request.
        /// </summary>
        T Instance { get; }
    }
}
