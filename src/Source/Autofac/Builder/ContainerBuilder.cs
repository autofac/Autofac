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
using Autofac.Registrars;
using Autofac.Registrars.Automatic;
using Autofac.Registrars.Collection;
using Autofac.Registrars.Delegate;
using Autofac.Registrars.Generic;
using Autofac.Registrars.ProvidedInstance;
using Autofac.Registrars.Reflective;

namespace Autofac.Builder
{
	/// <summary>
	/// Used to incrementally build component registrations.
	/// </summary>
	public class ContainerBuilder
	{
		private readonly IList<IModule> _registrars = new List<IModule>();
		private InstanceOwnership _defaultInstanceOwnership = InstanceOwnership.Container;
		private InstanceScope _defaultInstanceScope = InstanceScope.Singleton;
		private bool _wasBuilt;

		/// <summary>
		/// The default <see cref="InstanceOwnership"/> for new registrations.
		/// </summary>
		public virtual InstanceOwnership DefaultOwnership
		{
			get { return _defaultInstanceOwnership; }
		}

		/// <summary>
		/// The default <see cref="InstanceScope"/> for new registrations.
		/// </summary>
		public virtual InstanceScope DefaultScope
		{
			get { return _defaultInstanceScope; }
		}

		/// <summary>
		/// Set the default <see cref="InstanceOwnership"/> for new registrations. Registrations
		/// already made will not be affected by changes in this value.
		/// </summary>
		/// <param name="ownership">The new default ownership.</param>
		/// <returns>An object that when disposed, will restore the original default ownership.</returns>
		public virtual IDisposable SetDefaultOwnership(InstanceOwnership ownership)
		{
			InstanceOwnership oldValue = _defaultInstanceOwnership;
			_defaultInstanceOwnership = ownership;
			return new Guard(() => _defaultInstanceOwnership = oldValue);
		}

		/// <summary>
		/// Set the default <see cref="InstanceScope"/> for new registrations. Registrations
		/// already made will not be affected by changes in this value.
		/// </summary>
		/// <returns>An object that when disposed, will restore the original default scope.</returns>
		public virtual IDisposable SetDefaultScope(InstanceScope scope)
		{
			InstanceScope oldValue = _defaultInstanceScope;
			_defaultInstanceScope = scope;
			return new Guard(() => _defaultInstanceScope = oldValue);
		}

		/// <summary>
		/// Add a module to the container.
		/// </summary>
		/// <param name="module">The module to add.</param>
		public virtual void RegisterModule(IModule module)
		{
			Enforce.ArgumentNotNull(module, "module");
			_registrars.Add(module);
		}

		/// <summary>
		/// Register a component using a component registration.
		/// </summary>
		/// <param name="registration"></param>
		public virtual void RegisterComponent(IComponentRegistration registration)
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
		public virtual IContainer Build()
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
		public virtual void Build(IContainer container)
		{
			Enforce.ArgumentNotNull(container, "container");

			if (_wasBuilt)
				throw new InvalidOperationException();

			_wasBuilt = true;

			foreach (IModule registrar in _registrars)
				registrar.Configure(container);
		}

		/// <summary>
		/// Attach the registrar to the builder (register it as a module) and
		/// set defaults as necessary.
		/// </summary>
		/// <param name="registrar"></param>
		/// <returns></returns>
		internal virtual TRegistrar AttachRegistrar<TRegistrar>(Registrar<TRegistrar> registrar)
			where TRegistrar : IRegistrar<TRegistrar>
		{
			Enforce.ArgumentNotNull(registrar, "registrar");
			RegisterModule(registrar);
			return registrar
				.WithOwnership(DefaultOwnership)
				.WithScope(DefaultScope);
		}

		/// <summary>
		/// Register an un-parameterised generic type, e.g. <code>Repository&lt;&gt;</code>.
		/// Concrete types will be made as they are requested, e.g. with <code>Resolve&lt;Repository&lt;int&gt;&gt;()</code>.
		/// </summary>
		/// <param name="implementor">The implementor.</param>
		/// <returns>A registrar allowing configuration to continue.</returns>
		public virtual IGenericRegistrar RegisterGeneric(Type implementor)
		{
			Enforce.ArgumentNotNull(implementor, "implementor");
			return AttachRegistrar(new GenericRegistrar(implementor));
		}

