// This software is part of the Autofac IoC container
// Copyright © 2011 Autofac Contributors
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
using Autofac.Core.Lifetime;
using Autofac.Util;

namespace Autofac.Builder
{
    /// <summary>
    /// Data common to all registrations made in the container, both direct (IComponentRegistration)
    /// and dynamic (IRegistrationSource.)
    /// </summary>
    public class RegistrationData
    {
        bool _defaultServiceOverridden;
        Service _defaultService;

#if !NET35
        readonly ICollection<Service> _services = new HashSet<Service>();
#else
        readonly ICollection<Service> _services = new List<Service>();
#endif

        InstanceOwnership _ownership = InstanceOwnership.OwnedByLifetimeScope;
        IComponentLifetime _lifetime = new CurrentScopeLifetime();
        InstanceSharing _sharing = InstanceSharing.None;
        readonly IDictionary<string, object> _metadata = new Dictionary<string, object>();
        readonly ICollection<EventHandler<PreparingEventArgs>> _preparingHandlers = new List<EventHandler<PreparingEventArgs>>();
        readonly ICollection<EventHandler<ActivatingEventArgs<object>>> _activatingHandlers = new List<EventHandler<ActivatingEventArgs<object>>>();
        readonly ICollection<EventHandler<ActivatedEventArgs<object>>> _activatedHandlers = new List<EventHandler<ActivatedEventArgs<object>>>();

        /// <summary>
        /// Construct a RegistrationData instance.
        /// </summary>
        /// <param name="defaultService">The default service that will be used if no others
        /// are added.</param>
        public RegistrationData(Service defaultService)
        {
            if (defaultService == null) throw new ArgumentNullException("defaultService");
            _defaultService = defaultService;
        }

        /// <summary>
        /// The services explicitly assigned to the component.
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
            if (services == null) throw new ArgumentNullException("services");
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
            if (service == null) throw new ArgumentNullException("service");
            _defaultServiceOverridden = true;
            _services.Add(service);
        }

        /// <summary>
        /// The instance ownership assigned to the component.
        /// </summary>
        public InstanceOwnership Ownership
        {
            get { return _ownership; }
            set { _ownership = value; }
        }

        /// <summary>
        /// The lifetime assigned to the component.
        /// </summary>
        public IComponentLifetime Lifetime
        {
            get { return _lifetime; }
            set { _lifetime = Enforce.ArgumentNotNull(value, "lifetime"); }
        }

        /// <summary>
        /// The sharing mode assigned to the component.
        /// </summary>
        public InstanceSharing Sharing 
        {
            get { return _sharing; }
            set { _sharing = value; }
        }

        /// <summary>
        /// Extended properties assigned to the component.
        /// </summary>
        public IDictionary<string, object> Metadata { get { return _metadata; } }

        /// <summary>
        /// Handlers for the Preparing event.
        /// </summary>
        public ICollection<EventHandler<PreparingEventArgs>> PreparingHandlers { get { return _preparingHandlers; } }

        /// <summary>
        /// Handlers for the Activating event.
        /// </summary>
        public ICollection<EventHandler<ActivatingEventArgs<object>>> ActivatingHandlers { get { return _activatingHandlers; } }

        /// <summary>
        /// Handlers for the Activated event.
        /// </summary>
        public ICollection<EventHandler<ActivatedEventArgs<object>>> ActivatedHandlers { get { return _activatedHandlers; } }

        /// <summary>
        /// Copies the contents of another RegistrationData object into this one.
        /// </summary>
        /// <param name="that">The data to copy.</param>
        /// <param name="includeDefaultService">When true, the default service
        /// will be changed to that of the other.</param>
        public void CopyFrom(RegistrationData that, bool includeDefaultService)
        {
            Ownership = that.Ownership;
            Sharing = that.Sharing;
            Lifetime = that.Lifetime;

            _defaultServiceOverridden |= that._defaultServiceOverridden;
            if (includeDefaultService)
                _defaultService = that._defaultService;

            AddAll(_services, that._services);            
            AddAll(Metadata, that.Metadata);
            AddAll(PreparingHandlers, that.PreparingHandlers);
            AddAll(ActivatingHandlers, that.ActivatingHandlers);
            AddAll(ActivatedHandlers, that.ActivatedHandlers);
        }

        static void AddAll<T>(ICollection<T> to, IEnumerable<T> from)
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
