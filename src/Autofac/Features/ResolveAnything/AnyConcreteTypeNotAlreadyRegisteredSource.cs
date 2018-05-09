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
using System.Reflection;
using Autofac.Builder;
using Autofac.Core;

namespace Autofac.Features.ResolveAnything
{
    /// <summary>
    /// Provides registrations on-the-fly for any concrete type not already registered with
    /// the container.
    /// </summary>
    public class AnyConcreteTypeNotAlreadyRegisteredSource : IRegistrationSource
    {
        private readonly Func<Type, bool> _predicate;

        /// <summary>
        /// Initializes a new instance of the <see cref="AnyConcreteTypeNotAlreadyRegisteredSource"/> class.
        /// </summary>
        public AnyConcreteTypeNotAlreadyRegisteredSource()
            : this(t => true)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AnyConcreteTypeNotAlreadyRegisteredSource"/> class.
        /// </summary>
        /// <param name="predicate">A predicate that selects types the source will register.</param>
        public AnyConcreteTypeNotAlreadyRegisteredSource(Func<Type, bool> predicate)
        {
            _predicate = predicate ?? throw new ArgumentNullException(nameof(predicate));
        }

        /// <summary>
        /// Retrieve registrations for an unregistered service, to be used
        /// by the container.
        /// </summary>
        /// <param name="service">The service that was requested.</param>
        /// <param name="registrationAccessor">A function that will return existing registrations for a service.</param>
        /// <returns>Registrations providing the service.</returns>
        public IEnumerable<IComponentRegistration> RegistrationsFor(
            Service service,
            Func<Service, IEnumerable<IComponentRegistration>> registrationAccessor)
        {
            if (registrationAccessor == null)
            {
                throw new ArgumentNullException(nameof(registrationAccessor));
            }

            var ts = service as TypedService;
            if (ts == null || ts.ServiceType == typeof(string))
            {
                return Enumerable.Empty<IComponentRegistration>();
            }

            var typeInfo = ts.ServiceType.GetTypeInfo();
            if (!typeInfo.IsClass ||
                typeInfo.IsSubclassOf(typeof(Delegate)) ||
                typeInfo.IsAbstract ||
                typeInfo.IsGenericTypeDefinition ||

                !_predicate(ts.ServiceType) ||
                registrationAccessor(service).Any())
            {
                return Enumerable.Empty<IComponentRegistration>();
            }

            if (typeInfo.IsGenericType)
            {
                foreach (var typeParameter in typeInfo.GenericTypeArguments)
                {
                    // Issue #855: If the generic type argument doesn't match the filter don't look for a registration.
                    if (_predicate(typeParameter) && !registrationAccessor(new TypedService(typeParameter)).Any())
                    {
                        return Enumerable.Empty<IComponentRegistration>();
                    }
                }
            }

            var builder = RegistrationBuilder.ForType(ts.ServiceType);
            RegistrationConfiguration?.Invoke(builder);
            return new[] { builder.CreateRegistration() };
        }

        /// <summary>
        /// Gets a value indicating whether the registrations provided by this source are 1:1 adapters on top
        /// of other components (e.g., Meta, Func, or Owned).
        /// </summary>
        public bool IsAdapterForIndividualComponents => false;

        /// <summary>
        /// Gets or sets an expression used to configure generated registrations.
        /// </summary>
        /// <value>
        /// A <see cref="System.Action{T}"/> that can be used to modify the behavior
        /// of registrations that are generated by this source.
        /// </value>
        public Action<IRegistrationBuilder<object, ConcreteReflectionActivatorData, SingleRegistrationStyle>> RegistrationConfiguration { get; set; }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents the current <see cref="System.Object"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents the current <see cref="System.Object"/>.
        /// </returns>
        public override string ToString()
        {
            return AnyConcreteTypeNotAlreadyRegisteredSourceResources.AnyConcreteTypeNotAlreadyRegisteredSourceDescription;
        }
    }
}
