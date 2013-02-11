// This software is part of the Autofac IoC container
// Copyright (c) 2007 - 2013 Autofac Contributors
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
using System.ServiceModel.DomainServices.Server;
using Autofac.Integration.Web;

namespace Autofac.Extras.DomainServices
{
    /// <summary>
    /// Provides an interface for <see cref="DomainService"/> factory implementations.
    /// </summary>
    public class AutofacDomainServiceFactory : IDomainServiceFactory
    {
        private readonly IContainerProvider _containerProvider;

        private ILifetimeScope RequestLifetimeScope
        {
            get
            {
                return _containerProvider.RequestLifetime;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AutofacDomainServiceFactory"/> class.
        /// </summary>
        /// <param name="containerProvider">The container provider.</param>
        /// <exception cref="System.ArgumentNullException">containerProvider</exception>
        public AutofacDomainServiceFactory(IContainerProvider containerProvider)
        {
            if (containerProvider == null)
            {
                throw new ArgumentNullException("containerProvider");
            }
            _containerProvider = containerProvider;
        }

        /// <summary>
        /// Creates a new <see cref="DomainService" /> instance.
        /// </summary>
        /// <param name="domainServiceType">The <see cref="Type" /> of <see cref="DomainService" /> to create.</param>
        /// <param name="context">The current <see cref="DomainServiceContext" />.</param>
        /// <returns>
        /// A <see cref="DomainService" /> instance.
        /// </returns>
        public DomainService CreateDomainService(Type domainServiceType, DomainServiceContext context)
        {
            return (DomainService)RequestLifetimeScope.Resolve(domainServiceType, TypedParameter.From(context));
        }

        /// <summary>
        /// Releases an existing <see cref="DomainService" /> instance.
        /// </summary>
        /// <param name="domainService">The <see cref="DomainService" /> instance to release.</param>
        public void ReleaseDomainService(DomainService domainService)
        {
            RequestLifetimeScope.Disposer.AddInstanceForDisposal(domainService);
        }
    }
}
