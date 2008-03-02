// This software is part of the Autofac IoC container
// Copyright (c) 2007 Nicholas Blumhardt
// nicholas.blumhardt@gmail.com
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.

using System;
using System.Collections.Generic;

namespace Autofac
{
    /// <summary>
    /// Provides the full Autofac container functionality.
    /// </summary>
    /// <remarks>
    /// It is recommended that in most instances the more limited IContext interface is 
    /// used instead, as this is easier to implement on top of a different back-end, e.g. a
    /// customised or alternative container.
    /// </remarks>
    public interface IContainer : IContext, IDisposable
    {
        /// <summary>
        /// Begin a new sub-context. Contextual and transient instances created inside
        /// the subcontext will be disposed along with it.
        /// </summary>
        /// <returns>A new subcontext.</returns>
        IContainer CreateInnerContainer();

        /// <summary>
        /// Register a component.
        /// </summary>
        /// <param name="registration">A component registration.</param>
        void RegisterComponent(IComponentRegistration registration);

        /// <summary>
        /// Add a source from which registrations may be retrieved in the case that they
        /// are not available in the container.
        /// </summary>
        /// <param name="source">The source.</param>
        void AddRegistrationSource(IRegistrationSource source);

        /// <summary>
        /// The disposer associated with this container. Instances can be associated
        /// with it manually if required.
        /// </summary>
        IDisposer Disposer { get; }

        /// <summary>
        /// Fired when a new instance is being activated. The instance can be
        /// wrapped or switched at this time by setting the Instance property in
        /// the provided event arguments.
        /// </summary>
        event EventHandler<ActivatingEventArgs> Activating;

        /// <summary>
        /// Fired when the activation process for a new instance is complete.
        /// </summary>
        event EventHandler<ActivatedEventArgs> Activated;

        /// <summary>
        /// If the container is an inner container, retrieves the outer container.
        /// Otherwise, null;
        /// </summary>
        IContainer OuterContainer { get; }
        
        /// <summary>
        /// The registrations for all of the components registered with the container.
        /// </summary>
        IEnumerable<IComponentRegistration> ComponentRegistrations { get; }

        /// <summary>
        /// Fired whenever a component is registed into the container.
        /// </summary>
        event EventHandler<ComponentRegisteredEventArgs> ComponentRegistered;

        /// <summary>
        /// Gets the default component registration that will be used to satisfy
        /// requests for the provided service.
        /// </summary>
        /// <param name="service">The service.</param>
        /// <param name="registration">The registration.</param>
        /// <returns>True if a default exists, false otherwise.</returns>
        bool TryGetDefaultRegistrationFor(Service service, out IComponentRegistration registration);
    }
}
