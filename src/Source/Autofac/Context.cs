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
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace Autofac
{
    /// <summary>
    /// Provides dependency resolution during a single resolve operation.
    /// </summary>
    class Context : IContext
    {
        #region Inner classes

        /// <summary>
        /// Tracks activation events that need to be fired.
        /// </summary>
        class Activation
        {
            private IComponentRegistration Registration { get; set; }
            private IContext Context { get; set; }
            private object Instance { get; set; }

            public Activation(IContext context, IComponentRegistration registration, object instance)
            {
                Context = Enforce.ArgumentNotNull(context, "context");
                Registration = Enforce.ArgumentNotNull(registration, "registration");
                Instance = Enforce.ArgumentNotNull(instance, "instance");
            }

            public void Activated()
            {
                Registration.InstanceActivated(Context, Instance);
            }
        }

        #endregion

        #region Fields

        /// <summary>
        /// A context with no registered components.
        /// </summary>
        public static readonly Context Empty = new Context(Container.Empty);

        IRegistrationContext _registrationContext;
        
        IList<Activation> _activations = new List<Activation>();
        
		/// <summary>
		/// For the duration of a single resolve operation, tracks the services
		/// that have been requested.
		/// </summary>
        Stack<Service> _componentResolutionStack = new Stack<Service>();

        /// <summary>
        /// Catch circular dependencies that are triggered by post-resolve processing (e.g. 'OnActivated')
        /// </summary>
        const int MaxResolveDepth = 100;
        int _resolveDepth = 0;

        #endregion

        #region Initialisation

        /// <summary>
        /// Initializes a new instance of the <see cref="Context"/> class.
        /// </summary>
        /// <param name="registrationContext">The container from which to draw component registrations.</param>
        internal Context(IRegistrationContext registrationContext)
        {
            Enforce.ArgumentNotNull(registrationContext, "registrationContext");
            _registrationContext = registrationContext;
        }

        #endregion

        #region IContext members

        /// <summary>
        /// Retrieve a service registered with the container.
        /// </summary>
        /// <typeparam name="TService">The service to retrieve.</typeparam>
        /// <returns>
        /// The component instance that provides the service.
        /// </returns>
        /// <exception cref="ComponentNotRegisteredException"/>
        /// <exception cref="DependencyResolutionException"/>
        public TService Resolve<TService>(params Parameter[] parameters)
        {
            Enforce.ArgumentNotNull(parameters, "parameters");
            return this.Resolve<TService>(this.MakeActivationParameters(parameters));
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
        public TService Resolve<TService>(string serviceName, params Parameter[] parameters)
        {
            Enforce.ArgumentNotNull(serviceName, "serviceName");
            Enforce.ArgumentNotNull(parameters, "parameters");
            return this.Resolve<TService>(serviceName, this.MakeActivationParameters(parameters));
        }

        /// <summary>
        /// Retrieve a service registered with the container.
        /// </summary>
        /// <param name="service">The service to retrieve.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns>
        /// The component instance that provides the service.
        /// </returns>
        /// <exception cref="ComponentNotRegisteredException"/>
        /// <exception cref="DependencyResolutionException"/>
        public object Resolve(Service service, params Parameter[] parameters)
        {
            Enforce.ArgumentNotNull(service, "service");
            Enforce.ArgumentNotNull(parameters, "parameters");
            return this.Resolve(service, this.MakeActivationParameters(parameters));
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
        public object Resolve(Type serviceType, params Parameter[] parameters)
        {
            Enforce.ArgumentNotNull(serviceType, "serviceType");
            Enforce.ArgumentNotNull(parameters, "parameters");
            return Resolve(new TypedService(serviceType), this.MakeActivationParameters(parameters));
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
        public object Resolve(string serviceName, params Parameter[] parameters)
        {
            Enforce.ArgumentNotNull(serviceName, "serviceName");
            Enforce.ArgumentNotNull(parameters, "parameters");
            return Resolve(new NamedService(serviceName), this.MakeActivationParameters(parameters));
        }

        /// <summary>
        /// Retrieve a service registered with the container.
        /// </summary>
        /// <typeparam name="TService">The service to retrieve.</typeparam>
        /// <param name="parameters">The parameters.</param>
        /// <returns>
        /// The component instance that provides the service, or null if
        /// none is available.
        /// </returns>
        /// <remarks>Useful with the C#3 initialiser syntax.</remarks>
        /// <example>
        /// container.Register&lt;ISomething&gt;(c =&gt; new Something(){ AProperty = c.ResolveOptional&lt;IOptional&gt;() });
        /// </example>
        public TService ResolveOptional<TService>(params Parameter[] parameters)
        {
            Enforce.ArgumentNotNull(parameters, "parameters");
            return ResolveOptional<TService>(this.MakeActivationParameters(parameters));
        }

        /// <summary>
        /// Retrieve a service registered with the container.
        /// </summary>
        /// <typeparam name="TService">The service to retrieve.</typeparam>
        /// <param name="instance">The component instance that provides the service.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns>
        /// True if the service was registered and its instance created;
        /// false otherwise.
        /// </returns>
        /// <exception cref="DependencyResolutionException"/>
        public bool TryResolve<TService>(out TService instance, params Parameter[] parameters)
        {
            Enforce.ArgumentNotNull(parameters, "parameters");
            return this.TryResolve<TService>(out instance, this.MakeActivationParameters(parameters));
        }

        /// <summary>
        /// Retrieve a service registered with the container.
        /// </summary>
        /// <param name="serviceType">The service to retrieve.</param>
        /// <param name="instance">The component instance that provides the service.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns>
        /// True if the service was registered and its instance created;
        /// false otherwise.
        /// </returns>
        /// <exception cref="DependencyResolutionException"/>
        public bool TryResolve(Type serviceType, out object instance, params Parameter[] parameters)
        {
            Enforce.ArgumentNotNull(serviceType, "serviceType");
            Enforce.ArgumentNotNull(parameters, "parameters");
            return TryResolve(new TypedService(serviceType), out instance, this.MakeActivationParameters(parameters));
        }

        /// <summary>
        /// Retrieve a service registered with the container.
        /// </summary>
        /// <param name="componentName">The name of the component to retrieve.</param>
        /// <param name="instance">The component instance that provides the service.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns>
        /// True if the service was registered and its instance created;
        /// false otherwise.
        /// </returns>
        /// <exception cref="DependencyResolutionException"/>
        public bool TryResolve(string componentName, out object instance, params Parameter[] parameters)
        {
            Enforce.ArgumentNotNull(componentName, "componentName");
            Enforce.ArgumentNotNull(parameters, "parameters");
            return TryResolve(new NamedService(componentName), out instance, this.MakeActivationParameters(parameters));
        }

        /// <summary>
        /// Retrieve a service registered with the container.
        /// </summary>
        /// <param name="service">The key of the component to retrieve.</param>
        /// <param name="instance">The component instance that provides the service.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns>
        /// True if the service was registered and its instance created;
        /// false otherwise.
        /// </returns>
        /// <exception cref="DependencyResolutionException"/>
        public bool TryResolve(Service service, out object instance, params Parameter[] parameters)
        {
            Enforce.ArgumentNotNull(service, "service");
            Enforce.ArgumentNotNull(parameters, "parameters");
            return this.TryResolve(service, out instance, this.MakeActivationParameters(parameters));
        }

        /// <summary>
        /// Determine whether or not a service has been registered.
        /// </summary>
        /// <param name="service">The service to test for the registration of.</param>
        /// <returns>True if the service is registered.</returns>
        public bool IsRegistered(Service service)
        {
            IComponentRegistration unused1;
            IDisposer unused2;
            IContext unused3;
            return _registrationContext.TryGetRegistration(service, out unused1, out unused2, out unused3);
        }

        /// <summary>
        /// Determine whether or not a service has been registered.
        /// </summary>
        /// <param name="serviceType">The service to test for the registration of.</param>
        /// <returns>True if the service is registered.</returns>
        public bool IsRegistered(Type serviceType)
        {
            Enforce.ArgumentNotNull(serviceType, "serviceType");
            return IsRegistered(new TypedService(serviceType));
        }

        /// <summary>
        /// Determine whether or not a service has been registered.
        /// </summary>
        /// <param name="serviceName">The service to test for the registration of.</param>
        /// <returns>True if the service is registered.</returns>
        public bool IsRegistered(string serviceName)
        {
            Enforce.ArgumentNotNullOrEmpty(serviceName, "serviceName");
            return IsRegistered(new NamedService(serviceName));
        }

        /// <summary>
        /// Determine whether or not a service has been registered.
        /// </summary>
        /// <typeparam name="TService">The service to test for the registration of.</typeparam>
        /// <returns>True if the service is registered.</returns>
        public bool IsRegistered<TService>()
        {
            return IsRegistered(typeof(TService));
        }

        /// <summary>
        /// Set any null-valued properties on <paramref name="instance"/> that can be
        /// resolved by the container.
        /// </summary>
        /// <typeparam name="T">Type of instance. Used only to provide method chaining.</typeparam>
        /// <param name="instance">The instance to inject properties into.</param>
        /// <returns><paramref name="instance"/>.</returns>
        /// <remarks>
        /// Inspect all public writeable properties and inject
        /// values from the container if available. For factory-lifecycle components
        /// a speed improvement could be had here by caching the property-value
        /// pairs.
        /// </remarks>
        public T InjectUnsetProperties<T>(T instance)
        {
            return InjectProperties(instance, false);
        }

        /// <summary>
        /// Set any properties on <paramref name="instance"/> that can be
        /// resolved by the container. (Generally use <see cref="InjectUnsetProperties"/>
        /// unless you're using the Null Object pattern for unset dependencies.)
        /// </summary>
        /// <typeparam name="T">Type of instance. Used only to provide method chaining.</typeparam>
        /// <param name="instance">The instance to inject properties into.</param>
        /// <returns><paramref name="instance"/>.</returns>
        public T InjectProperties<T>(T instance)
        {
            return InjectProperties(instance, true);
        }

        /// <summary>
        /// Set any properties on <paramref name="instance"/> that can be
        /// resolved by the container.
        /// </summary>
        /// <typeparam name="T">Type of instance. Used only to provide method chaining.</typeparam>
        /// <param name="instance">The instance to inject properties into.</param>
        /// <param name="overrideSetValues">If set to <c>true</c> any properties with existing
        /// values will be overwritten.</param>
        /// <returns><paramref name="instance"/>.</returns>
        private T InjectProperties<T>(T instance, bool overrideSetValues)
        {
            if (!typeof(T).IsValueType && (object)instance == null)
                throw new ArgumentNullException("instance");

            Type instanceType = instance.GetType();

            foreach (PropertyInfo property in instanceType.GetProperties(
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.SetProperty))
            {
                Type propertyType = property.PropertyType;

                if (propertyType.IsValueType)
                    continue;

                if (property.GetIndexParameters().Length != 0)
                    continue;

                if (!IsRegistered(propertyType))
                    continue;

                var accessors = property.GetAccessors(false);
                if (accessors.Length == 1 && accessors[0].ReturnType != typeof(void))
                    continue;

                if (!overrideSetValues &&
                    accessors.Length == 2 &&
                    (property.GetValue(instance, null) != null))
                    continue;

                object propertyValue = Resolve(propertyType);
                property.SetValue(instance, propertyValue, null);
            }

            return instance;
        }

        #endregion

        IActivationParameters MakeActivationParameters(Parameter[] parameters)
        {
            Enforce.ArgumentNotNull(parameters, "parameters");

            if (parameters.Length == 0)
                return ActivationParameters.Empty;

            var result = new ActivationParameters(parameters.Length);
            foreach (var namedValue in parameters)
                result.Add(namedValue.Name, namedValue.Value);
            return result;
        }

        void ActivationsComplete()
        {
            var activations = _activations;
            _activations = new List<Activation>();
            foreach (Activation activation in activations)
                activation.Activated();
        }

        string CreateDependencyGraphTo(Service service)
        {
            Enforce.ArgumentNotNull(service, "service");

            string dependencyGraph = service.Description;

            foreach (Service requestor in _componentResolutionStack)
                dependencyGraph = requestor.Description + " -> " + dependencyGraph;

            return dependencyGraph;
        }

        bool IsCircularDependency(Service service)
        {
            Enforce.ArgumentNotNull(service, "service");

            return _componentResolutionStack.Count(i => i == service) > 1;
        }

        #region IContext Members

        public TService Resolve<TService>(IActivationParameters parameters)
        {
            Enforce.ArgumentNotNull(parameters, "parameters");
            return (TService)Resolve(typeof(TService), parameters);
        }

        public TService Resolve<TService>(string serviceName, IActivationParameters parameters)
        {
            Enforce.ArgumentNotNull(serviceName, "serviceName");
            Enforce.ArgumentNotNull(parameters, "parameters");
            return (TService)Resolve(new NamedService(serviceName), parameters);
        }

        public object Resolve(Type serviceType, IActivationParameters parameters)
        {
            Enforce.ArgumentNotNull(serviceType, "serviceType");
            Enforce.ArgumentNotNull(parameters, "parameters");
            return Resolve(new TypedService(serviceType), parameters);
        }

        public object Resolve(string serviceName, IActivationParameters parameters)
        {
            Enforce.ArgumentNotNull(serviceName, "serviceName");
            Enforce.ArgumentNotNull(parameters, "parameters");
            return Resolve(new NamedService(serviceName), parameters);
        }

        public object Resolve(Service service, IActivationParameters parameters)
        {
            Enforce.ArgumentNotNull(service, "service");
            Enforce.ArgumentNotNull(parameters, "parameters");

            object result = null;

            if (!TryResolve(service, out result, parameters))
                throw new ComponentNotRegisteredException(service);

            return result;
        }

        public bool TryResolve<TService>(out TService instance, IActivationParameters parameters)
        {
            Enforce.ArgumentNotNull(parameters, "parameters");
            object untypedInstance = null;
            bool result = TryResolve(typeof(TService), out untypedInstance, parameters);
            instance = (TService)untypedInstance;
            return result;
        }

        public bool TryResolve(Type serviceType, out object instance, IActivationParameters parameters)
        {
            Enforce.ArgumentNotNull(serviceType, "serviceType");
            Enforce.ArgumentNotNull(parameters, "parameters");
            return TryResolve(new TypedService(serviceType), out instance, parameters);
        }

        public bool TryResolve(string componentName, out object instance, IActivationParameters parameters)
        {
            Enforce.ArgumentNotNull(componentName, "componentName");
            Enforce.ArgumentNotNull(parameters, "parameters");
            return TryResolve(new NamedService(componentName), out instance, parameters);
        }

        public bool TryResolve(Service service, out object instance, IActivationParameters parameters)
        {
            Enforce.ArgumentNotNull(service, "service");
            Enforce.ArgumentNotNull(parameters, "parameters");

            instance = null;
            if (++_resolveDepth > MaxResolveDepth)
                throw new DependencyResolutionException(string.Format(CultureInfo.CurrentCulture,
                    ContextResources.MaxDepthExceeded, service));

            try
            {
                IComponentRegistration registration;
                IDisposer disposer;
                IContext specificContext;
                if (!_registrationContext.TryGetRegistration(service, out registration, out disposer, out specificContext))
                    return false;

                if (specificContext != null)
                    return specificContext.TryResolve(service, out instance, parameters);

                if (IsCircularDependency(service))
                    throw new DependencyResolutionException(string.Format(CultureInfo.CurrentCulture,
                        ContextResources.CircularDependency, CreateDependencyGraphTo(service)));

                bool newInstance;
                _componentResolutionStack.Push(service);
                try
                {
                    instance = registration.ResolveInstance(this, parameters, disposer, out newInstance);

                    if (newInstance)
                        _activations.Add(new Activation(this, registration, instance));
                }
                finally
                {
                    _componentResolutionStack.Pop();
                }

                if (_componentResolutionStack.Count == 0)
                    ActivationsComplete();

                return true;
            }
            finally
            {
                --_resolveDepth;
            }
        }

        public TService ResolveOptional<TService>(IActivationParameters parameters)
        {
            Enforce.ArgumentNotNull(parameters, "parameters");
            object result;
            TryResolve(typeof(TService), out result, parameters);
            return (TService)result;
        }

        #endregion
    }
}
