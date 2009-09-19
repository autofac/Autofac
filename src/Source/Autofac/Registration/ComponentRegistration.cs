// This software is part of the Autofac IoC container
// Copyright (c) 2007 - 2008 Autofac Contributors
// http://autofac.org
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
using System.Linq;
using Autofac.Core;
using Autofac.Lifetime;
using Autofac.Util;

namespace Autofac.Registration
{
    /// <summary>
    /// Describes a logical component within the container.
    /// </summary>
    public class ComponentRegistration : Disposable, IComponentRegistration
    {
        /// <summary>
        /// Create a new component registration.
        /// </summary>
        /// <param name="id">Unique identifier for the component.</param>
        /// <param name="activator">Activator used to activate instances.</param>
        /// <param name="lifetime">Determines how the component will be associated with its lifetime.</param>
        /// <param name="sharing">Whether the component is shared within its lifetime scope.</param>
        /// <param name="ownership">Whether the component instances are disposed at the end of their lifetimes.</param>
        /// <param name="services">Services the component provides.</param>
        /// <param name="extendedProperties">Data associated with the component.</param>
        public ComponentRegistration(
            Guid id,
            IInstanceActivator activator,
            IComponentLifetime lifetime,
            InstanceSharing sharing,
            InstanceOwnership ownership,
            IEnumerable<Service> services,
            IDictionary<string, object> extendedProperties)
        {
            Id = id;
            Activator = Enforce.ArgumentNotNull(activator, "activator");
            Lifetime = Enforce.ArgumentNotNull(lifetime, "lifetime");
            Sharing = sharing;
            Ownership = ownership;
            Services = Enforce.ArgumentElementNotNull(
                Enforce.ArgumentNotNull(services, "services"), "services").ToList();
            ExtendedProperties = new Dictionary<string, object>(
                Enforce.ArgumentNotNull(extendedProperties, "extendedProperties"));
        }

        /// <summary>
        /// A unique identifier for this component (shared in all sub-contexts.)
        /// This value also appears in Services.
        /// </summary>
        public Guid Id { get; private set; }

        /// <summary>
        /// The activator used to create instances.
        /// </summary>
        public IInstanceActivator Activator { get; private set; }

        /// <summary>
        /// The lifetime associated with the component.
        /// </summary>
        public IComponentLifetime Lifetime { get; private set; }

        /// <summary>
        /// Whether the component instances are shared or not.
        /// </summary>
        public InstanceSharing Sharing { get; private set; }

        /// <summary>
        /// Whether the instances of the component should be disposed by the container.
        /// </summary>
        public InstanceOwnership Ownership { get; private set; }

        /// <summary>
        /// The services provided by the component.
        /// </summary>
        public IEnumerable<Service> Services { get; private set; }

        /// <summary>
        /// Additional data associated with the component.
        /// </summary>
        public IDictionary<string, object> ExtendedProperties { get; private set; }

        /// <summary>
        /// Fired when a new instance is required. The instance can be
        /// provided in order to skip the regular activator, by setting the Instance property in
        /// the provided event arguments.
        /// </summary>
        public event EventHandler<PreparingEventArgs<object>> Preparing = (s, e) => { };

        /// <summary>
        /// Called by the container when an instance is required.
        /// </summary>
        /// <param name="context">The context in which the instance will be activated.</param>
        /// <param name="parameters">Parameters for activation.</param>
        public void RaisePreparing(IComponentContext context, IEnumerable<Parameter> parameters)
        {
            Preparing(this, new PreparingEventArgs<object>(context, this, parameters));
        }

        /// <summary>
        /// Fired when a new instance is being activated. The instance can be
        /// wrapped or switched at this time by setting the Instance property in
        /// the provided event arguments.
        /// </summary>
        public event EventHandler<ActivatingEventArgs<object>> Activating = (s, e) => { };

        /// <summary>
        /// Called by the container once an instance has been constructed.
        /// </summary>
        /// <param name="context">The context in which the instance was activated.</param>
        /// <param name="instance">The instance.</param>
        public void RaiseActivating(IComponentContext context, object instance)
        {
            Activating(this, new ActivatingEventArgs<object>(context, this, instance));
        }

        /// <summary>
        /// Fired when the activation process for a new instance is complete.
        /// </summary>
        public event EventHandler<ActivatedEventArgs<object>> Activated = (s, e) => { };

        /// <summary>
        /// Called by the container once an instance has been fully constructed, including
        /// any requested objects that depend on the instance.
        /// </summary>
        /// <param name="context">The context in which the instance was activated.</param>
        /// <param name="instance">The instance.</param>
        public void RaiseActivated(IComponentContext context, object instance)
        {
            Activated(this, new ActivatedEventArgs<object>(context, this, instance));
        }

        /// <summary>
        /// Describes the component in a human-readable form.
        /// </summary>
        /// <returns>A description of the component.</returns>
        public override string ToString()
        {
            return Activator.LimitType.ToString();
        }
    }
}
