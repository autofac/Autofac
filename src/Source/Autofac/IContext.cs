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

namespace Autofac
{
	/// <summary>
	/// The context in which a service can be accessed or a component's
	/// dependencies resolved. Disposal of a context will dispose any owned
	/// components.
	/// </summary>
	/// <remarks>
	/// This interface is intended to be the lowest-common-denominator between
	/// IoC containers - the Autofac-specific features are generally available
	/// through the Container class only. By restricting wider application access
	/// to this interface, an application can remain compatible with other
	/// containers and be adapted to them via this interface if necessary.
	/// </remarks>
	public interface IContext
	{
        /// <summary>
        /// Retrieve a service registered with the container.
        /// </summary>
        /// <typeparam name="TService">The service to retrieve.</typeparam>
        /// <returns>The component instance that provides the service.</returns>
        /// <exception cref="ComponentNotRegisteredException" />
        /// <exception cref="DependencyResolutionException" />
        TService Resolve<TService>(params Parameter[] parameters);

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
        TService Resolve<TService>(string serviceName, params Parameter[] parameters);

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
        object Resolve(Type serviceType, params Parameter[] parameters);

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
        object Resolve(string serviceName, params Parameter[] parameters);

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
        object Resolve(Service service, params Parameter[] parameters);

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
        bool TryResolve<TService>(out TService instance, params Parameter[] parameters);

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
        bool TryResolve(Type serviceType, out object instance, params Parameter[] parameters);

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
        bool TryResolve(string componentName, out object instance, params Parameter[] parameters);

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
        bool TryResolve(Service service, out object instance, params Parameter[] parameters);

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
		TService ResolveOptional<TService>(params Parameter[] parameters);

        /// <summary>
        /// Retrieve a service registered with the container.
        /// </summary>
        /// <typeparam name="TService">The service to retrieve.</typeparam>
        /// <returns>The component instance that provides the service.</returns>
        /// <exception cref="ComponentNotRegisteredException" />
        /// <exception cref="DependencyResolutionException" />
        TService Resolve<TService>(IEnumerable<Parameter> parameters);

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
        TService Resolve<TService>(string serviceName, IEnumerable<Parameter> parameters);

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
        object Resolve(Type serviceType, IEnumerable<Parameter> parameters);

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
        object Resolve(string serviceName, IEnumerable<Parameter> parameters);

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
        object Resolve(Service service, IEnumerable<Parameter> parameters);

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
        bool TryResolve<TService>(out TService instance, IEnumerable<Parameter> parameters);

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
        bool TryResolve(Type serviceType, out object instance, IEnumerable<Parameter> parameters);

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
        bool TryResolve(string componentName, out object instance, IEnumerable<Parameter> parameters);

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
        bool TryResolve(Service service, out object instance, IEnumerable<Parameter> parameters);

        /// <summary>
        /// Retrieve a service registered with the container.
        /// </summary>
        /// <typeparam name="TService">The service to retrieve.</typeparam>
        /// <param name="parameters">The parameters.</param>
        /// <returns>
        /// The component instance that provides the service, or null if
        /// none is available.
        /// </returns>
        /// <remarks>Useful with the C#3 initializer syntax.</remarks>
        /// <example>
        /// container.Register&lt;ISomething&gt;(c =&gt; new Something(){ AProperty = c.ResolveOptional&lt;IOptional&gt;() });
        /// </example>
        TService ResolveOptional<TService>(IEnumerable<Parameter> parameters);


		/// <summary>
		/// Retrieve a service registered with the container
		/// </summary>
		/// <typeparam name="TService">The type of the service to retrieve.</typeparam>
		/// <param name="serviceName">Name of the service to retrieve.</param>
		/// <param name="parameters">The parameters.</param>
		/// <returns>
		/// The component instance that provides the service, or null if
		/// none is available
		/// </returns>
		TService ResolveOptional<TService>(string serviceName, IEnumerable<Parameter> parameters);

		/// <summary>
		/// Retrieve a service registered with the container
		/// </summary>
		/// <typeparam name="TService">The type of the service to retrieve.</typeparam>
		/// <param name="serviceName">Name of the service to retrieve.</param>
		/// <param name="parameters">The parameters.</param>
		/// <returns>
		/// The component instance that provides the service, or null if
		/// none is available
		/// </returns>
		TService ResolveOptional<TService>(string serviceName, params Parameter[] parameters);

        /// <summary>
        /// Determine whether or not a service has been registered.
        /// </summary>
        /// <param name="serviceType">The service to test for the registration of.</param>
        /// <returns>True if the service is registered.</returns>
        bool IsRegistered(Type serviceType);

        /// <summary>
        /// Determine whether or not a service has been registered.
        /// </summary>
        /// <param name="serviceName">The service to test for the registration of.</param>
        /// <returns>True if the service is registered.</returns>
        bool IsRegistered(string serviceName);

        /// <summary>
        /// Determine whether or not a service has been registered.
        /// </summary>
        /// <param name="service">The service to test for the registration of.</param>
        /// <returns>True if the service is registered.</returns>
        bool IsRegistered(Service service);

		/// <summary>
		/// Determine whether or not a service has been registered.
		/// </summary>
		/// <typeparam name="TService">The service to test for the registration of.</typeparam>
		/// <returns>True if the service is registered.</returns>
		bool IsRegistered<TService>();

		/// <summary>
		/// Set any properties on <paramref name="instance"/> that can be
		/// resolved by the container. (Generally use <see cref="InjectUnsetProperties"/>
		/// unless you're using the Null Object pattern for unset dependencies.)
		/// </summary>
		/// <typeparam name="T">Type of instance. Used only to provide method chaining.</typeparam>
		/// <param name="instance">The instance to inject properties into.</param>
		/// <returns><paramref name="instance"/>.</returns>
		T InjectProperties<T>(T instance);

		/// <summary>
		/// Set any null-valued properties on <paramref name="instance"/> that can be
		/// resolved by the container.
		/// </summary>
		/// <typeparam name="T">Type of instance. Used only to provide method chaining.</typeparam>
		/// <param name="instance">The instance to inject properties into.</param>
		/// <returns><paramref name="instance"/>.</returns>
		T InjectUnsetProperties<T>(T instance);
	}
}