		/// <summary>
		/// Register a component to be created through reflection.
		/// </summary>
		/// <typeparam name="T">The type of the component.</typeparam>
		/// <returns>A registrar allowing details of the registration to be customised.</returns>
		public virtual IReflectiveRegistrar Register<T>()
		{
			return Register(typeof(T));
		}

		/// <summary>
		/// Register a component to be created through reflection.
		/// </summary>
		/// <param name="implementor">The type of the component.</param>
		/// <returns>
		/// A registrar allowing details of the registration to be customised.
		/// </returns>
		public virtual IReflectiveRegistrar Register(Type implementor)
		{
			Enforce.ArgumentNotNull(implementor, "implementor");
			return AttachRegistrar(new ReflectiveRegistrar(implementor));
		}

		/// <summary>
		/// Registers the types matching.
		/// </summary>
		/// <param name="predicate">The predicate.</param>
		/// <returns>A registrar allowing configuration to continue.</returns>
		public virtual IGenericRegistrar RegisterTypesMatching(Predicate<Type> predicate)
		{
			Enforce.ArgumentNotNull(predicate, "predicate");
			return AttachRegistrar(new AutomaticRegistrar(predicate));
		}

		/// <summary>
		/// Registers the types from assembly.
		/// </summary>
		/// <param name="assembly">The assembly.</param>
		/// <returns>A registrar allowing configuration to continue.</returns>
		public virtual IGenericRegistrar RegisterTypesFromAssembly(Assembly assembly)
		{
			Enforce.ArgumentNotNull(assembly, "assembly");
			return RegisterTypesMatching(t => t.Assembly == assembly);
		}

		/// <summary>
		/// Registers the types assignable to.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns>A registrar allowing configuration to continue.</returns>
		public virtual IGenericRegistrar RegisterTypesAssignableTo<T>()
		{
			return RegisterTypesMatching(t => typeof(T).IsAssignableFrom(t));
		}

        /// <summary>
        /// Registers the type as a collection. If no services or names are specified, the
        /// default service will be IEnumerable&lt;T&gt;();
        /// </summary>
        /// <typeparam name="T">Collection item type</typeparam>
        /// <returns>A registrar allowing configuration to continue.</returns>
        public virtual IConcreteRegistrar RegisterCollection<T>()
        {
            return AttachRegistrar(new CollectionRegistrar<T>());
        }

        /// <summary>
        /// Registers the type as a collection. If no services or names are specified, the
        /// default service will be IEnumerable&lt;T&gt;();
        /// </summary>
        /// <param name="collectionType">Collection item type</param>
        /// <returns>A registrar allowing configuration to continue.</returns>
        public virtual IConcreteRegistrar RegisterCollection(Type collectionType)
        {
            Enforce.ArgumentNotNull(collectionType, "collectionType");
            return AttachRegistrar(
                (Registrar<IConcreteRegistrar>)
                Activator.CreateInstance(typeof(CollectionRegistrar<>).MakeGenericType(collectionType)));
        }

        /// <summary>
		/// Register a component that will be created using a provided delegate.
		/// </summary>
		/// <typeparam name="T">The type of the component.</typeparam>
		/// <param name="creator">The creator.</param>
		/// <returns>
		/// A registrar allowing details of the registration to be customised.
		/// </returns>
		public virtual IConcreteRegistrar Register<T>(ComponentActivator<T> creator)
		{
			Enforce.ArgumentNotNull(creator, "creator");
			return Register((c, p) => creator(c));
		}

		/// <summary>
		/// Register a component that will be created using a provided delegate.
		/// </summary>
		/// <typeparam name="T">The type of the component.</typeparam>
		/// <param name="creator">The creator.</param>
		/// <returns>
		/// A registrar allowing details of the registration to be customised.
		/// </returns>
		public virtual IConcreteRegistrar Register<T>(ComponentActivatorWithParameters<T> creator)
		{
			Enforce.ArgumentNotNull(creator, "creator");

			return AttachRegistrar(new DelegateRegistrar(typeof(T), (c, p) => creator(c, p)));
		}

		/// <summary>
		/// Register a component using a provided instance.
		/// </summary>
		/// <typeparam name="T">The type of the component.</typeparam>
		/// <param name="instance">The instance.</param>
		/// <returns>
		/// A registrar allowing details of the registration to be customised.
		/// </returns>
		public virtual IConcreteRegistrar Register<T>(T instance)
		{
			// Scope of instances is always singleton, this will throw an exception
			// if the default is otherwise.
			return AttachRegistrar(new ProvidedInstanceRegistrar(instance, typeof(T)));
		}
	}
}