// This software is part of the Autofac IoC container
// Copyright © 2014 Autofac Contributors
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
using System.Linq;
using Autofac.Builder;
using Autofac.Core;
using Microsoft.Framework.DependencyInjection;

namespace Autofac.Integration.AspNet
{
    internal class ChainedRegistrationSource : IRegistrationSource
    {
        private readonly IServiceProvider _fallbackServiceProvider;

        public ChainedRegistrationSource(IServiceProvider fallbackServiceProvider)
        {
            _fallbackServiceProvider = fallbackServiceProvider;
        }

        public bool IsAdapterForIndividualComponents
        {
            get { return false; }
        }

        public IEnumerable<IComponentRegistration> RegistrationsFor(
                Service service,
                Func<Service, IEnumerable<IComponentRegistration>> registrationAcessor)
        {
            var serviceWithType = service as IServiceWithType;
            if (serviceWithType == null)
            {
                yield break;
            }

            // Only introduce services that are not already registered
            if (registrationAcessor(service).Any())
            {
                yield break;
            }

            var serviceType = serviceWithType.ServiceType;
            if (serviceType == typeof(FallbackScope))
            {
                // This is where we rescope the _fallbackServiceProvider for use in inner scopes
                // When we actually resolve fallback services, we first access the scoped fallback
                // service provider by resolving FallbackScope and using its ServiceProvider property.
                yield return RegistrationBuilder.ForDelegate(serviceType, (context, p) =>
                {
                    var lifetime = context.Resolve<ILifetimeScope>() as ISharingLifetimeScope;

                    if (lifetime != null)
                    {
                        var parentLifetime = lifetime.ParentLifetimeScope;

                        FallbackScope parentFallback;
                        if (parentLifetime != null &&
                            parentLifetime.TryResolve<FallbackScope>(out parentFallback))
                        {
                            var scopeFactory = parentFallback.ServiceProvider.GetService<IServiceScopeFactory>();

                            if (scopeFactory != null)
                            {
                                return new FallbackScope(scopeFactory.CreateScope());
                            }
                        }
                    }

                    return new FallbackScope(_fallbackServiceProvider);
                })
                .InstancePerLifetimeScope()
                .CreateRegistration();
            }
            else if (_fallbackServiceProvider.GetService(serviceType) != null)
            {
                yield return RegistrationBuilder.ForDelegate(serviceType, (context, p) =>
                {
                    var fallbackScope = context.Resolve<FallbackScope>();
                    return fallbackScope.ServiceProvider.GetService(serviceType);
                })
                .PreserveExistingDefaults()
                .CreateRegistration();
            }
        }

        private class FallbackScope : IDisposable
        {
            private readonly IDisposable _scopeDisposer;

            public FallbackScope(IServiceProvider fallbackServiceProvider)
                : this(fallbackServiceProvider, scopeDisposer: null)
            {
            }
            public FallbackScope(IServiceScope fallbackScope)
                : this(fallbackScope.ServiceProvider, fallbackScope)
            {
            }

            private FallbackScope(IServiceProvider fallbackServiceProvider, IDisposable scopeDisposer)
            {
                ServiceProvider = fallbackServiceProvider;
                _scopeDisposer = scopeDisposer;
            }

            public IServiceProvider ServiceProvider { get; private set; }

            public void Dispose()
            {
                if (_scopeDisposer != null)
                {
                    _scopeDisposer.Dispose();
                }
            }
        }
    }
}
