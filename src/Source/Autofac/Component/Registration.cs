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
using System.Globalization;

namespace Autofac.Component
{
    /// <summary>
    /// Implements IComponentRegistration using activator and
    /// scope 'policy' classes.
    /// </summary>
	/// <remarks>
	/// ComponentRegistration, IActivator, IScope etc. are not parameterised
	/// with the component type because in some situations, i.e. when calling
	/// remote services through a proxy, a component type may not be available.
	/// </remarks>
    public class Registration : Disposable, IComponentRegistration
    {
        IComponentDescriptor _descriptor;
        IActivator _activator;
        IScope _scope;
		InstanceOwnership _ownershipModel;
        object _synchRoot = new object();

        /// <summary>
        /// Create a new ComponentRegistration.
        /// </summary>
        /// <param name="descriptor">The descriptor.</param>
        /// <param name="activator">An object with which new component instances
        /// can be created. Required.</param>
        /// <param name="scope">An object that tracks created instances with
        /// respect to their scope of usage, i.e., per-thread, per-call etc.
        /// Required. Will be disposed when the registration is disposed.</param>
        /// <param name="ownershipModel">The ownership model that determines
        /// whether the instances are disposed along with the scope.</param>
        public Registration(
            IComponentDescriptor descriptor,
            IActivator activator,
            IScope scope,
            InstanceOwnership ownershipModel)
        {
            _descriptor = Enforce.ArgumentNotNull(descriptor, "descriptor");
            _activator = Enforce.ArgumentNotNull(activator, "activator");
            _scope = Enforce.ArgumentNotNull(scope, "scope");

			_ownershipModel = ownershipModel;
        }

        #region IComponentRegistration Members

        /// <summary>
        /// Describes the component registration and the
        /// services it provides.
        /// </summary>
        /// <value></value>
        public IComponentDescriptor Descriptor
        {
            get
            {
                return _descriptor;
            }
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
        public virtual object ResolveInstance(IContext context, IActivationParameters parameters, IDisposer disposer, out bool newInstance)
        {
            Enforce.ArgumentNotNull(context, "context");
            Enforce.ArgumentNotNull(parameters, "parameters");
            Enforce.ArgumentNotNull(disposer, "disposer");

            CheckNotDisposed();

            lock (_synchRoot)
            {
                object instance;
                if (_scope.InstanceAvailable)
                {
                    instance = _scope.GetInstance();
                    newInstance = false;
                }
                else
                {
                    instance = _activator.ActivateInstance(context, parameters);

                    var activatingArgs = new ActivatingEventArgs(context, this, instance);
                    Activating(this, activatingArgs);

                    instance = activatingArgs.Instance;

                    if (_ownershipModel == InstanceOwnership.Container && instance is IDisposable)
                        disposer.AddInstanceForDisposal((IDisposable)instance);

                    _scope.SetInstance(instance);
                    newInstance = true;
                }

                return instance;
            }
        }

        /// <summary>
        /// Create a duplicate of this instance if it is semantically valid to
        /// copy it to a new context.
        /// </summary>
        /// <param name="duplicate">The duplicate.</param>
        /// <returns>True if the duplicate was created.</returns>
		public virtual bool DuplicateForNewContext(out IComponentRegistration duplicate)
		{
			duplicate = null;

			IScope newScope;
			if (!_activator.CanSupportNewContext)
				return false;

			if (!_scope.DuplicateForNewContext(out newScope))
				return false;

			var duplicateRegistration = CreateDuplicate(Descriptor, _activator, newScope, _ownershipModel);
			duplicate = duplicateRegistration;
            duplicate.Activating += (s, e) => Activating(this, e);
            duplicate.Activated += (s, e) => Activated(this, e);

			return true;
		}
		
		/// <summary>
		/// Semantically equivalent to ICloneable.Clone().
		/// </summary>
		protected virtual Registration CreateDuplicate(
            IComponentDescriptor descriptor,
			IActivator activator,
			IScope newScope,
			InstanceOwnership ownershipModel)
		{
            Enforce.ArgumentNotNull(descriptor, "descriptor");
			Enforce.ArgumentNotNull(activator, "activator");
			Enforce.ArgumentNotNull(newScope, "newScope");
			
			return new Registration(descriptor, _activator, newScope, _ownershipModel);
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

        /// <summary>
        /// Called by the container once an instance has been fully constructed, including
        /// any requested objects that depend on the instance.
        /// </summary>
        /// <param name="context">The context in which the instance was activated.</param>
        /// <param name="instance">The instance.</param>
        public virtual void InstanceActivated(IContext context, object instance)
        {
            var activatedArgs = new ActivatedEventArgs(context, this, instance);
            Activated(this, activatedArgs);
        }

        #endregion

        /// <summary>
        /// Gets the scope.
        /// </summary>
        /// <value>The scope.</value>
        protected internal IScope Scope
        {
            get { return _scope; }
        }
    }
}
