// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Autofac.Builder;

namespace Autofac
{
    /// <summary>
    /// When implemented by a component, an instance of the component will be resolved
    /// and started as soon as the container is built. Autofac will not call the Start()
    /// method when subsequent instances are resolved. If this behavior is required, use
    /// an <c>OnActivated()</c> event handler instead.
    /// </summary>
    /// <remarks>
    /// <para>
    /// For equivalent "Stop" functionality, implement <see cref="IDisposable"/>. Autofac
    /// will always dispose a component before any of its dependencies (except in the presence
    /// of circular dependencies, in which case the components in the cycle are disposed in
    /// reverse-construction order.)
    /// </para>
    ///
    /// <para>
    /// Components that implement <see cref="IStartable"/> and configured with a InstancePerLifetimeScope()
    /// lifetime are still only started *once*, in the root <see cref="IContainer"/> or <see cref="ILifetimeScope"/>
    /// where the component was registered.
    /// </para>
    /// </remarks>
    public interface IStartable
    {
        /// <summary>
        /// Perform once-off startup processing.
        /// </summary>
        void Start();
    }
}
