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
using System.Linq;
using Autofac.Core;
using Autofac.Core.Lifetime;
using Autofac.Util;

namespace Autofac.Builder
{
    /// <summary>
    /// Data common to all registrations made in the container, both direct (IComponentRegistration)
    /// and dynamic (IRegistrationSource).
    /// </summary>
    public class RegistrationData
    {
        private bool _defaultServiceOverridden;
        private Service _defaultService;

        private readonly ICollection<Service> _services = new HashSet<Service>();

        private IComponentLifetime _lifetime = new CurrentScopeLifetime();

        /// <summary>
        /// Initializes a new instance of the <see cref="RegistrationData"/> class.
        /// </summary>
        /// <param name="defaultService">The default service that will be used if no others
        /// are added.</param>
        public RegistrationData(Service defaultService)
        {
            if (defaultService == null) throw new ArgumentNullException(nameof(defaultService));

            _defaultService = defaultService;

            Metadata = new Dictionary<string, object>
            {
                { MetadataKeys.RegistrationOrderMetadataKey, SequenceGenerator.GetNextUniqueSequence() },
            };
        }

        /// <summary>
        /// Gets the services explicitly assigned to the component.
        /// </summary>
        public IEnumerable<Service> Services
        {
            get
            {
                if (_defaultServiceOverridden)
                    return _services;

                return _services.DefaultIfEmpty(_defaultService);
            }
        }

        /// <summary>
        /// Add multiple services for the registration, overriding the default.
        /// </summary>
        /// <param name="services">The services to add.</param>
        /// <remarks>If an empty collection is specified, this will still
        /// clear the default service.</remarks>
        public void AddServices(IEnumerable<Service> services)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            _defaultServiceOverridden = true; // important even when services is empty
            foreach (var service in services)
                AddService(service);
        }

        /// <summary>
        /// Add a service to the registration, overriding the default.
        /// </summary>
        /// <param name="service">The service to add.</param>
        public void AddService(Service service)
        {
            if (service == null) throw new ArgumentNullException(nameof(service));

            _defaultServiceOverridden = true;
            _services.Add(service);
        }

        /// <summary>
        /// Gets or sets the instance ownership assigned to the component.
        /// </summary>
        public InstanceOwnership Ownership { get; set; } = InstanceOwnership.OwnedByLifetimeScope;

        /// <summary>
        /// Gets or sets the lifetime assigned to the component.
        /// </summary>
        public IComponentLifetime Lifetime
        {
            get
            {
                return _lifetime;
            }

            set
            {
                if (value == null) throw new ArgumentNullException(nameof(value));
                _lifetime = value;
            }
        }

        /// <summary>
        /// Gets or sets the sharing mode assigned to the component.
        /// </summary>
        public InstanceSharing Sharing { get; set; } = InstanceSharing.None;

        /// <summary>
        /// Gets the extended properties assigned to the component.
        /// </summary>
        public IDictionary<string, object> Metadata { get; }

        /// <summary>
        /// Gets or sets the callback used to register this component.
        /// </summary>
        /// <value>
        /// A <see cref="Builder.DeferredCallback"/> that contains the delegate
        /// used to register this component with an <see cref="IComponentRegistry"/>.
        /// </value>
        public DeferredCallback DeferredCallback { get; set; }

        /// <summary>
        /// Gets the handlers for the Preparing event.
        /// </summary>
        public ICollection<EventHandler<PreparingEventArgs>> PreparingHandlers { get; } = new List<EventHandler<PreparingEventArgs>>();

        /// <summary>
        /// Gets the handlers for the Activating event.
        /// </summary>
        public ICollection<EventHandler<ActivatingEventArgs<object>>> ActivatingHandlers { get; } = new List<EventHandler<ActivatingEventArgs<object>>>();

        /// <summary>
        /// Gets the handlers for the Activated event.
        /// </summary>
        public ICollection<EventHandler<ActivatedEventArgs<object>>> ActivatedHandlers { get; } = new List<EventHandler<ActivatedEventArgs<object>>>();

        /// <summary>
        /// Copies the contents of another RegistrationData object into this one.
        /// </summary>
        /// <param name="that">The data to copy.</param>
        /// <param name="includeDefaultService">When true, the default service
        /// will be changed to that of the other.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="that" /> is <see langword="null" />.
        /// </exception>
        public void CopyFrom(RegistrationData that, bool includeDefaultService)
        {
            if (that == null) throw new ArgumentNullException(nameof(that));

            Ownership = that.Ownership;
            Sharing = that.Sharing;
            Lifetime = that.Lifetime;

            _defaultServiceOverridden |= that._defaultServiceOverridden;
            if (includeDefaultService)
                _defaultService = that._defaultService;

            AddAll(_services, that._services);
            AddAll(Metadata, that.Metadata.Where(m => m.Key != MetadataKeys.RegistrationOrderMetadataKey));
            AddAll(PreparingHandlers, that.PreparingHandlers);
            AddAll(ActivatingHandlers, that.ActivatingHandlers);
            AddAll(ActivatedHandlers, that.ActivatedHandlers);
        }

        private static void AddAll<T>(ICollection<T> to, IEnumerable<T> from)
        {
            foreach (var item in from)
                to.Add(item);
        }

        /// <summary>
        /// Empties the configured services.
        /// </summary>
        public void ClearServices()
        {
            _services.Clear();
            _defaultServiceOverridden = true;
        }
    }
}
