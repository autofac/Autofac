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

namespace Autofac
{
    /// <summary>
    /// Standard container implementation.
    /// </summary>
	public class Container : Disposable, IRegistrationContext, IContext, IContainer, IServiceProvider
    {
        #region Fields

        /// <summary>
        /// Protects instance variables from concurrent access.
        /// </summary>
        readonly object _synchRoot = new object();

        /// <summary>
        /// Associates each service with the default registration that
        /// can provide that service.
        /// </summary>
        readonly IDictionary<Service, IComponentRegistration> _defaultRegistrations = new Dictionary<Service, IComponentRegistration>();

		/// <summary>
		/// Supports nested containers.
		/// </summary>
		readonly Container _outerContainer;

		/// <summary>
		/// External registration sources.
		/// </summary>
		readonly IList<IRegistrationSource> _registrationSources = new List<IRegistrationSource>();

        /// <summary>
        /// Disposer that handles disposal of instances attached to the container.
        /// </summary>
        readonly IDisposer _disposer = new Disposer();

        // *** WARNING *** The order of declaration is significant here - SelfRegistrationDescriptor must be
        // created before Empty.
        
        /// <summary>
        /// When creating inner containers the construction of the descriptor was previously
        /// the most expensive operation - using a shared descriptor eliminates this.
        /// </summary>
        static readonly IComponentDescriptor SelfRegistrationDescriptor =
            new Component.Descriptor(
                new UniqueService(),
                new Service[] { new TypedService(typeof(IContainer)), new TypedService(typeof(IContext)) },
                typeof(Container));

        /// <summary>
        /// A container with no component registrations.
        /// </summary>
        public static readonly Container Empty = new Container();

        #endregion

        #region Initialisation

