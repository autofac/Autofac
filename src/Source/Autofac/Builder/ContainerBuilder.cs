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
using Autofac.Registrars;

namespace Autofac.Builder
{
	/// <summary>
	/// Used to incrementally build component registrations.
	/// </summary>
	public class ContainerBuilder
	{
		private IList<IModule> _registrars = new List<IModule>();
		private bool _wasBuilt;
		private InstanceOwnership _defaultInstanceOwnership = InstanceOwnership.Container;
		private InstanceScope _defaultInstanceScope = InstanceScope.Singleton;

        /// <summary>
        /// Set the default <see cref="InstanceOwnership"/> for new registrations. Registrations
        /// already made will not be affected by changes in this value.
        /// </summary>
        /// <param name="ownership">The new default ownership.</param>
        /// <returns></returns>
		public IDisposable SetDefaultOwnership(InstanceOwnership ownership)
		{
            InstanceOwnership oldValue = _defaultInstanceOwnership;
    		_defaultInstanceOwnership = ownership;
            return new Guard(() => _defaultInstanceOwnership = oldValue);
		}

        /// <summary>
        /// The default <see cref="InstanceOwnership"/> for new registrations.
        /// </summary>
        public InstanceOwnership DefaultOwnership
        {
            get
            {
                return _defaultInstanceOwnership;
            }
        }

		/// <summary>
		/// Set the default <see cref="InstanceScope"/> for new registrations. Registrations
		/// already made will not be affected by changes in this value.
		/// </summary>
        public IDisposable SetDefaultScope(InstanceScope scope)
		{
            InstanceScope oldValue = _defaultInstanceScope;
            _defaultInstanceScope = scope;
            return new Guard(() => _defaultInstanceScope = oldValue);
		}

        /// <summary>
        /// The default <see cref="InstanceScope"/> for new registrations.
        /// </summary>
        public InstanceScope DefaultScope
        {
            get
            {
                return _defaultInstanceScope;
            }
        }

		/// <summary>
		/// Add a module to the container.
		/// </summary>
		/// <param name="module">The module to add.</param>
		public void RegisterModule(IModule module)
		{
            Enforce.ArgumentNotNull(module, "module");
			_registrars.Add(module);
		}

        /// <summary>
        /// Register a component using a component registration.
        /// </summary>
        /// <param name="registration"></param>
        public void RegisterComponent(IComponentRegistration registration)
        {
            Enforce.ArgumentNotNull(registration, "registration");
            RegisterModule(new RegistrationRegistrar(registration));
        }

        /// <summary>
		/// Create a new container with the registrations that have been built so far.
		/// </summary>
		/// <remarks>
		/// Build can only be called once per ContainerBuilder - this prevents lifecycle
		/// issues for provided instances.
		/// </remarks>
		/// <returns>A new container with the registrations made.</returns>
		public IContainer Build()
		{
			var result = new Container();
			Build(result);
			return result;
		}

		/// <summary>
		/// Configure an existing container with the registrations that have been built so far.
		/// </summary>
		/// <remarks>
		/// Build can only be called once per ContainerBuilder - this prevents lifecycle
		/// issues for provided instances.
		/// </remarks>
		/// <param name="container">An existing container to make the registrations in.</param>
		public void Build(IContainer container)
		{
            Enforce.ArgumentNotNull(container, "container");

			if (_wasBuilt)
				throw new InvalidOperationException();

			_wasBuilt = true;

			foreach (IModule registrar in _registrars)
				registrar.Configure(container);
		}
	}
}
