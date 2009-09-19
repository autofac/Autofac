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
using Autofac.Core.Lifetime;
using Autofac.Util;
using Autofac.Core;

namespace Autofac.Builder
{
    /// <summary>
    /// Data common to all registrations made in the container, both direct (IComponentRegistration)
    /// and dynamic (IRegistrationSource.)
    /// </summary>
    public class RegistrationData
    {
        ICollection<Service> _services = new HashSet<Service>();
        InstanceOwnership _ownership = InstanceOwnership.OwnedByLifetimeScope;
        IComponentLifetime _lifetime = new CurrentScopeLifetime();
        InstanceSharing _sharing = InstanceSharing.None;
        IDictionary<string, object> _extendedProperties = new Dictionary<string, object>();
        ICollection<EventHandler<PreparingEventArgs<object>>> _preparingHandlers = new List<EventHandler<PreparingEventArgs<object>>>();
        ICollection<EventHandler<ActivatingEventArgs<object>>> _activatingHandlers = new List<EventHandler<ActivatingEventArgs<object>>>();
        ICollection<EventHandler<ActivatedEventArgs<object>>> _activatedHandlers = new List<EventHandler<ActivatedEventArgs<object>>>();

        /// <summary>
        /// The services explicitly assigned to the component.
        /// </summary>
        public ICollection<Service> Services { get { return _services; } }

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
        public IDictionary<string, object> ExtendedProperties { get { return _extendedProperties; } }

        /// <summary>
        /// Handlers for the Preparing event.
        /// </summary>
        public ICollection<EventHandler<PreparingEventArgs<object>>> PreparingHandlers { get { return _preparingHandlers; } }

        /// <summary>
        /// Handlers for the Activating event.
        /// </summary>
        public ICollection<EventHandler<ActivatingEventArgs<object>>> ActivatingHandlers { get { return _activatingHandlers; } }

        /// <summary>
        /// Handlers for the Activated event.
        /// </summary>
        public ICollection<EventHandler<ActivatedEventArgs<object>>> ActivatedHandlers { get { return _activatedHandlers; } }
    }
}
