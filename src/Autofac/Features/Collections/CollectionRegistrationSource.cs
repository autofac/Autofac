// This software is part of the Autofac IoC container
// Copyright © 2011 Autofac Contributors
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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Autofac.Builder;
using Autofac.Core;
using Autofac.Core.Activators.Delegate;
using Autofac.Core.Lifetime;
using Autofac.Core.Registration;
using Autofac.Features.Decorators;
using Autofac.Util;

namespace Autofac.Features.Collections
{
    /// <summary>
    /// Registration source providing implicit collection/list/enumerable support.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This registration source provides enumerable support to allow resolving
    /// the set of all registered services of a given type.
    /// </para>
    /// <para>
    /// What may not be immediately apparent is that it also means any time there
    /// are no items of a particular type registered, it will always return an
    /// empty set rather than <see langword="null" /> or throwing an exception.
    /// This is by design.
    /// </para>
    /// <para>
    /// Consider the [possibly majority] use case where you're resolving a set
    /// of message handlers or event handlers from the container. If there aren't
    /// any handlers, you want an empty set - not <see langword="null" /> or
    /// an exception. It's valid to have no handlers registered.
    /// </para>
    /// <para>
    /// This implicit support means other areas (like MVC support or manual
    /// property injection) must take care to only request enumerable values they
    /// expect to get something back for. In other words, "Don't ask the container
    /// for something you don't expect to resolve".
    /// </para>
    /// </remarks>
    internal class CollectionRegistrationSource : IRegistrationSource
    {
        /// <summary>
        /// Retrieve registrations for an unregistered service, to be used
        /// by the container.
        /// </summary>
        /// <param name="service">The service that was requested.</param>
        /// <param name="registrationAccessor">A function that will return existing registrations for a service.</param>
        /// <returns>Registrations providing the service.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Reliability",
            "CA2000:Dispose objects before losing scope",
            Justification = "Activator lifetime controlled by registry.")]
        public IEnumerable<IComponentRegistration> RegistrationsFor(Service service, Func<Service, IEnumerable<ServiceRegistration>> registrationAccessor)
        {
            if (service == null)
            {
                throw new ArgumentNullException(nameof(service));
            }

            if (registrationAccessor == null)
            {
                throw new ArgumentNullException(nameof(registrationAccessor));
            }

            if (!(service is IServiceWithType swt) || service is DecoratorService)
            {
                return Enumerable.Empty<IComponentRegistration>();
            }

            var serviceType = swt.ServiceType;
            Type? elementType = null;
            Type? limitType = null;
            Func<int, IList>? factory = null;

            if (serviceType.IsGenericTypeDefinedBy(typeof(IEnumerable<>)))
            {
                elementType = serviceType.GenericTypeArguments[0];
                limitType = elementType.MakeArrayType();
                factory = GenerateArrayFactory(elementType);
            }
            else if (serviceType.IsArray)
            {
                elementType = serviceType.GetElementType();
                limitType = serviceType;
                factory = GenerateArrayFactory(elementType);
            }
            else if (serviceType.IsGenericListOrCollectionInterfaceType())
            {
                elementType = serviceType.GenericTypeArguments[0];
                limitType = typeof(List<>).MakeGenericType(elementType);
                factory = GenerateListFactory(elementType);
            }

            if (elementType == null || factory == null || limitType == null)
            {
                return Enumerable.Empty<IComponentRegistration>();
            }

            var elementTypeService = swt.ChangeType(elementType);

            var activator = new DelegateActivator(
                limitType,
                (c, p) =>
                {
                    var itemRegistrations = c.ComponentRegistry
                        .ServiceRegistrationsFor(elementTypeService)
                        .Where(cr => !cr.Registration.Options.HasOption(RegistrationOptions.ExcludeFromCollections))
                        .OrderBy(cr => cr.Registration.GetRegistrationOrder())
                        .ToList();

                    var output = factory(itemRegistrations.Count);
                    var isFixedSize = output.IsFixedSize;

                    for (var i = 0; i < itemRegistrations.Count; i++)
                    {
                        var itemRegistration = itemRegistrations[i];
                        var resolveRequest = new ResolveRequest(elementTypeService, itemRegistration, p);
                        var component = c.ResolveComponent(resolveRequest);
                        if (isFixedSize)
                        {
                            output[i] = component;
                        }
                        else
                        {
                            output.Add(component);
                        }
                    }

                    return output;
                });

            var registration = new ComponentRegistration(
                Guid.NewGuid(),
                activator,
                CurrentScopeLifetime.Instance,
                InstanceSharing.None,
                InstanceOwnership.ExternallyOwned,
                new[] { service },
                new Dictionary<string, object?>());

            return new IComponentRegistration[] { registration };
        }

        /// <inheritdoc/>
        public bool IsAdapterForIndividualComponents => false;

        /// <inheritdoc/>
        public override string ToString()
            => CollectionRegistrationSourceResources.CollectionRegistrationSourceDescription;

        private static Func<int, IList> GenerateListFactory(Type elementType)
        {
            var parameter = Expression.Parameter(typeof(int));
            var genericType = typeof(List<>).MakeGenericType(elementType);
            var constructor = genericType.GetMatchingConstructor(new[] { typeof(int) });
            var newList = Expression.New(constructor, parameter);
            return Expression.Lambda<Func<int, IList>>(newList, parameter).Compile();
        }

        private static Func<int, IList> GenerateArrayFactory(Type elementType)
        {
            var parameter = Expression.Parameter(typeof(int));
            var newArray = Expression.NewArrayBounds(elementType, parameter);
            return Expression.Lambda<Func<int, IList>>(newArray, parameter).Compile();
        }
    }
}
