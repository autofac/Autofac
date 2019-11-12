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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Autofac.Builder;
using Autofac.Core;
using Autofac.Features.Decorators;
using Autofac.Util;

namespace Autofac.Features.Variance
{
    /// <summary>
    /// Enables contravariant <c>Resolve()</c> for interfaces that have a single contravariant ('in') parameter.
    /// </summary>
    /// <example>
    /// <code>
    /// interface IHandler&lt;in TCommand&gt;
    /// {
    ///     void Handle(TCommand command);
    /// }
    ///
    /// class Command { }
    ///
    /// class DerivedCommand : Command { }
    ///
    /// class CommandHandler : IHandler&lt;Command&gt; { ... }
    ///
    /// var builder = new ContainerBuilder();
    /// builder.RegisterSource(new ContravariantRegistrationSource());
    /// builder.RegisterType&lt;CommandHandler&gt;();
    /// var container = builder.Build();
    /// // Source enables this line, even though IHandler&lt;Command&gt; is the
    /// // actual registered type.
    /// var handler = container.Resolve&lt;IHandler&lt;DerivedCommand&gt;&gt;();
    /// handler.Handle(new DerivedCommand());
    /// </code>
    /// </example>
    public class ContravariantRegistrationSource : IRegistrationSource
    {
        private const string IsContravariantAdapter = "IsContravariantAdapter";

        /// <summary>
        /// Retrieve registrations for an unregistered service, to be used
        /// by the container.
        /// </summary>
        /// <param name="service">The service that was requested.</param>
        /// <param name="registrationAccessor">A function that will return existing registrations for a service.</param>
        /// <returns>Registrations providing the service.</returns>
        /// <remarks>
        /// If the source is queried for service s, and it returns a component that implements both s and s', then it
        /// will not be queried again for either s or s'. This means that if the source can return other implementations
        /// of s', it should return these, plus the transitive closure of other components implementing their
        /// additional services, along with the implementation of s. It is not an error to return components
        /// that do not implement <paramref name="service"/>.
        /// </remarks>
        public IEnumerable<IComponentRegistration> RegistrationsFor(
            Service service,
            Func<Service, IEnumerable<IComponentRegistration>> registrationAccessor)
        {
            if (service == null) throw new ArgumentNullException(nameof(service));
            if (registrationAccessor == null) throw new ArgumentNullException(nameof(registrationAccessor));

            if (!(service is IServiceWithType swt)
                || service is DecoratorService
                || !IsCompatibleInterfaceType(swt.ServiceType, out var contravariantParameterIndex))
                return Enumerable.Empty<IComponentRegistration>();

            var args = swt.ServiceType.GetTypeInfo().GenericTypeArguments;
            var definition = swt.ServiceType.GetGenericTypeDefinition();
            var contravariantParameter = args[contravariantParameterIndex];
            if (contravariantParameter.GetTypeInfo().IsValueType)
                return Enumerable.Empty<IComponentRegistration>();

            var possibleSubstitutions = GetTypesAssignableFrom(contravariantParameter);
            var variations = possibleSubstitutions
                .Select(s => SubstituteArrayElementAt(args, s, contravariantParameterIndex))
                .Where(a => definition.IsCompatibleWithGenericParameterConstraints(a))
                .Select(a => definition.MakeGenericType(a));
            var variantRegistrations = variations
                .SelectMany(v => registrationAccessor(swt.ChangeType(v)))
                .Where(r => !r.Metadata.ContainsKey(IsContravariantAdapter));

            return variantRegistrations
                .Select(vr => RegistrationBuilder
                    .ForDelegate((c, p) => c.ResolveComponent(new ResolveRequest(service, vr, p)))
                    .Targeting(vr, IsAdapterForIndividualComponents)
                    .As(service)
                    .WithMetadata(IsContravariantAdapter, true)
                    .CreateRegistration());
        }

        private static Type[] SubstituteArrayElementAt(Type[] array, Type newElement, int index)
        {
            var copy = array.ToArray();
            copy[index] = newElement;
            return copy;
        }

        private static IEnumerable<Type> GetTypesAssignableFrom(Type type)
        {
            return GetBagOfTypesAssignableFrom(type)
                .Distinct();
        }

        private static IEnumerable<Type> GetBagOfTypesAssignableFrom(Type type)
        {
            if (type.GetTypeInfo().BaseType != null)
            {
                yield return type.GetTypeInfo().BaseType;
                foreach (var fromBase in GetBagOfTypesAssignableFrom(type.GetTypeInfo().BaseType))
                    yield return fromBase;
            }
            else
            {
                if (type != typeof(object))
                    yield return typeof(object);
            }

            foreach (var ifce in type.GetTypeInfo().ImplementedInterfaces)
            {
                if (ifce != type)
                {
                    yield return ifce;
                    foreach (var fromIfce in GetBagOfTypesAssignableFrom(ifce))
                        yield return fromIfce;
                }
            }
        }

        private static bool IsCompatibleInterfaceType(Type type, out int contravariantParameterIndex)
        {
            if (type.GetTypeInfo().IsGenericType && type.GetTypeInfo().IsInterface)
            {
                var contravariantWithIndex = type
                    .GetGenericTypeDefinition()
                    .GetTypeInfo().GenericTypeParameters
                    .Select((c, i) => new
                    {
                        IsContravariant = (c.GetTypeInfo().GenericParameterAttributes & GenericParameterAttributes.Contravariant) !=
                        GenericParameterAttributes.None,
                        Index = i,
                    })
                    .Where(cwi => cwi.IsContravariant)
                    .ToArray();

                if (contravariantWithIndex.Length == 1)
                {
                    contravariantParameterIndex = contravariantWithIndex[0].Index;
                    return true;
                }
            }

            contravariantParameterIndex = default(int);
            return false;
        }

        /// <summary>
        /// Gets a value indicating whether the registrations provided by this source are 1:1 adapters on top
        /// of other components (e.g., Meta, Func, or Owned).
        /// </summary>
        public bool IsAdapterForIndividualComponents => true;
    }
}
