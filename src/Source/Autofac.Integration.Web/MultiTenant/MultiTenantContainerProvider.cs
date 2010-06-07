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
using System.Threading;

namespace Autofac.Integration.Web.MultiTenant
{
    /// <summary>
    /// Supports multi-tenancy by allowing tenant-specific container configurations.
    /// </summary>
    public class MultiTenantContainerProvider : ContainerProvider, IDisposable
    {
        readonly object _synchRoot = new object();
        int _isDisposed;

        readonly ITenantIdentificationPolicy _tenantIdentificationStrategy;
        readonly IDictionary<string, ILifetimeScope> _tenantApplicationScopes = new Dictionary<string, ILifetimeScope>();
        ILifetimeScope _defaultTenantApplicationScope;

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
            : base(applicationContainer)
        {
            if (tenantIdentificationStrategy == null) throw new ArgumentNullException("tenantIdentificationStrategy");
            _tenantIdentificationStrategy = tenantIdentificationStrategy;
        }

        /// <summary>
        /// Dispose of the current request's container, if it has been
        /// instantiated.
        /// </summary>
        public override void EndRequestLifetime()
        {
            lock (_synchRoot)
            {
                base.EndRequestLifetime();
            }
        }

        /// <summary>
        /// The container used to manage components for processing the
        /// current request.
        /// </summary>
        /// <value></value>
        public override ILifetimeScope RequestLifetime
        {
            get
            {
                lock (_synchRoot)
                {
                    return base.RequestLifetime;
                }
            }
        }

        protected override ILifetimeScope CreateRequestLifetime()
        {
            ILifetimeScope tenantScope;
            string tenantId;
            if (!(_tenantIdentificationStrategy.TryIdentifyTenant(out tenantId) &&
                    _tenantApplicationScopes.TryGetValue(tenantId, out tenantScope)))
                tenantScope = _defaultTenantApplicationScope;

            AmbientRequestLifetime = this.RequestLifetimeConfiguration == null ?
                tenantScope.BeginLifetimeScope(WebLifetime.Request) :
                tenantScope.BeginLifetimeScope(WebLifetime.Request, this.RequestLifetimeConfiguration);
            return base.CreateRequestLifetime();
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            var isDisposed = _isDisposed;
            Interlocked.CompareExchange(ref _isDisposed, 1, isDisposed);
            if (isDisposed == 0)
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach (var lifetimeScope in _tenantApplicationScopes.Values)
                    lifetimeScope.Dispose();

                this.ApplicationContainer.Dispose();
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
