// This software is part of the Autofac IoC container
// Copyright © 2011 Autofac Contributors
// https://autofac.org
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
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using Autofac.Util;

namespace Autofac.Core.Registration
{
    /// <summary>
    /// Describes a logical component within the container.
    /// </summary>
    [SuppressMessage("Microsoft.ApiDesignGuidelines", "CA2213", Justification = "The target registration, if provided, is disposed elsewhere.")]
    public class ComponentRegistration : Disposable, IComponentRegistration
    {
        private readonly IComponentRegistration _target;

        /// <summary>
        /// Initializes a new instance of the <see cref="ComponentRegistration"/> class.
        /// </summary>
        /// <param name="id">Unique identifier for the component.</param>
        /// <param name="activator">Activator used to activate instances.</param>
        /// <param name="lifetime">Determines how the component will be associated with its lifetime.</param>
        /// <param name="sharing">Whether the component is shared within its lifetime scope.</param>
        /// <param name="ownership">Whether the component instances are disposed at the end of their lifetimes.</param>
        /// <param name="services">Services the component provides.</param>
        /// <param name="metadata">Data associated with the component.</param>
        public ComponentRegistration(
            Guid id,
            IInstanceActivator activator,
            IComponentLifetime lifetime,
            InstanceSharing sharing,
            InstanceOwnership ownership,
            IEnumerable<Service> services,
            IDictionary<string, object> metadata)
        {
            if (activator == null) throw new ArgumentNullException(nameof(activator));
            if (lifetime == null) throw new ArgumentNullException(nameof(lifetime));
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (metadata == null) throw new ArgumentNullException(nameof(metadata));

            Id = id;
            Activator = activator;
            Lifetime = lifetime;
            Sharing = sharing;
            Ownership = ownership;
            Services = Enforce.ArgumentElementNotNull(services, nameof(services));
            Metadata = metadata;
            IsAdapterForIndividualComponent = false;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ComponentRegistration"/> class.
        /// </summary>
        /// <param name="id">Unique identifier for the component.</param>
        /// <param name="activator">Activator used to activate instances.</param>
        /// <param name="lifetime">Determines how the component will be associated with its lifetime.</param>
        /// <param name="sharing">Whether the component is shared within its lifetime scope.</param>
        /// <param name="ownership">Whether the component instances are disposed at the end of their lifetimes.</param>
        /// <param name="services">Services the component provides.</param>
        /// <param name="metadata">Data associated with the component.</param>
        /// <param name="target">The component registration upon which this registration is based.</param>
        /// <param name="isAdapterForIndividualComponents">Whether the registration is a 1:1 adapters on top of another component.</param>
        public ComponentRegistration(
            Guid id,
            IInstanceActivator activator,
            IComponentLifetime lifetime,
            InstanceSharing sharing,
            InstanceOwnership ownership,
            IEnumerable<Service> services,
            IDictionary<string, object> metadata,
            IComponentRegistration target,
            bool isAdapterForIndividualComponents)
            : this(id, activator, lifetime, sharing, ownership, services, metadata)
        {
            if (target == null) throw new ArgumentNullException(nameof(target));

            _target = target;
            IsAdapterForIndividualComponent = isAdapterForIndividualComponents;
        }

        /// <summary>
        /// Gets the component registration upon which this registration is based.
        /// If this registration was created directly by the user, returns this.
        /// </summary>
        public IComponentRegistration Target => _target ?? this;

        /// <summary>
        /// Gets a unique identifier for this component (shared in all sub-contexts.)
        /// This value also appears in Services.
        /// </summary>
        public Guid Id { get; }

        /// <summary>
        /// Gets or sets the activator used to create instances.
        /// </summary>
        public IInstanceActivator Activator { get; set; }

        /// <summary>
        /// Gets the lifetime associated with the component.
        /// </summary>
        public IComponentLifetime Lifetime { get; }

        /// <summary>
        /// Gets information about whether the component instances are shared or not.
        /// </summary>
        public InstanceSharing Sharing { get; }

        /// <summary>
        /// Gets information about whether the instances of the component should be disposed by the container.
        /// </summary>
        public InstanceOwnership Ownership { get; }

        /// <summary>
        /// Gets the services provided by the component.
        /// </summary>
        public IEnumerable<Service> Services { get; }

        /// <summary>
        /// Gets additional data associated with the component.
        /// </summary>
        public IDictionary<string, object> Metadata { get; }

        /// <inheritdoc />
        public bool IsAdapterForIndividualComponent { get; }

        /// <summary>
        /// Fired when a new instance is required, prior to activation.
        /// Can be used to provide Autofac with additional parameters, used during activation.
        /// </summary>
        public event EventHandler<PreparingEventArgs> Preparing;

        /// <summary>
        /// Called by the container when an instance is required.
        /// </summary>
        /// <param name="context">The context in which the instance will be activated.</param>
        /// <param name="parameters">Parameters for activation.</param>
        public void RaisePreparing(IComponentContext context, ref IEnumerable<Parameter> parameters)
        {
            var handler = Preparing;
            if (handler == null) return;

            var args = new PreparingEventArgs(context, this, parameters);
            handler(this, args);
            parameters = args.Parameters;
        }

        /// <summary>
        /// Fired when a new instance is being activated. The instance can be
        /// wrapped or switched at this time by setting the Instance property in
        /// the provided event arguments.
        /// </summary>
        public event EventHandler<ActivatingEventArgs<object>> Activating;

        /// <summary>
        /// Called by the container once an instance has been constructed.
        /// </summary>
        /// <param name="context">The context in which the instance was activated.</param>
        /// <param name="parameters">The parameters supplied to the activator.</param>
        /// <param name="instance">The instance.</param>
        public void RaiseActivating(IComponentContext context, IEnumerable<Parameter> parameters, ref object instance)
        {
            var handler = Activating;
            if (handler == null) return;

            var args = new ActivatingEventArgs<object>(context, this, parameters, instance);
            handler(this, args);
            instance = args.Instance;
        }

        /// <summary>
        /// Fired when the activation process for a new instance is complete.
        /// </summary>
        public event EventHandler<ActivatedEventArgs<object>> Activated;

        /// <summary>
        /// Called by the container once an instance has been fully constructed, including
        /// any requested objects that depend on the instance.
        /// </summary>
        /// <param name="context">The context in which the instance was activated.</param>
        /// <param name="parameters">The parameters supplied to the activator.</param>
        /// <param name="instance">The instance.</param>
        public void RaiseActivated(IComponentContext context, IEnumerable<Parameter> parameters, object instance)
        {
            var handler = Activated;
            handler?.Invoke(this, new ActivatedEventArgs<object>(context, this, parameters, instance));
        }

        /// <summary>
        /// Describes the component in a human-readable form.
        /// </summary>
        /// <returns>A description of the component.</returns>
        public override string ToString()
        {
            // Activator = {0}, Services = [{1}], Lifetime = {2}, Sharing = {3}, Ownership = {4}
            return string.Format(
                CultureInfo.CurrentCulture,
                ComponentRegistrationResources.ToStringFormat,
                Activator,
                Services.Select(s => s.Description).JoinWith(", "),
                Lifetime,
                Sharing,
                Ownership);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
                Activator.Dispose();
            base.Dispose(disposing);
        }
    }
}
