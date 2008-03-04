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
using Autofac.Component;
using Autofac.Component.Activation;
using Autofac.Component.Scope;

namespace Autofac.Registrars.Automatic
{
    /// <summary>
    /// Provides registrations for any requested type.
    /// </summary>
    class AutomaticRegistrationHandler : IRegistrationSource
    {
        Predicate<Type> _predicate;
		InstanceOwnership _ownership;
		InstanceScope _scope;
        IEnumerable<EventHandler<ActivatingEventArgs>> _activatingHandlers;
        IEnumerable<EventHandler<ActivatedEventArgs>> _activatedHandlers;

        /// <summary>
        /// Initializes a new instance of the <see cref="AutomaticRegistrationHandler"/> class.
        /// </summary>
        /// <param name="predicate">The predicate.</param>
        /// <param name="ownership">The ownership.</param>
        /// <param name="scope">The scope.</param>
        /// <param name="activatingHandlers">The activating handlers.</param>
        /// <param name="activatedHandlers">The activated handlers.</param>
        public AutomaticRegistrationHandler(
            Predicate<Type> predicate,
			InstanceOwnership ownership,
			InstanceScope scope,
            IEnumerable<EventHandler<ActivatingEventArgs>> activatingHandlers,
            IEnumerable<EventHandler<ActivatedEventArgs>> activatedHandlers
        )
		{
            _predicate = Enforce.ArgumentNotNull(predicate, "predicate");
            _activatingHandlers = Enforce.ArgumentNotNull(activatingHandlers, "activatingHandlers");
            _activatedHandlers = Enforce.ArgumentNotNull(activatedHandlers, "activatedHandlers");
            _ownership = ownership;
			_scope = scope;
		}

		/// <summary>
		/// Retrieve a registration for an unregistered service, to be used
		/// by the container.
		/// </summary>
		/// <param name="service">The service that was requested.</param>
		/// <param name="registration">A registration providing the service.</param>
		/// <returns>True if the registration could be created.</returns>
		public bool TryGetRegistration(Service service, out IComponentRegistration registration)
		{
            Enforce.ArgumentNotNull(service, "service");

            registration = null;

            TypedService typedService = service as TypedService;
            if (typedService == null)
                return false;

            if (typedService.ServiceType.IsAbstract ||
                !_predicate(typedService.ServiceType))
                return false;

            var reg = new Registration(
                new UniqueService(),
                new[] { service },
                new ReflectionActivator(typedService.ServiceType),
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
