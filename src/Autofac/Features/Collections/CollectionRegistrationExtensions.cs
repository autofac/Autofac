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
using Autofac.Builder;
using Autofac.Core;
using Autofac.Core.Activators.Delegate;
using Autofac.Util;

namespace Autofac.Features.Collections
{
    /// <summary>
    /// Internal implementation of the RegisterCollection/MemberOf-style collection feature.
    /// </summary>
    static class CollectionRegistrationExtensions
    {
        const string MemberOfPropertyKey = "Autofac.CollectionRegistrationExtensions.MemberOf";

        public static IRegistrationBuilder<T[], SimpleActivatorData, SingleRegistrationStyle>
            RegisterCollection<T>(ContainerBuilder builder, string collectionName, Type elementType)
        {
            if (builder == null) throw new ArgumentNullException("builder");
            if (elementType == null) throw new ArgumentNullException("elementType");
            Enforce.ArgumentNotNullOrEmpty(collectionName, "collectionName");

            var arrayType = elementType.MakeArrayType();

            var activator = new DelegateActivator(arrayType, (c, p) =>
            {
                var elements = GetElementRegistrations(collectionName, c.ComponentRegistry);
                var items = elements.Select(e => c.ResolveComponent(e, p)).ToArray();

                var result = Array.CreateInstance(elementType, items.Length);
                items.CopyTo(result, 0);
                return result;
            });

            var rb = new RegistrationBuilder<T[], SimpleActivatorData, SingleRegistrationStyle>(
                new TypedService(typeof(T[])),
                new SimpleActivatorData(activator),
                new SingleRegistrationStyle());

            builder.RegisterCallback(cr => RegistrationBuilder.RegisterSingleComponent(cr, rb));

            return rb;
        }

        static IEnumerable<IComponentRegistration> GetElementRegistrations(string collectionName, IComponentRegistry registry)
        {
            return registry.Registrations.Where(cr => IsElementRegistration(collectionName, cr));
        }

        static bool IsElementRegistration(string collectionName, IComponentRegistration cr)
        {
            object crMembership;
            return cr.Metadata.TryGetValue(MemberOfPropertyKey, out crMembership) &&
                ((IEnumerable<string>)crMembership).Contains(collectionName);
        }

        public static IRegistrationBuilder<TLimit, TActivatorData, TSingleRegistrationStyle>
            MemberOf<TLimit, TActivatorData, TSingleRegistrationStyle>(
                IRegistrationBuilder<TLimit, TActivatorData, TSingleRegistrationStyle> registration,
                string collectionName)
            where TSingleRegistrationStyle : SingleRegistrationStyle
        {
            if (registration == null) throw new ArgumentNullException("registration");
            Enforce.ArgumentNotNullOrEmpty(collectionName, "collectionName");

            registration.OnRegistered(e =>
            {
                var ep = e.ComponentRegistration.Metadata;
                if (ep.ContainsKey(MemberOfPropertyKey))
                {
                    ep[MemberOfPropertyKey] =
                        ((IEnumerable<string>)ep[MemberOfPropertyKey])
                        .Union(new[] { collectionName });
                }
                else
                {
                    ep.Add(
                        MemberOfPropertyKey,
                        new[] { collectionName });
                }
            });

            return registration;
        }
    }
}
