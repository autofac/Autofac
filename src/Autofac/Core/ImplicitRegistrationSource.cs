// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Autofac.Builder;
using Autofac.Util;

namespace Autofac.Core
{
    /// <summary>
    /// Support simple definition of implicit relationships such as <see cref="Lazy{T}"/>.
    /// </summary>
    public abstract class ImplicitRegistrationSource : IRegistrationSource
    {
        private delegate IComponentRegistration RegistrationCreator(Service providedService, Service valueService, ServiceRegistration valueRegistration);

        private static readonly MethodInfo CreateRegistrationMethod = typeof(ImplicitRegistrationSource).GetDeclaredMethod(nameof(CreateRegistration));

        private readonly Type _type;
        private readonly ConcurrentDictionary<Type, RegistrationCreator> _methodCache;

        /// <summary>
        /// Initializes a new instance of the <see cref="ImplicitRegistrationSource"/> class.
        /// </summary>
        /// <param name="type">The implicit type. Must be generic with only one type parameter.</param>
        protected ImplicitRegistrationSource(Type type)
        {
            _type = type ?? throw new ArgumentNullException(nameof(type));

            if (!type.IsGenericType)
            {
                throw new InvalidOperationException(ImplicitRegistrationSourceResources.TypeMustBeGeneric);
            }

            if (type.GetGenericArguments().Length != 1)
            {
                throw new InvalidOperationException(ImplicitRegistrationSourceResources.GenericTypeMustBeUnary);
            }

            _methodCache = new ConcurrentDictionary<Type, RegistrationCreator>();
        }

        /// <inheritdoc />
        public IEnumerable<IComponentRegistration> RegistrationsFor(Service service, Func<Service, IEnumerable<ServiceRegistration>> registrationAccessor)
        {
            if (registrationAccessor == null)
            {
                throw new ArgumentNullException(nameof(registrationAccessor));
            }

            if (!(service is IServiceWithType swt) || !swt.ServiceType.IsGenericTypeDefinedBy(_type))
            {
                return Enumerable.Empty<IComponentRegistration>();
            }

            var valueType = swt.ServiceType.GenericTypeArguments[0];
            var valueService = swt.ChangeType(valueType);
            var registrationCreator = _methodCache.GetOrAdd(valueType, t =>
            {
                return CreateRegistrationMethod.MakeGenericMethod(t).CreateDelegate<RegistrationCreator>(this);
            });

            return registrationAccessor(valueService)
                .Select(v => registrationCreator(service, valueService, v));
        }

        /// <inheritdoc />
        public virtual bool IsAdapterForIndividualComponents => true;

        /// <summary>
        /// Gets the description of the registration source.
        /// </summary>
        public virtual string Description => GetType().Name;

        /// <inheritdoc/>
        public override string ToString() => Description;

        /// <summary>
        /// Resolves an instance of the implicit type.
        /// </summary>
        /// <typeparam name="T">The child type used in the implicit type.</typeparam>
        /// <param name="ctx">A component context to resolve services.</param>
        /// <param name="request">A resolve request.</param>
        /// <returns>An implicit type instance.</returns>
        protected abstract object ResolveInstance<T>(IComponentContext ctx, ResolveRequest request)
            where T : notnull;

        /// <summary>
        /// Allows hooking into the registration pipeline of the registration source, useful for such things as marking a registration as externally owned.
        /// </summary>
        /// <param name="registration">The registration builder.</param>
        /// <returns>The updated registration builder.</returns>
        protected virtual IRegistrationBuilder<object, SimpleActivatorData, SingleRegistrationStyle> BuildRegistration(IRegistrationBuilder<object, SimpleActivatorData, SingleRegistrationStyle> registration)
            => registration;

        private IComponentRegistration CreateRegistration<T>(Service providedService, Service valueService, ServiceRegistration serviceRegistration)
            where T : notnull
        {
            var registrationDelegate = RegistrationBuilder.ForDelegate(
                (c, p) =>
                {
                    var request = new ResolveRequest(valueService, serviceRegistration, p);

                    return ResolveInstance<T>(c, request);
                });

            var rb = BuildRegistration(registrationDelegate)
                .As(providedService)
                .Targeting(serviceRegistration.Registration)
                .InheritRegistrationOrderFrom(serviceRegistration.Registration);

            return rb.CreateRegistration();
        }
    }
}
