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

        public IComponentRegistry ComponentRegistry
        {
            get
            {
                return containerProvider.RequestLifetime.ComponentRegistry;
            }
        }

        public ILifetimeScope BeginLifetimeScope()
        {
            return containerProvider.RequestLifetime.BeginLifetimeScope();
        }

        public IDisposer Disposer
        {
            get { return containerProvider.RequestLifetime.Disposer; }
        }

        public object Tag
        {
            get
            {
                return containerProvider.RequestLifetime.Tag;
            }
            set
            {
                containerProvider.RequestLifetime.Tag = value;
            }
        }

        public object Resolve(IComponentRegistration registration, IEnumerable<Parameter> parameters)
        {
            return containerProvider.RequestLifetime.Resolve(registration, parameters);
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}