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
using System.Text;
using System.Globalization;

namespace Autofac.Component.Registration
{
    /// <summary>
    /// The standard ICollectionRegistration. Exposes added components
    /// through the IEnumerable&lt;TItem>, ICollection&lt;TItem>
    /// and IList&lt;TItem> interfaces.
    /// </summary>
    /// <typeparam name="TItem">The service type (ItemType) that will be contained
    /// within the collection registration.</typeparam>
    /// <remarks>
    /// The collections returned by ResolveInstance() are dynamic in nature -
    /// they will change to reflect changes in the container (for instance if
    /// more matching services are registered.)
    /// </remarks>
    public class CollectionRegistration<TItemService> : Disposable, ICollectionRegistration
    {
        IEnumerable<Type> _serviceTypes = new Type[] {
                                              typeof(IEnumerable<TItemService>),
                                              typeof(ICollection<TItemService>),
                                              typeof(IList<TItemService>) };

        string _name = Guid.NewGuid().ToString();

        IList<IComponentRegistration> _itemRegistrations = new List<IComponentRegistration>();

        #region ICollectionRegistration Members

        /// <summary>
        /// The service type exposed by the individual items in the collection.
        /// </summary>
        /// <value></value>
        public Type ItemType
        {
            get
            {
                return typeof(TItemService);
            }
        }

        /// <summary>
        /// Used by the container to add subsequent component registrations
        /// exposing the ItemType service to the collection.
        /// </summary>
        /// <param name="item">The component registration to add to the
        /// collection. Required.</param>
        public void Add(IComponentRegistration item)
        {
            Enforce.ArgumentNotNull(item, "item");

            IList<Type> serviceTypes = new List<Type>(item.Services);
            if (!serviceTypes.Contains(ItemType))
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture,
                          CollectionRegistrationResources.AddedRegistrationsMustExposeItemType, ItemType), "item");

            _itemRegistrations.Add(item);
        }

        #endregion

        #region IComponentRegistration Members

        /// <summary>
        /// The services exposed by the component.
        /// </summary>
        /// <value></value>
        public IEnumerable<Type> Services
        {
            get
            {
                return _serviceTypes;
            }
        }

        /// <summary>
        /// A name that uniquely identifies this component.
        /// </summary>
        /// <value></value>
        public string Name
        {
            get
            {
                return _name;
            }
        }

        /// <summary>
        /// 	<i>Must</i> return a valid instance, or throw
        /// an exception on failure.
        /// </summary>
        /// <param name="context">The context that is to be used
        /// to resolve the instance's dependencies.</param>
        /// <param name="parameters">Unused.</param>
        /// <returns>A newly-resolved instance.</returns>
        public object ResolveInstance(IContext context, IActivationParameters parameters, IDisposer disposer)
        {
            Enforce.ArgumentNotNull(context, "context");
            Enforce.ArgumentNotNull(parameters, "parameters");
            Enforce.ArgumentNotNull(disposer, "disposer");

            object instance = new ContextBoundServiceList<TItemService>(_itemRegistrations, context, disposer);
			var activatingArgs = new ActivatingEventArgs(context, this, instance);
			Activating(this, activatingArgs);
			instance = activatingArgs.Instance;
			var activatedArgs = new ActivatedEventArgs(context, this, instance);
			Activated(this, activatedArgs);
			return instance;
        }

        /// <summary>
        /// Create a duplicate of this instance if it is semantically valid to
        /// copy it to a new context.
        /// </summary>
        /// <param name="duplicate">The duplicate.</param>
        /// <returns>True if the duplicate was created.</returns>
		public bool TryDuplicateForNewContext(out IComponentRegistration duplicate)
		{
			CollectionRegistration<TItemService> result = new CollectionRegistration<TItemService>();
			foreach (IComponentRegistration item in _itemRegistrations)
			{
				IComponentRegistration itemDuplicate;
				if (!item.TryDuplicateForNewContext(out itemDuplicate))
				{
					duplicate = null;
					return false;
				}

				result.Add(itemDuplicate);
			}

			duplicate = result;
			return true;
		}

        /// <summary>
        /// Fired when a new instance is being activated. The instance can be
        /// wrapped or switched at this time by setting the Instance property in
        /// the provided event arguments.
        /// </summary>
		public event EventHandler<ActivatingEventArgs> Activating = (sender, e) => { };

        /// <summary>
        /// Fired when the activation process for a new instance is complete.
        /// </summary>
		public event EventHandler<ActivatedEventArgs> Activated = (sender, e) => { };

        #endregion

        #region IDisposable Members

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
			if (disposing)
				foreach (IComponentRegistration registration in _itemRegistrations)
					registration.Dispose();
        }

        #endregion
	}
}
