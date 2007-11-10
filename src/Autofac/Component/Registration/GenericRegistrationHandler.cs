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
using Autofac.Component.Activation;
using Autofac.Component.Scope;

namespace Autofac.Component.Registration
{
	/// <summary>
	/// This class, not yet part of the public API, provides a means
	/// by which generic component registrations can be made.
	/// </summary>
	/// <remarks>
	/// This class creates registrations by handling the Container.ServiceNotRegistered
	/// event. This feels hacky but keeps the container cleaner and simpler. Will have
	/// to see how performance goes.
	/// </remarks>
	class GenericRegistrationHandler : IRegistrationSource
	{
		IEnumerable<Type> _services;
		Type _implementor;
		InstanceOwnership _ownership;
		InstanceScope _scope;
        IEnumerable<EventHandler<ActivatingEventArgs>> _activatingHandlers;
        IEnumerable<EventHandler<ActivatedEventArgs>> _activatedHandlers;

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericRegistrationHandler"/> class.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <param name="implementor">The implementor.</param>
        /// <param name="ownership">The ownership.</param>
        /// <param name="scope">The scope.</param>
        /// <param name="activatingHandlers">The activating handlers.</param>
        /// <param name="activatedHandlers">The activated handlers.</param>
		public GenericRegistrationHandler(
			IEnumerable<Type> services,
			Type implementor,
			InstanceOwnership ownership,
			InstanceScope scope,
            IEnumerable<EventHandler<ActivatingEventArgs>> activatingHandlers,
            IEnumerable<EventHandler<ActivatedEventArgs>> activatedHandlers
        )
		{
            Enforce.ArgumentNotNull(services, "services");
            Enforce.ArgumentNotNull(implementor, "implementor");
            Enforce.ArgumentNotNull(activatingHandlers, "activatingHandlers");
            Enforce.ArgumentNotNull(activatedHandlers, "activatedHandlers");

			// Checking that implementor supports services is non-trivial,
			// constraints etc. make this best left to the runtime.

			_services = services;
			_implementor = implementor;
			_ownership = ownership;
			_scope = scope;
            _activatingHandlers = activatingHandlers;
            _activatedHandlers = activatedHandlers;
		}

		/// <summary>
		/// Retrieve a registration for an unregistered service, to be used
		/// by the container.
		/// </summary>
		/// <param name="service">The service that was requested.</param>
		/// <param name="registration">A registration providing the service.</param>
		/// <returns>True if the registration could be created.</returns>
		public bool TryGetRegistration(Type service, out IComponentRegistration registration)
		{
            Enforce.ArgumentNotNull(service, "service");

			if (!service.IsGenericType || !_services.Contains(service.GetGenericTypeDefinition()))
			{
				registration = null;
				return false;
			}

			var args = service.GetGenericArguments();
			var concrete = _implementor.MakeGenericType(args);
			var services = _services.Select(abs => abs.MakeGenericType(args));
			var reg = new ComponentRegistration(
				services,
				new ReflectionActivator(concrete),
				_scope.ToIScope(),
				_ownership);

			foreach (var activatingHandler in _activatingHandlers)
				reg.Activating += activatingHandler;

			foreach (var activatedHandler in _activatedHandlers)
				reg.Activated += activatedHandler;

			registration = reg;
			return true;
		}

	}
}
