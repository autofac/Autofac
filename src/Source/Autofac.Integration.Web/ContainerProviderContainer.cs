// Contributed by Chad Lee 2009-06-15
// Copyright (c) 2010 Autofac Contributors
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
using Autofac.Core;

namespace Autofac.Integration.Web
{
    /// <summary>
    /// Provides an implementation of <see cref="Autofac.IContainer"/> which uses the configured
    /// <see cref="IContainerProvider"/> to route calls to the current request container.
    /// </summary>
    public class ContainerProviderContainer : IContainer
    {
        private readonly IContainerProvider _containerProvider;

        /// <summary>
        /// Initializes a new instance of <see cref="Autofac.Integration.Web.ContainerProviderContainer"/>.
        /// </summary>
        /// <param name="containerProvider">The <see cref="IContainerProvider"/> to use to retrieve the current request container.</param>
        public ContainerProviderContainer(IContainerProvider containerProvider)
        {
            if (containerProvider == null)
                throw new ArgumentNullException("containerProvider");

            _containerProvider = containerProvider;
        }

        public IComponentRegistry ComponentRegistry
        {
            get
            {
                return _containerProvider.RequestLifetime.ComponentRegistry;
            }
        }

        public ILifetimeScope BeginLifetimeScope()
        {
            return _containerProvider.RequestLifetime.BeginLifetimeScope();
        }

        public ILifetimeScope BeginLifetimeScope(object tag)
        {
            return _containerProvider.RequestLifetime.BeginLifetimeScope(tag);
        }

        public ILifetimeScope BeginLifetimeScope(Action<ContainerBuilder> configurationAction)
        {
            return _containerProvider.RequestLifetime.BeginLifetimeScope(configurationAction);
        }

        public ILifetimeScope BeginLifetimeScope(object tag, Action<ContainerBuilder> configurationAction)
        {
            return _containerProvider.RequestLifetime.BeginLifetimeScope(tag, configurationAction);
        }

        public IDisposer Disposer
        {
            get { return _containerProvider.RequestLifetime.Disposer; }
        }

        public object Tag
        {
            get
            {
                return _containerProvider.RequestLifetime.Tag;
            }
        }

        public object Resolve(IComponentRegistration registration, IEnumerable<Parameter> parameters)
        {
            return _containerProvider.RequestLifetime.Resolve(registration, parameters);
        }

        public void Dispose()
        {
        }
    }
}