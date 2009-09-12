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
using Autofac.Component;
using Autofac.Component.Activation;
using Autofac.Component.Scope;
using System.Linq;

namespace Autofac.Registrars
{
    /// <summary>
    /// This class provides a common base for registration handlers that provide
    /// reflection-based registrations.
    /// </summary>
    public abstract class ReflectiveRegistrationSource : IRegistrationSource
    {
        DeferredRegistrationParameters _deferredParams;
        IConstructorSelector _constructorSelector;
        IEnumerable<Parameter> _arguments;
        IEnumerable<NamedPropertyParameter> _properties;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReflectiveRegistrationSource"/> class.
        /// </summary>
        /// <param name="deferredParams">The deferred params.</param>
        /// <param name="constructorSelector">The constructor selector.</param>
        /// <param name="arguments">Additional arguments to supply to the component's constructor.</param>
        /// <param name="properties">Additional properties to set on the component.</param>
        protected ReflectiveRegistrationSource(
            DeferredRegistrationParameters deferredParams,
            IConstructorSelector constructorSelector,
            IEnumerable<Parameter> arguments,
            IEnumerable<NamedPropertyParameter> properties
        )
        {
            _deferredParams = Enforce.ArgumentNotNull(deferredParams, "deferredParams");
            _constructorSelector = Enforce.ArgumentNotNull(constructorSelector, "constructorSelector");
            _arguments = Enforce.ArgumentNotNull(arguments, "arguments");
            _properties = Enforce.ArgumentNotNull(properties, "properties");
        }

        /// <summary>
        /// Retrieve a registration for an unregistered service, to be used
        /// by the container.
        /// </summary>
        /// <param name="service">The service that was requested.</param>
        /// <param name="registeredServicesTest">A predicate that can be queried to determine if a service is already registered.</param>
        /// <param name="registration">A registration providing the service.</param>
        /// <returns>True if the registration could be created.</returns>
        public virtual bool TryGetRegistration(Service service, Func<Service, bool> registeredServicesTest, out IComponentRegistration registration)
        {
            Enforce.ArgumentNotNull(service, "service");
            registration = null;

            Type concrete;
            IEnumerable<Service> services;
            if (!TryGetImplementation(service, out concrete, out services))
                return false;

            var descriptor = new Descriptor(
                    new UniqueService(),
                    services,
                    concrete);

            var activator = new ReflectionActivator(
                    concrete,
                    _arguments,
                    _properties,
                    _constructorSelector);

            registration = CreateRegistration(descriptor, activator);
            return true;
        }

        /// <summary>
        /// Determine if the service represents a type that can be registered, and if so,
        /// retrieve that type as well as the services that the registration should expose.
        /// </summary>
        /// <param name="service">The service.</param>
        /// <param name="implementor">The implementation type.</param>
        /// <param name="services">The services.</param>
        /// <returns>True if a registration can be made.</returns>
        protected abstract bool TryGetImplementation(Service service, out Type implementor, out IEnumerable<Service> services);

        IComponentRegistration CreateRegistration(IComponentDescriptor descriptor, IActivator activator)
        {
            Enforce.ArgumentNotNull(descriptor, "descriptor");
            Enforce.ArgumentNotNull(activator, "activator");

            var reg = _deferredParams.RegistrationCreator(
                descriptor,
                activator,
                _deferredParams.Scope.ToIScope(),
                _deferredParams.Ownership);

            foreach (var preparingHandler in _deferredParams.PreparingHandlers)
                reg.Preparing += preparingHandler;

            foreach (var activatingHandler in _deferredParams.ActivatingHandlers)
                reg.Activating += activatingHandler;

            foreach (var activatedHandler in _deferredParams.ActivatedHandlers)
                reg.Activated += activatedHandler;

            return reg;
        }
    }
}
