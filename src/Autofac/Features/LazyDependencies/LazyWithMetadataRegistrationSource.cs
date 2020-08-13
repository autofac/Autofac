// This software is part of the Autofac IoC container
// Copyright © 2007 - 2008 Autofac Contributors
// https://autofac.org
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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Autofac.Builder;
using Autofac.Core;
using Autofac.Features.Metadata;
using Autofac.Util;

namespace Autofac.Features.LazyDependencies
{
    /// <summary>
    /// Support the <c>System.Lazy&lt;T, TMetadata&gt;</c>
    /// types automatically whenever type T is registered with the container.
    /// Metadata values come from the component registration's metadata.
    /// When a dependency of a lazy type is used, the instantiation of the underlying
    /// component will be delayed until the Value property is first accessed.
    /// </summary>
    internal class LazyWithMetadataRegistrationSource : IRegistrationSource
    {
        private static readonly MethodInfo CreateLazyRegistrationMethod = typeof(LazyWithMetadataRegistrationSource).GetDeclaredMethod(nameof(CreateLazyRegistration));

        private delegate IComponentRegistration RegistrationCreator(Service providedService, Service valueService, ServiceRegistration registrationResolveInfo);

        private readonly ConcurrentDictionary<(Type ValueType, Type MetaType), RegistrationCreator> _methodCache = new ConcurrentDictionary<(Type, Type), RegistrationCreator>();

        /// <inheritdoc/>
        public IEnumerable<IComponentRegistration> RegistrationsFor(Service service, Func<Service, IEnumerable<ServiceRegistration>> registrationAccessor)
        {
            if (registrationAccessor == null)
            {
                throw new ArgumentNullException(nameof(registrationAccessor));
            }

            var lazyType = typeof(Lazy<,>);
            if (!(service is IServiceWithType swt) || !swt.ServiceType.IsGenericTypeDefinedBy(lazyType))
            {
                return Enumerable.Empty<IComponentRegistration>();
            }

            var genericTypeArguments = swt.ServiceType.GenericTypeArguments;
            var valueType = genericTypeArguments[0];
            var metaType = genericTypeArguments[1];

            if (!metaType.IsClass)
            {
                return Enumerable.Empty<IComponentRegistration>();
            }

            var valueService = swt.ChangeType(valueType);

            var registrationCreator = _methodCache.GetOrAdd((valueType, metaType), types =>
            {
                return CreateLazyRegistrationMethod.MakeGenericMethod(types.ValueType, types.MetaType).CreateDelegate<RegistrationCreator>(null);
            });

            return registrationAccessor(valueService)
                .Select(v => registrationCreator(service, valueService, v));
        }

        /// <inheritdoc/>
        public bool IsAdapterForIndividualComponents => true;

        /// <inheritdoc/>
        public override string ToString()
        {
            return LazyWithMetadataRegistrationSourceResources.LazyWithMetadataRegistrationSourceDescription;
        }

        private static IComponentRegistration CreateLazyRegistration<T, TMeta>(Service providedService, Service valueService, ServiceRegistration registrationResolveInfo)
        {
            var metadataProvider = MetadataViewProvider.GetMetadataViewProvider<TMeta>();

            var rb = RegistrationBuilder.ForDelegate(
                (c, p) =>
                {
                    var context = c.Resolve<IComponentContext>();
                    var lazyType = ((IServiceWithType)providedService).ServiceType;
                    var request = new ResolveRequest(valueService, registrationResolveInfo, p);
                    var valueFactory = new Func<T>(() => (T)context.ResolveComponent(request));
                    var metadata = metadataProvider(registrationResolveInfo.Registration.Target.Metadata);
                    return Activator.CreateInstance(lazyType, valueFactory, metadata);
                })
                .As(providedService)
                .Targeting(registrationResolveInfo.Registration);

            return rb.CreateRegistration();
        }
    }
}
