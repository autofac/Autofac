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
using System.Web;
using Autofac.Util;

namespace Autofac.Integration.Web.MultiTenant
{
    /// <summary>
    /// Supports multi-tenancy by allowing tenant-specific container configurations.
    /// </summary>
    public class MultiTenantContainerProvider : Disposable, IContainerProvider
    {
        readonly object _synchRoot = new object();

        readonly ITenantIdentificationPolicy _tenantIdentificationStrategy;
        readonly IContainer _applicationContainer;
        readonly IDictionary<string, ILifetimeScope> _tenantApplicationScopes = new Dictionary<string, ILifetimeScope>();
        ILifetimeScope _defaultTenantApplicationScope;
        Action<ContainerBuilder> _requestLifetimeConfiguration;

        /// <summary>
        /// Construct a <see cref="MultiTenantContainerProvider"/> for the given base container
        /// and tenant identification strategy.
        /// </summary>
        /// <param name="tenantIdentificationStrategy">The strategy that determines how the
        /// current tenant is identified.</param>
        /// <param name="applicationContainer">The base container configuration, shared by all
        /// tenants.</param>
        public MultiTenantContainerProvider(
            ITenantIdentificationPolicy tenantIdentificationStrategy,
            IContainer applicationContainer)
        {
            if (tenantIdentificationStrategy == null) throw new ArgumentNullException("tenantIdentificationStrategy");
            if (applicationContainer == null) throw new ArgumentNullException("applicationContainer");
            _tenantIdentificationStrategy = tenantIdentificationStrategy;
            _applicationContainer = applicationContainer;
        }

        /// <summary>
        /// Dispose of the current request's container, if it has been
        /// instantiated.
        /// </summary>
        public void EndRequestLifetime()
        {
            lock (_synchRoot)
            {
                var rc = AmbientRequestLifetime;
                if (rc != null)
                    rc.Dispose();
            }
        }

        /// <summary>
        /// The global, application-wide container.
        /// </summary>
        /// <value></value>
        public IContainer ApplicationContainer
        {
            get
            {
                return _applicationContainer;
            }
        }

        /// <summary>
        /// The container used to manage components for processing the
        /// current request.
        /// </summary>
        /// <value></value>
        public ILifetimeScope RequestLifetime
        {
            get
            {
                lock (_synchRoot)
                {
                    if (AmbientRequestLifetime != null)
                        return AmbientRequestLifetime;

                    ILifetimeScope tenantScope;
                    string tenantId;
                    if (!(_tenantIdentificationStrategy.TryIdentifyTenant(out tenantId) &&
                            _tenantApplicationScopes.TryGetValue(tenantId, out tenantScope)))
                        tenantScope = _defaultTenantApplicationScope;

                    AmbientRequestLifetime = _requestLifetimeConfiguration == null ?
                        tenantScope.BeginLifetimeScope(WebLifetime.Request) :
                        tenantScope.BeginLifetimeScope(WebLifetime.Request, _requestLifetimeConfiguration);

                    return AmbientRequestLifetime;
                }
            }
        }

        static ILifetimeScope AmbientRequestLifetime
        {
            get
            {
                return (ILifetimeScope)HttpContext.Current.Items[typeof(ILifetimeScope)];
            }
            set
            {
                HttpContext.Current.Items[typeof(ILifetimeScope)] = value;
            }
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                foreach (var lifetimeScope in _tenantApplicationScopes.Values)
                    lifetimeScope.Dispose();

                _applicationContainer.Dispose();
            }
        }

        /// <summary>
        /// Configure the callback that executes immediately before any components are
        /// resolved from a new request lifetime.
        /// </summary>
        /// <param name="configuration">Configuration that acts upon a request-specific
        /// <see cref="ContainerBuilder"/>.</param>
        public void ConfigureRequestLifetime(Action<ContainerBuilder> configuration)
        {
            if (configuration == null) throw new ArgumentNullException("configuration");
            _requestLifetimeConfiguration = configuration;
        }

        /// <summary>
        /// Configure components that will be visible when no tenant can be identified.
        /// </summary>
        /// <param name="configuration">Configuration that acts upon a
        /// <see cref="ContainerBuilder"/> shared by all unidentified tenants.</param>
        public void ConfigureDefaultTenant(Action<ContainerBuilder> configuration)
        {
            if (configuration == null) throw new ArgumentNullException("configuration");
            _defaultTenantApplicationScope = ApplicationContainer.BeginLifetimeScope(configuration);
        }

        /// <summary>
        /// Configure components specific to the tenant identified by <paramref name="tenantId"/>.
        /// </summary>
        /// <param name="tenantId">The identifier for the tenant to be configured.</param>
        /// <param name="configuration">Configuration that acts upon a
        /// <see cref="ContainerBuilder"/> specific to the tenant identified by
        /// <paramref name="tenantId"/>.</param>
        public void ConfigureTenant(string tenantId, Action<ContainerBuilder> configuration)
        {
            if (tenantId == null) throw new ArgumentNullException("tenantId");
            if (configuration == null) throw new ArgumentNullException("configuration");
            _tenantApplicationScopes.Add(tenantId, ApplicationContainer.BeginLifetimeScope(tenantId, configuration));
        }
    }
}
