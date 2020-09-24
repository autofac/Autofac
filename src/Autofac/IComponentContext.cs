// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac.Core;
using Autofac.Core.Registration;

namespace Autofac
{
    /// <summary>
    /// The context in which a service can be accessed or a component's
    /// dependencies resolved. Disposal of a context will dispose any owned
    /// components.
    /// </summary>
    public interface IComponentContext
    {
        /// <summary>
        /// Gets the associated services with the components that provide them.
        /// </summary>
        IComponentRegistry ComponentRegistry { get; }

        /// <summary>
        /// Resolve an instance of the provided registration within the context.
        /// </summary>
        /// <param name="request">The resolve request.</param>
        /// <returns>
        /// The component instance.
        /// </returns>
        /// <exception cref="ComponentNotRegisteredException"/>
        /// <exception cref="DependencyResolutionException"/>
        object ResolveComponent(ResolveRequest request);
    }
}
