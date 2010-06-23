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
using System.Web;
using Autofac.Util;

namespace Autofac.Integration.Web.MultiTenant
{
    /// <summary>
    /// Supports multi-tenancy by allowing tenant-specific container configurations.
    /// </summary>
    public class MultiTenantContainerProvider : Disposable, IContainerProvider
    {
        readonly ITenantIdentificationPolicy _tenantIdentificationStrategy;
        readonly TenancyRegistry _tenancyRegistry;
        readonly Action<ContainerBuilder> _requestLifetimeConfiguration;

        /// <summary>
        /// Construct a <see cref="MultiTenantContainerProvider"/> for the given base container
        /// and tenant identification strategy.
        /// </summary>
        /// <param name="tenantIdentificationStrategy">The strategy that determines how the
        /// current tenant is identified.</param>
        /// <param name="tenancyRegistry">The tenancy registry.</param>
        public MultiTenantContainerProvider(
            ITenantIdentificationPolicy tenantIdentificationStrategy,
            TenancyRegistry tenancyRegistry)
        {
            if (tenantIdentificationStrategy == null) throw new ArgumentNullException("tenantIdentificationStrategy");
            if (tenancyRegistry == null) throw new ArgumentNullException("tenancyRegistry");
            _tenantIdentificationStrategy = tenantIdentificationStrategy;
            _tenancyRegistry = tenancyRegistry;
        }

        /// <summary>
        /// Construct a <see cref="MultiTenantContainerProvider"/> for the given base container
        /// and tenant identification strategy.
        /// </summary>
        /// <param name="tenantIdentificationStrategy">The strategy that determines how the
        /// current tenant is identified.</param>
        /// <param name="tenancyRegistry">The tenancy registry.</param>
        /// <param name="requestLifetimeConfiguration">Configuration that acts upon a request-specific
        /// <see cref="ContainerBuilder"/>.</param>
        public MultiTenantContainerProvider(
            ITenantIdentificationPolicy tenantIdentificationStrategy,
            TenancyRegistry tenancyRegistry,
            Action<ContainerBuilder> requestLifetimeConfiguration)
            : this(tenantIdentificationStrategy, tenancyRegistry)
        {
            if (requestLifetimeConfiguration == null) throw new ArgumentNullException("requestLifetimeConfiguration");
            _requestLifetimeConfiguration = requestLifetimeConfiguration;
        }

        /// <summary>
        /// Dispose of the current request's container, if it has been
        /// instantiated.
        /// </summary>
        public void EndRequestLifetime()
        {
            var rc = AmbientRequestLifetime;
            if (rc != null)
                rc.Dispose();
        }

        /// <summary>
        /// The global, application-wide container.
        /// </summary>
        public IContainer ApplicationContainer
        {
            get
            {
                return _tenancyRegistry.Container;
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
                if (AmbientRequestLifetime != null)
                    return AmbientRequestLifetime;

                var tenantScope = _tenancyRegistry.GetTenantApplicationScope(_tenantIdentificationStrategy);

                AmbientRequestLifetime = _requestLifetimeConfiguration == null ?
                    tenantScope.BeginLifetimeScope(WebLifetime.Request) :
                    tenantScope.BeginLifetimeScope(WebLifetime.Request, _requestLifetimeConfiguration);

                return AmbientRequestLifetime;
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
            if (disposing)
                _tenancyRegistry.Dispose();
            base.Dispose(disposing);
        }
    }
}
