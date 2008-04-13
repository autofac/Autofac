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
using System.Globalization;
using System.Linq;
using Autofac.Component;

namespace Autofac.Component
{
	/// <summary>
	/// Description of TaggedRegistration.
	/// </summary>
	class TaggedRegistration<TTag> : IComponentRegistration, IDisposable
	{
        static readonly Parameter[] EmptyParameters = new Parameter[0];
		readonly TTag _tag;
		readonly IComponentRegistration _inner;

        /// <summary>
        /// Create a new TaggedRegistration.
        /// </summary>
        /// <param name="tag">The tag corresponding to the context in which this
        /// component may be resolved.</param>
        /// <param name="inner">The decorated registration.</param>
		public TaggedRegistration(
			TTag tag,
			IComponentRegistration inner)
		{
        	_inner = Enforce.ArgumentNotNull(inner, "inner");
            _tag = tag;
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
        public virtual object ResolveInstance(
        	IContext context, 
        	IActivationParameters parameters, 
        	IDisposer disposer, 
        	out bool newInstance)
        {
            Enforce.ArgumentNotNull(context, "context");
            Enforce.ArgumentNotNull(parameters, "parameters");
            Enforce.ArgumentNotNull(disposer, "disposer");

            newInstance = false;
            
            var ct = context.Resolve<ContextTag<TTag>>();
            if (ct.HasTag && object.Equals(ct.Tag, _tag))
            {
            	return _inner.ResolveInstance(context, parameters, disposer, out newInstance);
            }
            else
            {
                var container = context.Resolve<IContainer>();
                if (container.OuterContainer != null)
                    return container.OuterContainer.Resolve(Descriptor.Id, MakeParameters(parameters));
                else
                    throw new DependencyResolutionException(string.Format(CultureInfo.CurrentCulture,
                        TaggedRegistrationResources.TaggedContextNotFound,
                        _tag));
            }
        }
        /// <summary>
        /// Describes the component registration and the
        /// services it provides.
        /// </summary>
        public virtual IComponentDescriptor Descriptor
        {
        	get
        	{
        		return _inner.Descriptor;
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
			IComponentRegistration innerDuplicate;
			if (_inner.DuplicateForNewContext(out innerDuplicate))
			{
				duplicate = new TaggedRegistration<TTag>(_tag, innerDuplicate);
				return true;
			}
			else
			{
				duplicate = null;
				return false;
			}
		}

        /// <summary>
        /// Fired when a new instance is required. The instance can be
        /// provided in order to skip the regular activator, by setting the Instance property in
        /// the provided event arguments.
        /// </summary>
        public virtual event EventHandler<PreparingEventArgs> Preparing
        {
        	add
        	{
        		_inner.Preparing += value;
        	}
        	remove
        	{
        		_inner.Preparing -= value;
        	}
        }

		/// <summary>
		/// Fired when a new instance is being activated. The instance can be
		/// wrapped or switched at this time by setting the Instance property in
		/// the provided event arguments.
		/// </summary>
		public virtual event EventHandler<ActivatingEventArgs> Activating
		{
			add
        	{
        		_inner.Activating += value;
        	}
        	remove
        	{
        		_inner.Activating -= value;
        	}
		}
		
		/// <summary>
		/// Fired when the activation process for a new instance is complete.
		/// </summary>
		public virtual event EventHandler<ActivatedEventArgs> Activated
		{
			add
        	{
        		_inner.Activated += value;
        	}
        	remove
        	{
        		_inner.Activated -= value;
        	}
		}

        /// <summary>
        /// Called by the container once an instance has been fully constructed, including
        /// any requested objects that depend on the instance.
        /// </summary>
        /// <param name="context">The context in which the instance was activated.</param>
        /// <param name="instance">The instance.</param>
        public virtual void InstanceActivated(IContext context, object instance)
        {
        	_inner.InstanceActivated(context, instance);
        }
        
        public virtual void Dispose()
        {
        	_inner.Dispose();
        }

        private static Parameter[] MakeParameters(IActivationParameters p)
        {
            Enforce.ArgumentNotNull(p, "p");

            if (p.Count == 0)
                return EmptyParameters;

            return p.Select(kvp => new Parameter(kvp.Key, kvp.Value)).ToArray();
        }

        /// <summary>
        /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </returns>
        public override string ToString()
        {
        	return string.Format(TaggedRegistrationResources.ToStringFormat, _inner, _tag);
        }
	}
}