        /// <summary>
		/// Create a new container.
		/// </summary>
		public Container()
        {
            RegisterComponent(
                new Component.Registration(
                    SelfRegistrationDescriptor,
                    new Component.Activation.ProvidedInstanceActivator(this),
                    new Component.Scope.SingletonScope(),
                    InstanceOwnership.External));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Container"/> class.
        /// </summary>
        /// <param name="outerScope">The outer scope.</param>
		protected Container(Container outerScope)
		: this()
		{
            Enforce.ArgumentNotNull(outerScope, "outerScope");
			_outerContainer = outerScope;
		}

        #endregion

        #region IContainer Support

        /// <summary>
        /// Begin a new sub-context. Contextual and transient instances created inside
        /// the subcontext will be disposed along with it.
        /// </summary>
        /// <returns>A new subcontext.</returns>
        public virtual IContainer CreateInnerContainer()
        {
            return new Container(this);
        }

        /// <summary>
        /// Register a component.
        /// </summary>
        /// <param name="registration">A component registration.</param>
        public virtual void RegisterComponent(IComponentRegistration registration)
        {
            RegisterComponentInternal(registration, null);
        }

        /// <summary>
        /// Registers the component, restricting its provided services to 'specificService' in order
        /// to protect pre-existing defaults if necessary.
        /// </summary>
        /// <param name="registration">The registration.</param>
        /// <param name="specificService">The specific service or null.</param>
        void RegisterComponentInternal(IComponentRegistration registration, Service specificService)
        {
            Enforce.ArgumentNotNull(registration, "registration");

            lock (_synchRoot)
            {
                CheckNotDisposed();

                _disposer.AddInstanceForDisposal(registration);

                if (specificService != null)
                {
                    SetDefaultRegistrationForService(registration.Descriptor.Id, registration);
                    SetDefaultRegistrationForService(specificService, registration);
                }
                else
                {
                    foreach (Service service in registration.Descriptor.Services)
                        SetDefaultRegistrationForService(service, registration);
                }

                FireComponentRegistered(registration);
            }
        }

        void SetDefaultRegistrationForService(Service service, IComponentRegistration registration)
        {
            Enforce.ArgumentNotNull(service, "service");
            Enforce.ArgumentNotNull(registration, "registration");
            _defaultRegistrations[service] = registration;
        }

        void FireComponentRegistered(IComponentRegistration registration)
        {
            Enforce.ArgumentNotNull(registration, "registration");
            var args = new ComponentRegisteredEventArgs(this, registration);
            ComponentRegistered(this, args);
        }

		/// <summary>
		/// Add a source from which registrations may be retrieved in the case that they
		/// are not available in the container.
		/// </summary>
		/// <param name="registrationSource"></param>
		public virtual void AddRegistrationSource(IRegistrationSource registrationSource)
		{
            Enforce.ArgumentNotNull(registrationSource, "registrationSource");
			_registrationSources.Add(registrationSource);
		}

        /// <summary>
        /// The disposer associated with this container. Instances can be associated
        /// with it manually if required.
        /// </summary>
        public virtual IDisposer Disposer
        {
            get
            {
                return _disposer;
            }
        }

        /// <summary>
        /// If the container is an inner container, retrieves the outer container.
        /// Otherwise, null;
        /// </summary>
        /// <value></value>
        public virtual IContainer OuterContainer
        {
            get
            {
                return _outerContainer;
            }
        }
        
        /// <summary>
        /// The registrations for all of the components registered with the container.
        /// </summary>
        public virtual IEnumerable<IComponentRegistration> ComponentRegistrations
        {
        	get
        	{
        		lock (_synchRoot)
        		{
        			return _defaultRegistrations.Values.Distinct().ToList();
        		}
        	}
        }

        /// <summary>
        /// Fired whenever a component is registed into the container.
        /// </summary>
        public virtual event EventHandler<ComponentRegisteredEventArgs> ComponentRegistered = (sender, e) => { };
        
        /// <summary>
        /// Gets the default component registration that will be used to satisfy
        /// requests for the provided service.
        /// </summary>
        /// <param name="service">The service.</param>
        /// <param name="registration">The registration.</param>
        /// <returns>
        /// True if a default exists, false otherwise.
        /// </returns>
        public virtual bool TryGetDefaultRegistrationFor(Service service, out IComponentRegistration registration)
        {
            Enforce.ArgumentNotNull(service, "service");
            IDisposer unused;
            IContext unused2;
            return ((IRegistrationContext)this).TryGetRegistration(service, out registration, out unused, out unused2);
        }

        /// <summary>
        /// Enables context tagging in the container.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tag">The tag applied to this container and the contexts genrated when
        /// it resolves component dependencies.</param>
        public virtual void TagWith<T>(T tag)
        {
        	lock(_synchRoot)
        	{
	            if (!IsRegistered<ContextTag<T>>())
	            {
	                RegisterComponent(
	                    new Component.Registration(
	                        new Component.Descriptor(
	                            new UniqueService(),
	                            new[] { new TypedService(typeof(ContextTag<T>)) },
	                            typeof(ContextTag<T>)),
	                        new Component.Activation.DelegateActivator((c, p) => new ContextTag<T>()),
	                        new Component.Scope.ContainerScope(),
	                        InstanceOwnership.Container));
	            }
	
	            Resolve<ContextTag<T>>().Tag = tag;
        	}
        }
        
        #endregion

        #region Registration Context Support

        /// <summary>
        /// Gets a registration from the container by key.
        /// </summary>
        /// <param name="key">The key for the registration (name or generated service key.)</param>
        /// <param name="registration">The registration result.</param>
        /// <param name="disposer">The disposer that should be used to dispose of instances activated by
        /// the registration.</param>
        /// <param name="context">The context that should be used when activating instances from the
        /// registration.</param>
        /// <returns></returns>
        /// <remarks>
        /// This method has gotten a bit gnarly. It might help to replace the three 'out' parameters
        /// with a single structure, though usage in Context doesn't really point that way.
        /// </remarks>
        bool IRegistrationContext.TryGetRegistration(Service key, out IComponentRegistration registration, out IDisposer disposer, out IContext context)
        {
            Enforce.ArgumentNotNull(key, "key");
            context = null;

            lock (_synchRoot)
            {
                CheckNotDisposed();

                if (((IRegistrationContext)this).TryGetLocalRegistration(key, out registration, out disposer))
                    return true;

                if (_outerContainer != null &&
                    ((IRegistrationContext)_outerContainer).TryGetRegistration(key, out registration, out disposer, out context))
                {
                    if (context == null)
                        context = _outerContainer;
                    return true;
                }

                return false;
            }
        }

        /// <summary>
        /// Gets a registration from the specific outer container by key.
        /// </summary>
        /// <param name="key">The key for the registration (name or generated service key.)</param>
        /// <param name="registration">The registration result.</param>
        /// <param name="disposer">The disposer that should be used to dispose of instances activated by
        /// the registration.</param>
        /// <returns>True if a registration is available.</returns>
        bool IRegistrationContext.TryGetLocalRegistration(Service key, out IComponentRegistration registration, out IDisposer disposer)
        {
            Enforce.ArgumentNotNull(key, "key");
            disposer = null;

            lock (_synchRoot)
            {
                CheckNotDisposed();

                if (_defaultRegistrations.TryGetValue(key, out registration) ||
                    TryGetRegistrationFromSources(key, out registration))
                {
                    disposer = Disposer;
                    return true;
                }

                if (_outerContainer != null)
                {
                    Service expectedImplementer;
                    if (_outerContainer.TryGetDefaultImplementer(key, out expectedImplementer) &&
                        _defaultRegistrations.TryGetValue(expectedImplementer, out registration))
                    {
                        SetDefaultRegistrationForService(key, registration);
                        disposer = Disposer;
                        return true;
                    }

                    if (_outerContainer.TryExportToNewContext(key, out registration))
                    {
                        RegisterComponentInternal(registration, key);
                        disposer = Disposer;
                        return true;
                    }
                }

                return false;
            }
        }

        /// <summary>
        /// Searches for an IRegistrationSource that can provide a registration for an
        /// unregistered service, and completes the registration process if possible.
        /// </summary>
        /// <param name="key">The requested service.</param>
        /// <param name="registration">The registration for that service.</param>
        /// <returns>
        /// True if a registration was provided, otherwise, false.
        /// </returns>
        bool TryGetRegistrationFromSources(Service key, out IComponentRegistration registration)
        {
            Enforce.ArgumentNotNull(key, "key");

            registration = null;

            foreach (IRegistrationSource registrationSource in _registrationSources)
            {
                if (registrationSource.TryGetRegistration(key, out registration))
                {
                    if (!registration.Descriptor.Services.Contains(key))
                    {
                        registration.Dispose();
                        throw new ArgumentException(ContainerResources.RequiredServiceNotSupported);
                    }

                    RegisterComponent(registration);
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Create an IComponentRegistration for a new subcontext if available.
        /// </summary>
        /// <param name="key">The service that was requested. Note that any
        /// additional services provided by the component will also be exported.</param>
        /// <param name="registration">The new registration.</param>
        /// <returns>True if the new context could be supported.</returns>
        bool TryExportToNewContext(Service key, out IComponentRegistration registration)
        {
            Enforce.ArgumentNotNull(key, "key");

            lock (_synchRoot)
            {
                IComponentRegistration localRegistration;
                if (_defaultRegistrations.TryGetValue(key, out localRegistration))
                {
                    return localRegistration.DuplicateForNewContext(out registration);
                }
                else if (TryGetRegistrationFromSources(key, out localRegistration))
                {
                	return localRegistration.DuplicateForNewContext(out registration);
                }
                else if (_outerContainer != null)
                {
                    return _outerContainer.TryExportToNewContext(key, out registration);
                }
                else
                {
                    registration = null;
                    return false;
                }
            }
        }

        bool TryGetDefaultImplementer(Service key, out Service id)
        {
            IComponentRegistration registration;
            if (_defaultRegistrations.TryGetValue(key, out registration))
            {
                id = registration.Descriptor.Id;
                return true;
            }
            else
            {
                id = null;
                return false;
            }
        }

        #endregion

        #region IDisposable Support

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
                lock (_synchRoot)
                {
                    Disposer.Dispose();
                }
        }

        #endregion

        #region IContext Support

        /// <summary>
        /// Creates the context for a single resolve operation.
        /// </summary>
        /// <returns>The context.</returns>
        protected virtual IContext CreateResolutionContext()
        {
            return new Context(this);
        }

        /// <summary>
        /// Retrieve a service registered with the container.
        /// </summary>
        /// <typeparam name="TService">The service to retrieve.</typeparam>
        /// <param name="parameters"></param>
        /// <returns>
        /// The component instance that provides the service.
        /// </returns>
        /// <exception cref="ComponentNotRegisteredException"/>
        /// <exception cref="DependencyResolutionException"/>
        public virtual TService Resolve<TService>(params Parameter[] parameters)
        {
            return CreateResolutionContext().Resolve<TService>(parameters);
        }

        /// <summary>
        /// Retrieve a service registered with the container.
        /// </summary>
        /// <typeparam name="TService">The type to which the result will be cast.</typeparam>
        /// <param name="serviceName">Name of the service.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns>
        /// The component instance that provides the service.
        /// </returns>
        /// <exception cref="ComponentNotRegisteredException"/>
        /// <exception cref="DependencyResolutionException"/>
        public virtual TService Resolve<TService>(string serviceName, params Parameter[] parameters)
        {
            return CreateResolutionContext().Resolve<TService>(serviceName, parameters);
        }

        /// <summary>
        /// Retrieve a service registered with the container.
        /// </summary>
        /// <param name="serviceType">The service to retrieve.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns>
        /// The component instance that provides the service.
        /// </returns>
        /// <exception cref="ComponentNotRegisteredException"/>
        /// <exception cref="DependencyResolutionException"/>
        public virtual object Resolve(Type serviceType, params Parameter[] parameters)
        {
            return CreateResolutionContext().Resolve(serviceType, parameters);
        }

        /// <summary>
        /// Retrieve a service registered with the container.
        /// </summary>
        /// <param name="serviceName">The service to retrieve.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns>
        /// The component instance that provides the service.
        /// </returns>
        /// <exception cref="ComponentNotRegisteredException"/>
        /// <exception cref="DependencyResolutionException"/>
        public virtual object Resolve(string serviceName, params Parameter[] parameters)
        {
            return CreateResolutionContext().Resolve(serviceName, parameters);
        }

        /// <summary>
        /// Retrieve a service registered with the container.
        /// </summary>
        /// <param name="service">The service to retrieve.</param>
        /// <param name="parameters"></param>
        /// <returns>
        /// The component instance that provides the service.
        /// </returns>
        /// <exception cref="ComponentNotRegisteredException"/>
        /// <exception cref="DependencyResolutionException"/>
        public virtual object Resolve(Service service, params Parameter[] parameters)
        {
            return CreateResolutionContext().Resolve(service, parameters);
        }

        /// <summary>
        /// Retrieve a service registered with the container.
        /// </summary>
        /// <typeparam name="TService">The service to retrieve.</typeparam>
        /// <param name="instance">The component instance that provides the service.</param>
        /// <param name="parameters"></param>
        /// <returns>
        /// True if the service was registered and its instance created;
        /// false otherwise.
        /// </returns>
        /// <exception cref="DependencyResolutionException"/>
        public virtual bool TryResolve<TService>(out TService instance, params Parameter[] parameters)
        {
            return CreateResolutionContext().TryResolve<TService>(out instance, parameters);
        }

        /// <summary>
        /// Retrieve a service registered with the container.
        /// </summary>
        /// <param name="serviceType">The service to retrieve.</param>
        /// <param name="instance">The component instance that provides the service.</param>
        /// <param name="parameters"></param>
        /// <returns>
        /// True if the service was registered and its instance created;
        /// false otherwise.
        /// </returns>
        /// <exception cref="DependencyResolutionException"/>
        public virtual bool TryResolve(Type serviceType, out object instance, params Parameter[] parameters)
        {
            return CreateResolutionContext().TryResolve(serviceType, out instance, parameters);
        }

        /// <summary>
        /// Retrieve a service registered with the container.
        /// </summary>
        /// <typeparam name="TService">The service to retrieve.</typeparam>
        /// <param name="parameters"></param>
        /// <returns>
        /// The component instance that provides the service, or null if
        /// none is available.
        /// </returns>
        /// <remarks>Useful with the C#3 initialiser syntax.</remarks>
        /// <example>
        /// container.Register&lt;ISomething&gt;(c =&gt; new Something(){ AProperty = c.ResolveOptional&lt;IOptional&gt;() });
        /// </example>
        public virtual TService ResolveOptional<TService>(params Parameter[] parameters)
        {
            return CreateResolutionContext().ResolveOptional<TService>(parameters);
        }

        /// <summary>
        /// Determines whether the specified service is registered.
        /// </summary>
        /// <param name="service">The service.</param>
        /// <returns>
        /// 	<c>true</c> if the specified service is registered; otherwise, <c>false</c>.
        /// </returns>
        public virtual bool IsRegistered(Type service)
        {
            return CreateResolutionContext().IsRegistered(service);
        }

        /// <summary>
        /// Determines whether the specified service is registered.
        /// </summary>
        /// <param name="name">The service.</param>
        /// <returns>
        /// 	<c>true</c> if the specified service is registered; otherwise, <c>false</c>.
        /// </returns>
        public virtual bool IsRegistered(string name)
        {
            return CreateResolutionContext().IsRegistered(name);
        }

        /// <summary>
        /// Determines whether the specified service is registered.
        /// </summary>
        /// <param name="service">The service.</param>
        /// <returns>
        /// 	<c>true</c> if the specified service is registered; otherwise, <c>false</c>.
        /// </returns>
        public virtual bool IsRegistered(Service service)
        {
            return CreateResolutionContext().IsRegistered(service);
        }

        /// <summary>
        /// Determine whether or not a service has been registered.
        /// </summary>
        /// <typeparam name="TService">The service to test for the registration of.</typeparam>
        /// <returns>True if the service is registered.</returns>
        public virtual bool IsRegistered<TService>()
        {
            return CreateResolutionContext().IsRegistered<TService>();
        }

        /// <summary>
        /// Set any properties on <paramref name="instance"/> that can be
        /// resolved by the container. (Generally use <see cref="InjectUnsetProperties"/>
        /// unless you're using the Null Object pattern for unset dependencies.)
        /// </summary>
        /// <typeparam name="T">Type of instance. Used only to provide method chaining.</typeparam>
        /// <param name="instance">The instance to inject properties into.</param>
        /// <returns><paramref name="instance"/>.</returns>
        public virtual T InjectProperties<T>(T instance)
        {
            return CreateResolutionContext().InjectProperties<T>(instance);
        }

        /// <summary>
        /// Set any null-valued properties on <paramref name="instance"/> that can be
        /// resolved by the container.
        /// </summary>
        /// <typeparam name="T">Type of instance. Used only to provide method chaining.</typeparam>
        /// <param name="instance">The instance to inject properties into.</param>
        /// <returns><paramref name="instance"/>.</returns>
        public virtual T InjectUnsetProperties<T>(T instance)
        {
            return CreateResolutionContext().InjectUnsetProperties<T>(instance);
        }

        /// <summary>
        /// Retrieve a service registered with the container.
        /// </summary>
        /// <param name="componentName">The name of the component to retrieve.</param>
        /// <param name="instance">The component instance that provides the service.</param>
        /// <param name="parameters"></param>
        /// <returns>
        /// True if the service was registered and its instance created;
        /// false otherwise.
        /// </returns>
        /// <exception cref="DependencyResolutionException"/>
        public virtual bool TryResolve(string componentName, out object instance, params Parameter[] parameters)
        {
            return CreateResolutionContext().TryResolve(componentName, out instance, parameters);
        }

        /// <summary>
        /// Retrieve a service registered with the container.
        /// </summary>
        /// <param name="service">The key of the component to retrieve.</param>
        /// <param name="instance">The component instance that provides the service.</param>
        /// <param name="parameters"></param>
        /// <returns>
        /// True if the service was registered and its instance created;
        /// false otherwise.
        /// </returns>
        /// <exception cref="DependencyResolutionException"/>
        public virtual bool TryResolve(Service service, out object instance, params Parameter[] parameters)
        {
            return CreateResolutionContext().TryResolve(service, out instance, parameters);
        }

        #endregion

        #region IServiceProvider Members

        /// <summary>
        /// Gets the service object of the specified type.
        /// </summary>
        /// <param name="serviceType">An object that specifies the type of service object 
        /// to get.</param>
        /// <returns>
        /// A service object of type <paramref name="serviceType"/>.-or- null if there is 
        /// no service object of type <paramref name="serviceType"/>.
        /// </returns>
        object IServiceProvider.GetService(Type serviceType)
        {
            Enforce.ArgumentNotNull(serviceType, "serviceType");
            return IsRegistered(serviceType) ? Resolve(serviceType) : null;
        }

        #endregion
    }
}
