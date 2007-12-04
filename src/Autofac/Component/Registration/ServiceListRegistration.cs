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
using System.Linq;
using System.Text;
using Autofac.Builder;

namespace Autofac.Component.Registration
{
    /// <summary>
    /// Exposed by the generic ServiceListRegistration type to expose non-generic Add().
    /// </summary>
    public interface IServiceListRegistration
    {
        /// <summary>
        /// Add a service (key into another component registration) to those returned
        /// in the list.
        /// </summary>
        /// <param name="item"></param>
        void Add(Service item);
    }

    /// <summary>
    /// Registration that exposes collection interfaces onto a subset of other components
    /// in the container.
    /// </summary>
    /// <typeparam name="TItem"></typeparam>
    public class ServiceListRegistration<TItem> : IServiceListRegistration, IComponentRegistration
    {
        ICollection<Service> _items = new List<Service>();

        IEnumerable<Service> _services;

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceListRegistration&lt;TItem&gt;"/> class.
        /// </summary>
        /// <param name="services">The services.</param>
        public ServiceListRegistration(IEnumerable<Service> services)
        {
            Enforce.ArgumentNotNull(services, "services");

            foreach (var service in services)
            {
                var typed = service as TypedService;
                if (typed != null)
                    if (typed.ServiceType != typeof(IEnumerable<TItem>) &&
                        typed.ServiceType != typeof(ICollection<TItem>) &&
                        typed.ServiceType != typeof(IList<TItem>))
                        throw new NotSupportedException();
            }
            
            _services = services;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceListRegistration&lt;TItem&gt;"/> class.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <param name="items">The items.</param>
        protected ServiceListRegistration(IEnumerable<Service> services, IEnumerable<Service> items)
            : this(services)
        {
            Enforce.ArgumentNotNull(items, "items");

            foreach (var item in items)
                _items.Add(item);
        }

        #region ICompositeRegistration Members

        /// <summary>
        /// Add a service (key into another component registration) to those returned
        /// in the list.
        /// </summary>
        /// <param name="item"></param>
        public void Add(Service item)
        {
            Enforce.ArgumentNotNull(item, "item");
            _items.Add(item);
        }

        #endregion

        #region IComponentRegistration Members

        /// <summary>
        /// The services (named and typed) exposed by the component.
        /// </summary>
        /// <value></value>
        public IEnumerable<Service> Services
        {
            get { return _services; }
        }

        /// <summary>
        /// 	<i>Must</i> return a valid instance, or throw
        /// an exception on failure.
        /// </summary>
        /// <param name="context">The context that is to be used
        /// to resolve the instance's dependencies.</param>
        /// <param name="parameters">Parameters that can be used in the resolution process.</param>
        /// <param name="disposer">The disposer.</param>
        /// <param name="newInstance">if set to <c>true</c> a new instance was created.</param>
        /// <returns>A newly-resolved instance.</returns>
        public object ResolveInstance(IContext context, IActivationParameters parameters, IDisposer disposer, out bool newInstance)
        {
            Enforce.ArgumentNotNull(context, "context");
            Enforce.ArgumentNotNull(parameters, "parameters");
            Enforce.ArgumentNotNull(disposer, "disposer");

            var instance = new List<TItem>();
            foreach (var item in _items)
            {
                object itemInstance;
                if (!context.TryResolve(item, out itemInstance))
                    throw new ComponentNotRegisteredException(item);
                instance.Add((TItem)itemInstance);
            }

            var activatingArgs = new ActivatingEventArgs(context, this, instance);
            Activating(this, activatingArgs);

            newInstance = true;
            return activatingArgs.Instance;
        }

        /// <summary>
        /// Create a duplicate of this instance if it is semantically valid to
        /// copy it to a new context.
        /// </summary>
        /// <param name="duplicate">The duplicate.</param>
        /// <returns>True if the duplicate was created.</returns>
        public bool TryDuplicateForNewContext(out IComponentRegistration duplicate)
        {
            duplicate = new ServiceListRegistration<TItem>(_items);
            return true;
        }

        /// <summary>
        /// Fired when a new instance is being activated. The instance can be
        /// wrapped or switched at this time by setting the Instance property in
        /// the provided event arguments.
        /// </summary>
        public event EventHandler<ActivatingEventArgs> Activating = (s, e) => { };

        /// <summary>
        /// Fired when the activation process for a new instance is complete.
        /// </summary>
        public event EventHandler<ActivatedEventArgs> Activated = (s, e) => { };

        /// <summary>
        /// Called by the container once an instance has been fully constructed, including
        /// any requested objects that depend on the instance.
        /// </summary>
        /// <param name="context">The context in which the instance was activated.</param>
        /// <param name="instance">The instance.</param>
        public void InstanceActivated(IContext context, object instance)
        {
            Activated(this, new ActivatedEventArgs(context, this, instance));
        }

        #endregion

        #region IDisposable Members

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
        }

        #endregion
    }
}
