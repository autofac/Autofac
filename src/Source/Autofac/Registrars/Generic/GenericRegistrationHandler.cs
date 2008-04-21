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
using System.Linq;
using Autofac.Component;
using Autofac.Component.Activation;
using Autofac.Component.Scope;

namespace Autofac.Registrars.Generic
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
	public class GenericRegistrationHandler : ReflectiveRegistrationSource
	{
		IEnumerable<Type> _serviceTypes;
		Type _implementor;

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericRegistrationHandler"/> class.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <param name="implementor">The implementor.</param>
        /// <param name="deferredParams">The deferred params.</param>
        /// <param name="constructorSelector">The constructor selector.</param>
		public GenericRegistrationHandler(
			IEnumerable<Service> services,
			Type implementor,
            DeferredRegistrationParameters deferredParams,
            IConstructorSelector constructorSelector
        )
            : base(deferredParams, constructorSelector)
		{
            Enforce.ArgumentNotNull(services, "services");
            Enforce.ArgumentNotNull(implementor, "implementor");

            foreach (var service in services)
                if (!(service is TypedService) || !(((TypedService)service).ServiceType.IsGenericType))
                    throw new ArgumentException(
                        string.Format(GenericRegistrationHandlerResources.ServiceNotGenericType,
                            service));

			// Checking that implementor supports services is non-trivial,
			// constraints etc. make this best left to the runtime.

			_serviceTypes = services.Select(s => ((TypedService)s).ServiceType);
			_implementor = implementor;
		}

        /// <summary>
        /// Determine if the service represents a type that can be registered, and if so,
        /// retrieve that type as well as the services that the registration should expose.
        /// </summary>
        /// <param name="service">The service.</param>
        /// <param name="implementor">The implementation type.</param>
        /// <param name="services">The services.</param>
        /// <returns>True if a registration can be made.</returns>
        protected override bool TryGetImplementation(Service service, out Type implementor, out IEnumerable<Service> services)
        {
            Enforce.ArgumentNotNull(service, "service");
            implementor = null;
            services = null;
            
            TypedService typedService = service as TypedService;
            if (typedService == null)
                return false;

            if (!typedService.ServiceType.IsGenericType ||
                !_serviceTypes.Contains(typedService.ServiceType.GetGenericTypeDefinition()))
                return false;

            var args = typedService.ServiceType.GetGenericArguments();
            implementor = _implementor.MakeGenericType(args);

            services = (from abs in _serviceTypes
                        select new TypedService(abs.MakeGenericType(args))).Cast<Service>();
            
            return true;
        }
	}
}
