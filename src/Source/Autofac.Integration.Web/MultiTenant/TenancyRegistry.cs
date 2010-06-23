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
using Autofac.Util;

namespace Autofac.Integration.Web.MultiTenant
{
    /// <summary>
    /// Tracks the application-level scope for multiple configurations.
    /// </summary>
    public class TenancyRegistry : Disposable
    {
        readonly object _synchRoot = new object();
        readonly IContainer _container;
        readonly IDictionary<object, ILifetimeScope> _tenantApplicationScopes = new Dictionary<object, ILifetimeScope>();
        ILifetimeScope _defaultTenantApplicationScope;

        /// <summary>
        /// Construct a <see cref="TenancyRegistry"/> for the given application container.
        /// </summary>
        /// <param name="container">The base container configuration, shared by all
        /// tenants.</param>
        public TenancyRegistry(IContainer container)
        {
            if (container == null) throw new ArgumentNullException("container");
            _container = container;
        }

        /// <summary>
        /// The global, application-wide container.
        /// </summary>
        public IContainer Container
        {
            get { return _container; }
        }

        /// <summary>
        /// Determine the current application-level lifetime scope given a
        /// strategy for identifying the current tenant.
        /// </summary>
        /// <param name="tenantIdentificationStrategy">The tenant identification strategy.</param>
        /// <returns>The application-level lifetime scope.</returns>
        public ILifetimeScope GetTenantApplicationScope(ITenantIdentificationPolicy tenantIdentificationStrategy)
        {
            lock (_synchRoot)
            {
                ILifetimeScope tenantScope;
                object tenantId;
                if (!(tenantIdentificationStrategy.TryIdentifyTenant(out tenantId) &&
                      _tenantApplicationScopes.TryGetValue(tenantId, out tenantScope)))
                    tenantScope = _defaultTenantApplicationScope ?? Container;

                return tenantScope;
            }
        }


        /// <summary>
        /// Configure components that will be visible when no tenant can be identified.
        /// </summary>
        /// <param name="configuration">Configuration that acts upon a
        /// <see cref="ContainerBuilder"/> shared by all unidentified tenants.</param>
        public void ConfigureDefaultTenant(Action<ContainerBuilder> configuration)
        {
            if (configuration == null) throw new ArgumentNullException("configuration");
            _defaultTenantApplicationScope = Container.BeginLifetimeScope(configuration);
        }

        /// <summary>
        /// Configure components specific to the tenant identified by <paramref name="tenantId"/>.
        /// </summary>
        /// <param name="tenantId">The identifier for the tenant to be configured.</param>
        /// <param name="configuration">Configuration that acts upon a
        /// <see cref="ContainerBuilder"/> specific to the tenant identified by
        /// <paramref name="tenantId"/>.</param>
        public void ConfigureTenant(object tenantId, Action<ContainerBuilder> configuration)
        {
            if (tenantId == null) throw new ArgumentNullException("tenantId");
            if (configuration == null) throw new ArgumentNullException("configuration");
            _tenantApplicationScopes.Add(tenantId, Container.BeginLifetimeScope(tenantId, configuration));
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach (var lifetimeScope in _tenantApplicationScopes.Values)
                    lifetimeScope.Dispose();

                _defaultTenantApplicationScope.Dispose();
                _container.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}