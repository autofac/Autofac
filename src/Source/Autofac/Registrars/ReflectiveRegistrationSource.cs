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

namespace Autofac.Registrars
{
    /// <summary>
    /// This class provides a common base for registration handlers that provide
    /// reflection-based registrations.
    /// </summary>
    abstract class ReflectiveRegistrationSource : IRegistrationSource
    {
        InstanceOwnership _ownership;
        InstanceScope _scope;
        IEnumerable<EventHandler<ActivatingEventArgs>> _activatingHandlers;
        IEnumerable<EventHandler<ActivatedEventArgs>> _activatedHandlers;
        RegistrationCreator _createRegistration;
        IConstructorSelector _constructorSelector;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReflectiveRegistrationSource"/> class.
        /// </summary>
        /// <param name="ownership">The ownership.</param>
        /// <param name="scope">The scope.</param>
        /// <param name="activatingHandlers">The activating handlers.</param>
        /// <param name="activatedHandlers">The activated handlers.</param>
        /// <param name="createRegistration">The registration creator.</param>
        /// <param name="constructorSelector">The constructor selector.</param>
        protected ReflectiveRegistrationSource(
            InstanceOwnership ownership,
            InstanceScope scope,
            IEnumerable<EventHandler<ActivatingEventArgs>> activatingHandlers,
            IEnumerable<EventHandler<ActivatedEventArgs>> activatedHandlers,
            RegistrationCreator createRegistration,
            IConstructorSelector constructorSelector
        )
        {
            _ownership = ownership;
            _scope = scope;
            _activatingHandlers = Enforce.ArgumentNotNull(activatingHandlers, "activatingHandlers"); ;
            _activatedHandlers = Enforce.ArgumentNotNull(activatedHandlers, "activatedHandlers");
            _createRegistration = Enforce.ArgumentNotNull(createRegistration, "createRegistration"); ;
            _constructorSelector = Enforce.ArgumentNotNull(constructorSelector, "constructorSelector");
        }

        /// <summary>
        /// Retrieve a registration for an unregistered service, to be used
        /// by the container.
        /// </summary>
        /// <param name="service">The service that was requested.</param>
        /// <param name="registration">A registration providing the service.</param>
        /// <returns>True if the registration could be created.</returns>
        public virtual bool TryGetRegistration(Service service, out IComponentRegistration registration)
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
                    ActivationParameters.Empty,
                    ActivationParameters.Empty,
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

            var reg = _createRegistration(
                descriptor,
                activator,
                _scope.ToIScope(),
                _ownership);

            foreach (var activatingHandler in _activatingHandlers)
                reg.Activating += activatingHandler;

            foreach (var activatedHandler in _activatedHandlers)
                reg.Activated += activatedHandler;

            return reg;
        }
    }
}
