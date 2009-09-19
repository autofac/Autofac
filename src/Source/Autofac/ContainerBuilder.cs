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
using System.Reflection;

namespace Autofac
{
	/// <summary>
	/// Used to incrementally build component registrations for a container.
	/// </summary>
	public class ContainerBuilder
	{
        private readonly IList<Action<IComponentRegistry>> _configurationCallbacks = new List<Action<IComponentRegistry>>();
		private bool _wasBuilt;

        /// <summary>
        /// Register a callback that will be invoked when the container is configured.
        /// </summary>
        /// <param name="configurationCallback">Callback to execute.</param>
        public virtual void RegisterCallback(Action<IComponentRegistry> configurationCallback)
        {
            _configurationCallbacks.Add(Enforce.ArgumentNotNull(configurationCallback, "configurationCallback"));
        }

		/// <summary>
		/// Create a new container with the registrations that have been built so far.
		/// </summary>
		/// <remarks>
		/// Build can only be called once per ContainerBuilder - this prevents lifecycle
		/// issues for provided instances.
		/// </remarks>
		/// <returns>A new container with the registrations made.</returns>
		public virtual IContainer Build()
		{
			var result = new Container();
			Build(result.ComponentRegistry);
			return result;
		}

		/// <summary>
		/// Configure an existing comopnent registry with the registrations that have been built so far.
		/// </summary>
		/// <remarks>
		/// Build can only be called once per ContainerBuilder - this prevents lifecycle
		/// issues for provided instances.
		/// </remarks>
        /// <param name="componentRegistry">An existing component registry to make the registrations in.</param>
		public virtual void Build(IComponentRegistry componentRegistry)
		{
            Enforce.ArgumentNotNull(componentRegistry, "componentRegistry");

			if (_wasBuilt)
				throw new InvalidOperationException();

			_wasBuilt = true;

			foreach (var callback in _configurationCallbacks)
                callback(componentRegistry);
		}
	}
}