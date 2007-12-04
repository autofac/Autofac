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

namespace Autofac
{
    /// <summary>
    /// A component registration is used by the container
    /// to create and manage the services it holds.
    /// </summary>
    public interface IComponentRegistration : IDisposable
	{
        /// <summary>
        /// The services (named and typed) exposed by the component.
        /// </summary>
        IEnumerable<Service> Services { get; }

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
        object ResolveInstance(IContext context, IActivationParameters parameters, IDisposer disposer, out bool newInstance);

		/// <summary>
		/// Create a duplicate of this instance if it is semantically valid to
		/// copy it to a new context.
		/// </summary>
		/// <param name="duplicate">The duplicate.</param>
		/// <returns>True if the duplicate was created.</returns>
		bool TryDuplicateForNewContext(out IComponentRegistration duplicate);

		/// <summary>
		/// Fired when a new instance is being activated. The instance can be
		/// wrapped or switched at this time by setting the Instance property in
		/// the provided event arguments.
		/// </summary>
		event EventHandler<ActivatingEventArgs> Activating;

		/// <summary>
		/// Fired when the activation process for a new instance is complete.
		/// </summary>
		event EventHandler<ActivatedEventArgs> Activated;

        /// <summary>
        /// Called by the container once an instance has been fully constructed, including
        /// any requested objects that depend on the instance.
        /// </summary>
        /// <param name="context">The context in which the instance was activated.</param>
        /// <param name="instance">The instance.</param>
        void InstanceActivated(IContext context, object instance);
    }
}
