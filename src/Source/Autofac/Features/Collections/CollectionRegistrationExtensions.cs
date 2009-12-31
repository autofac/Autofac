// This software is part of the Autofac IoC container
// Copyright (c) 2007 - 2009 Autofac Contributors
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

        public static RegistrationBuilder<T[], SimpleActivatorData, SingleRegistrationStyle>
            RegisterCollection<T>(this ContainerBuilder builder, string collectionName, Type elementType)
        {
            Enforce.ArgumentNotNull(builder, "builder");
            Enforce.ArgumentNotNull(elementType, "elementType");
            Enforce.ArgumentNotNullOrEmpty(collectionName, "collectionName");

            var arrayType = elementType.MakeArrayType();
            IEnumerable<IComponentRegistration> elements = null;

            var activator = new DelegateActivator(arrayType, (c, p) =>
            {
                if (elements == null)
                {
                    c.ComponentRegistry.Registered += (sender, e) =>
                    {
                        if (IsElementRegistration(collectionName, e.ComponentRegistration))
                            elements = GetElementRegistrations(collectionName, c.ComponentRegistry);
                    };
                    elements = GetElementRegistrations(collectionName, c.ComponentRegistry);
                }

                var items = elements.Select(e => c.Resolve(e, p)).ToArray();

                Array result = Array.CreateInstance(elementType, items.Length);
                items.CopyTo(result, 0);
                return result;
            });

            var rb = new RegistrationBuilder<T[], SimpleActivatorData, SingleRegistrationStyle>(
                new SimpleActivatorData(activator),
                new SingleRegistrationStyle(typeof(T[])));

            builder.RegisterCallback(cr => {
                RegistrationBuilder.RegisterSingleComponent(cr, rb);
            });

            return rb;
        }

        static IEnumerable<IComponentRegistration> GetElementRegistrations(string collectionName, IComponentRegistry registry)
        {
            return registry.Registrations.Where(cr => IsElementRegistration(collectionName, cr));
        }

        static bool IsElementRegistration(string collectionName, IComponentRegistration cr)
        {
            object crMembership;
            return cr.ExtendedProperties.TryGetValue(MemberOfPropertyKey, out crMembership) &&
                ((IEnumerable<string>)crMembership).Contains(collectionName);
        }

        public static RegistrationBuilder<TLimit, TActivatorData, TSingleRegistrationStyle>
            MemberOf<TLimit, TActivatorData, TSingleRegistrationStyle>(
                this RegistrationBuilder<TLimit, TActivatorData, TSingleRegistrationStyle> registration,
                string collectionName)
            where TSingleRegistrationStyle : SingleRegistrationStyle
        {
            Enforce.ArgumentNotNull(registration, "registration");
            Enforce.ArgumentNotNullOrEmpty(collectionName, "collectionName");

            registration.OnRegistered(e =>
            {
                var ep = e.ComponentRegistration.ExtendedProperties;
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
