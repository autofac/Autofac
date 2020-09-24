// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

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
