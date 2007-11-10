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
using Autofac.Component.Activation;
using Autofac.Component;
using Autofac.Component.Registration;

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
        /// Register a component to be created through reflection.
        /// </summary>
        /// <typeparam name="T">The type of the component.</typeparam>
        /// <returns>A registrar allowing details of the registration to be customised.</returns>
		public IReflectiveRegistrar Register<T>()
		{
			return Register(typeof(T));
		}

        /// <summary>
        /// Register a component to be created through reflection.
        /// </summary>
        /// <param name="implementor">The type of the component.</param>
        /// <returns>A registrar allowing details of the registration to be customised.</returns>
        public IReflectiveRegistrar Register(Type implementor)
        {
            Enforce.ArgumentNotNull(implementor, "implementor");
            var result = new ReflectionRegistrar(implementor);
            AddComponentRegistrar(result);
            return result;
        }

        /// <summary>
        /// Set the defaults on a regular component registrar then add it to the
        /// list of registrars.
        /// </summary>
        /// <param name="registrar"></param>
        private void AddComponentRegistrar(ComponentRegistrar registrar)
        {
            Enforce.ArgumentNotNull(registrar, "registrar");
            registrar.WithOwnership(_defaultInstanceOwnership);
            registrar.WithScope(_defaultInstanceScope);
            RegisterModule(registrar);
        }

        /// <summary>
        /// Register a component using a provided instance.
        /// </summary>
        /// <typeparam name="T">The type of the component.</typeparam>
        /// <param name="instance">The instance.</param>
        /// <returns>
        /// A registrar allowing details of the registration to be customised.
        /// </returns>
        public IRegistrar Register<T>(T instance)
		{
			var result = new ProvidedInstanceRegistrar(instance);
            AddComponentRegistrar(result);
			return result;
		}

        /// <summary>
        /// Register a component that will be created using a provided delegate.
        /// </summary>
        /// <typeparam name="T">The type of the component.</typeparam>
        /// <param name="creator">The creator.</param>
        /// <returns>
        /// A registrar allowing details of the registration to be customised.
        /// </returns>
        public IRegistrar Register<T>(ComponentActivator<T> creator)
        {
            Enforce.ArgumentNotNull(creator, "creator");
            return Register<T>((c, p) => creator(c));
        }

        /// <summary>
        /// Register a component that will be created using a provided delegate.
        /// </summary>
        /// <typeparam name="T">The type of the component.</typeparam>
        /// <param name="creator">The creator.</param>
        /// <returns>
        /// A registrar allowing details of the registration to be customised.
        /// </returns>
        public IRegistrar Register<T>(ComponentActivatorWithParameters<T> creator)
        {
            Enforce.ArgumentNotNull(creator, "creator");
            var result = new DelegateRegistrar(typeof(T), (c, p) => creator(c, p));
            AddComponentRegistrar(result);
            return result;
        }

        /// <summary>
        /// Register the IList&gt;T&lt;, ICollection&gt;T&lt; and IEnumerable&gt;T&lt;
        /// interfaces for a service. Subsequent to this call, any registrations made
        /// that expose the service will be accumulated in the collection registration.
        /// </summary>
        /// <remarks>
        /// T must not already be registered as a service.
        /// </remarks>
        /// <typeparam name="T">The service type that the collection will accumulate.</typeparam>
        public void RegisterAsCollection<T>()
        {
            RegisterAsCollection(typeof(T));
        }
        /// <summary>
        /// Register the IList&gt;T&lt;, ICollection&gt;T&lt; and IEnumerable&gt;T&lt;
        /// interfaces for a service. Subsequent to this call, any registrations made
        /// that expose the service will be accumulated in the collection registration.
        /// </summary>
        /// <remarks>
        /// T must not already be registered as a service.
        /// </remarks>
        /// <param name="collectedService">The service type that the collection will accumulate.</param>
        public void RegisterAsCollection(Type collectedService)
        {
            Enforce.ArgumentNotNull(collectedService, "collectedService");
            RegisterModule(new CollectionRegistrar(collectedService));
        }

		/// <summary>
		/// Add a module to the container.
		/// </summary>
		/// <param name="module">The module to add.</param>
		public void RegisterModule(IModule module)
		{
			_registrars.Add(module);
		}

		/// <summary>
		/// Register an un-parameterised generic type, e.g. <code>Repository&lt;&gt</code>.
		/// Concrete types will be made as they are requested, e.g. with <code>Resolve&lt;Repository&lt;int&gt;&gt;()</code>.
		/// </summary>
		/// <returns></returns>
		public IRegistrar RegisterGeneric(Type implementor)
		{
            Enforce.ArgumentNotNull(implementor, "implementor");
			var result = new GenericRegistrar(implementor);
            RegisterModule(result);
			return result;
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
        /// Register a regular component activator (with parameters)
        /// but expose it through a delegate type. The resolved delegate's
        /// parameters will be provided to the activator as named values.
        /// </summary>
        /// <typeparam name="TCreator">Delegate type that will be resolvable.</typeparam>
        /// <param name="creator">Activator that will create the result value of
        /// calls to the delegate.</param>
        public void RegisterFactory<TCreator>(ComponentActivator creator)
        {
            RegisterFactory(typeof(TCreator), creator);
        }

        /// <summary>
        /// Register a regular component activator (with parameters)
        /// but expose it through a delegate type. The resolved delegate's
        /// parameters will be provided to the activator as named values.
        /// </summary>
        /// <typeparam name="TCreator">Delegate type that will be resolvable.</typeparam>
        /// <param name="creator">Activator that will create the result value of
        /// calls to the delegate.</param>
        public void RegisterFactory(Type factoryDelegate, ComponentActivator creator)
        {
            Enforce.ArgumentNotNull(creator, "creator");
            Enforce.ArgumentNotNull(factoryDelegate, "factoryDelegate");

            RegisterComponent(new ContextAwareDelegateRegistration(factoryDelegate, creator));
        }
        
        /// <summary>
		/// Create a new container with the registrations that have been built so far.
		/// </summary>
		/// <remarks>
		/// Build can only be called once per ContainerBuilder - this prevents lifecycle
		/// issues for provided instances.
		/// </remarks>
		/// <returns>A new container with the registrations made.</returns>
		public Container Build()
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
		public void Build(Container container)
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
