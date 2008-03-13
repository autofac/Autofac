// This software is part of the Autofac IoC container
// Copyright (c) 2007-2008 Autofac Contributors
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
using Autofac.Component;
using Autofac.Registrars.Delegate;

namespace Autofac.Tags
{
	/// <summary>
	/// Description of TaggedDelegateRegistrar.
	/// </summary>
	class TaggedDelegateRegistrar<TTag> : DelegateRegistrar
	{
        TTag _tag;

        /// <summary>
        /// Initializes a new instance of the DelegateRegistrar&lt;TComponent&gt; class.
        /// </summary>
        /// <param name="implementor">The implementor.</param>
        /// <param name="creator">The creator.</param>
        /// <param name="tag">The tag of the target context.</param>
        public TaggedDelegateRegistrar(Type implementor, ComponentActivator creator, TTag tag)
			: base(implementor, creator)
		{
            _tag = tag;
		}

        /// <summary>
        /// Create the registration.
        /// </summary>
        /// <param name="descriptor">The descriptor.</param>
        /// <param name="activator">Activator.</param>
        /// <param name="scope">Scope.</param>
        /// <param name="ownership">Ownership model.</param>
        /// <returns>The registration.</returns>
		protected override IComponentRegistration CreateRegistration(
        	IComponentDescriptor descriptor,
            IActivator activator, 
        	IScope scope, 
        	InstanceOwnership ownership)
		{
			return new TaggedRegistration<TTag>(
                descriptor, activator, scope, ownership, _tag);
		}
	}
}
