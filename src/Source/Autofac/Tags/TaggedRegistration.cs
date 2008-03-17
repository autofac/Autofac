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

using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Autofac.Component;

namespace Autofac.Tags
{
	/// <summary>
	/// Description of TaggedRegistration.
	/// </summary>
	class TaggedRegistration<TTag> : Registration
	{
        static readonly Parameter[] EmptyParameters = new Parameter[0];
		readonly TTag _tag;

        /// <summary>
        /// Create a new TaggedRegistration.
        /// </summary>
        /// <param name="descriptor">The descriptor.</param>
        /// <param name="activator">An object with which new component instances
        /// can be created. Required.</param>
        /// <param name="scope">An object that tracks created instances with
        /// respect to their scope of usage, i.e., per-thread, per-call etc.
        /// Required. Will be disposed when the registration is disposed.</param>
        /// <param name="ownershipModel">The ownership model that determines
        /// whether the instances are disposed along with the scope.</param>
        /// <param name="tag">The tag corresponding to the context in which this
        /// component may be resolved.</param>
		public TaggedRegistration(
            IComponentDescriptor descriptor,
            IActivator activator,
            IScope scope,
			InstanceOwnership ownershipModel,
			TTag tag)
			: base(descriptor, activator, scope, ownershipModel)
		{
            _tag = tag;
        }

        /// <summary>
        /// Semantically equivalent to ICloneable.Clone().
        /// </summary>
        /// <param name="descriptor"></param>
        /// <param name="activator"></param>
        /// <param name="newScope"></param>
        /// <param name="ownershipModel"></param>
        /// <returns>
        /// A registrar allowing configuration to continue.
        /// </returns>
        protected override Registration CreateDuplicate(
            IComponentDescriptor descriptor,
			IActivator activator, 
			IScope newScope, 
			InstanceOwnership ownershipModel)
		{
			return new TaggedRegistration<TTag>(descriptor, activator, newScope, ownershipModel, _tag);
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
        public override object ResolveInstance(
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
            	return base.ResolveInstance(context, parameters, disposer, out newInstance);
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

        private static Parameter[] MakeParameters(IActivationParameters p)
        {
        	Enforce.ArgumentNotNull(p, "p");

            if (p.Count == 0)
                return EmptyParameters;

            return p.Select(kvp => new Parameter(kvp.Key, kvp.Value)).ToArray();
        }        
	}
}
