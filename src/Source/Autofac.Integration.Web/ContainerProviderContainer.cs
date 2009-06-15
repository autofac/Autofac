// Contributed by Chad Lee 2009-06-15
// Copyright (c) 2007 - 2009 Autofac Contributors
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

namespace Autofac.Integration.Web
{
    /// <summary>
    /// Provides an implementation of <see cref="Autofac.IContainer"/> which uses the configured
    /// <see cref="Autofac.Integration.Web.IContainerProvider"/> to route calls to the current request container.
    /// </summary>
    public class ContainerProviderContainer : IContainer
    {
        private readonly IContainerProvider containerProvider;

        /// <summary>
        /// Initializes a new instance of <see cref="Autofac.Integration.Web.ContainerProviderContainer"/>.
        /// </summary>
        /// <param name="containerProvider">The <see cref="Autofac.Integration.Web.IContainerProvider"/> to use to retrieve the current request container.</param>
        public ContainerProviderContainer(IContainerProvider containerProvider)
        {
            if (containerProvider == null)
                throw new ArgumentNullException("containerProvider");

            this.containerProvider = containerProvider;
        }

        #region Decorator Implementation

        public IContainer CreateInnerContainer()
        {
            return containerProvider.RequestContainer.CreateInnerContainer();
        }

        public void RegisterComponent(IComponentRegistration registration)
        {
            containerProvider.RequestContainer.RegisterComponent(registration);
        }

        public void AddRegistrationSource(IRegistrationSource source)
        {
            containerProvider.RequestContainer.AddRegistrationSource(source);
        }

        public IDisposer Disposer
        {
            get { return containerProvider.RequestContainer.Disposer; }
        }

        public IContainer OuterContainer
        {
            get { return containerProvider.RequestContainer.OuterContainer; }
        }

        public IEnumerable<IComponentRegistration> ComponentRegistrations
        {
            get { return containerProvider.RequestContainer.ComponentRegistrations; }
        }

        public event EventHandler<ComponentRegisteredEventArgs> ComponentRegistered
        {
            add { containerProvider.RequestContainer.ComponentRegistered += value; }
            remove { containerProvider.RequestContainer.ComponentRegistered -= value; }
        }

        public bool TryGetDefaultRegistrationFor(Service service, out IComponentRegistration registration)
        {
            return containerProvider.RequestContainer.TryGetDefaultRegistrationFor(service, out registration);
        }

        public void TagWith<T>(T tag)
        {
            containerProvider.RequestContainer.TagWith<T>(tag);
        }

        public TService Resolve<TService>(params Parameter[] parameters)
        {
            return containerProvider.RequestContainer.Resolve<TService>(parameters);
        }

        public TService Resolve<TService>(string serviceName, params Parameter[] parameters)
        {
            return containerProvider.RequestContainer.Resolve<TService>(serviceName, parameters);
        }

        public object Resolve(Type serviceType, params Parameter[] parameters)
        {
            return containerProvider.RequestContainer.Resolve(serviceType, parameters);
        }

        public object Resolve(string serviceName, params Parameter[] parameters)
        {
            return containerProvider.RequestContainer.Resolve(serviceName, parameters);
        }

        public object Resolve(Service service, params Parameter[] parameters)
        {
            return containerProvider.RequestContainer.Resolve(service, parameters);
        }

        public bool TryResolve<TService>(out TService instance, params Parameter[] parameters)
        {
            return containerProvider.RequestContainer.TryResolve<TService>(out instance, parameters);
        }

        public bool TryResolve(Type serviceType, out object instance, params Parameter[] parameters)
        {
            return containerProvider.RequestContainer.TryResolve(serviceType, out instance, parameters);
        }

        public bool TryResolve(string componentName, out object instance, params Parameter[] parameters)
        {
            return containerProvider.RequestContainer.TryResolve(componentName, out instance, parameters);
        }

        public bool TryResolve(Service service, out object instance, params Parameter[] parameters)
        {
            return containerProvider.RequestContainer.TryResolve(service, out instance, parameters);
        }

        public TService ResolveOptional<TService>(params Parameter[] parameters)
        {
            return containerProvider.RequestContainer.ResolveOptional<TService>(parameters);
        }

        public TService Resolve<TService>(IEnumerable<Parameter> parameters)
        {
            return containerProvider.RequestContainer.Resolve<TService>(parameters);
        }

        public TService Resolve<TService>(string serviceName, IEnumerable<Parameter> parameters)
        {
            return containerProvider.RequestContainer.Resolve<TService>(serviceName, parameters);
        }

        public object Resolve(Type serviceType, IEnumerable<Parameter> parameters)
        {
            return containerProvider.RequestContainer.Resolve(serviceType, parameters);
        }

        public object Resolve(string serviceName, IEnumerable<Parameter> parameters)
        {
            return containerProvider.RequestContainer.Resolve(serviceName, parameters);
        }

        public object Resolve(Service service, IEnumerable<Parameter> parameters)
        {
            return containerProvider.RequestContainer.Resolve(service, parameters);
        }

        public bool TryResolve<TService>(out TService instance, IEnumerable<Parameter> parameters)
        {
            return containerProvider.RequestContainer.TryResolve<TService>(out instance, parameters);
        }

        public bool TryResolve(Type serviceType, out object instance, IEnumerable<Parameter> parameters)
        {
            return containerProvider.RequestContainer.TryResolve(serviceType, out instance, parameters);
        }

        public bool TryResolve(string componentName, out object instance, IEnumerable<Parameter> parameters)
        {
            return containerProvider.RequestContainer.TryResolve(componentName, out instance, parameters);
        }

        public bool TryResolve(Service service, out object instance, IEnumerable<Parameter> parameters)
        {
            return containerProvider.RequestContainer.TryResolve(service, out instance, parameters);
        }

        public TService ResolveOptional<TService>(IEnumerable<Parameter> parameters)
        {
            return containerProvider.RequestContainer.ResolveOptional<TService>(parameters);
        }

        public bool IsRegistered(Type serviceType)
        {
            return containerProvider.RequestContainer.IsRegistered(serviceType);
        }

        public bool IsRegistered(string serviceName)
        {
            return containerProvider.RequestContainer.IsRegistered(serviceName);
        }

        public bool IsRegistered(Service service)
        {
            return containerProvider.RequestContainer.IsRegistered(service);
        }

        public bool IsRegistered<TService>()
        {
            return containerProvider.RequestContainer.IsRegistered<TService>();
        }

        public T InjectProperties<T>(T instance)
        {
            return containerProvider.RequestContainer.InjectProperties<T>(instance);
        }

        public T InjectUnsetProperties<T>(T instance)
        {
            return containerProvider.RequestContainer.InjectUnsetProperties<T>(instance);
        }

        public void Dispose()
        {
            containerProvider.RequestContainer.Dispose();
        }

        public TService ResolveOptional<TService>(string serviceName, IEnumerable<Parameter> parameters)
        {
            return containerProvider.RequestContainer.ResolveOptional<TService>(serviceName, parameters);
        }

        public TService ResolveOptional<TService>(string serviceName, params Parameter[] parameters)
        {
            return containerProvider.RequestContainer.ResolveOptional<TService>(serviceName, parameters);
        }

        #endregion
    }
}