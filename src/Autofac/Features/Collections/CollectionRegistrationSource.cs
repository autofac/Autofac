// This software is part of the Autofac IoC container
// Copyright © 2011 Autofac Contributors
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
    /// for something you don't expect to resolve."
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
        public IEnumerable<IComponentRegistration> RegistrationsFor(Service service, Func<Service, IEnumerable<IComponentRegistration>> registrationAccessor)
        {
            if (service == null) throw new ArgumentNullException(nameof(service));
            if (registrationAccessor == null) throw new ArgumentNullException(nameof(registrationAccessor));

            if (!(service is IServiceWithType swt) || service is DecoratorService)
                return Enumerable.Empty<IComponentRegistration>();

            var serviceType = swt.ServiceType;
            Type elementType = null;
            Type limitType = null;
            Func<IEnumerable<object>, object> generator = null;

            if (serviceType.IsGenericTypeDefinedBy(typeof(IEnumerable<>)))
            {
                elementType = serviceType.GetTypeInfo().GenericTypeArguments.First();
                limitType = elementType.MakeArrayType();
                generator = BuildGenerator(elementType, nameof(Enumerable.ToArray));
            }
            else if (serviceType.IsArray)
            {
                elementType = serviceType.GetElementType();
                limitType = serviceType;
                generator = BuildGenerator(elementType, nameof(Enumerable.ToArray));
            }
            else if (serviceType.IsGenericListOrCollectionInterfaceType())
            {
                elementType = serviceType.GetTypeInfo().GenericTypeArguments.First();
                limitType = typeof(List<>).MakeGenericType(elementType);
                generator = BuildGenerator(elementType, nameof(Enumerable.ToList));
            }

            if (elementType == null || limitType == null || generator == null)
                return Enumerable.Empty<IComponentRegistration>();

            var elementTypeService = swt.ChangeType(elementType);

            var activator = new DelegateActivator(
                limitType,
                (c, p) =>
                {
                    var elements = c.ComponentRegistry.RegistrationsFor(elementTypeService).OrderBy(cr => cr.GetRegistrationOrder());
                    var items = elements.Select(cr => c.ResolveComponent(cr, p));

                    return generator(items);
                });

            var registration = new ComponentRegistration(
                Guid.NewGuid(),
                activator,
                new CurrentScopeLifetime(),
                InstanceSharing.None,
                InstanceOwnership.ExternallyOwned,
                new[] { service },
                new Dictionary<string, object>());

            return new IComponentRegistration[] { registration };
        }

        public bool IsAdapterForIndividualComponents => false;

        public override string ToString()
        {
            return CollectionRegistrationSourceResources.CollectionRegistrationSourceDescription;
        }

        private static Func<IEnumerable<object>, object> BuildGenerator(Type elementType, string methodName)
        {
            var ofTypeMethod = typeof(Enumerable)
                .GetTypeInfo()
                .GetDeclaredMethod(nameof(Enumerable.OfType))
                .MakeGenericMethod(elementType);

            var enumerableMethod = typeof(Enumerable)
                .GetTypeInfo()
                .GetDeclaredMethod(methodName)
                .MakeGenericMethod(elementType);

            var items = Expression.Parameter(typeof(IEnumerable<object>), "items");
            var ofType = Expression.Call(ofTypeMethod, items);
            var toEnumerable = Expression.Call(enumerableMethod, ofType);
            var lambda = Expression.Lambda<Func<IEnumerable<object>, object>>(toEnumerable, items).Compile();
            return lambda;
        }
    }
}
