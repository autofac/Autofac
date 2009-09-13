// This software is part of the Autofac IoC container
// Copyright (c) 2007 - 2008 Autofac Contributors
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
using System.Text;
using Autofac.Activators;

namespace Autofac.Builder
{
    /// <summary>
    /// Support for MemberOf() style collection registrations.
    /// </summary>
    public static class CollectionRegistrationExtensions
    {
        // This whole approach is broken because
        // using a service to mark items causes them to be registed under that
        // service only (their own type is ignored as a default.)
        // Including this just to get core solution building
        class MemberService : Service
        {
            Service _collectionService;

            public MemberService(Service collectionService)
            {
                _collectionService = Enforce.ArgumentNotNull(collectionService, "collectionService");
            }

            public override string Description
            {
                get { return "Member of " + _collectionService.Description; }
            }

            public override bool Equals(object obj)
            {
                var ms = obj as MemberService;
                return ms != null && ms._collectionService == _collectionService;
            }

            public override int GetHashCode()
            {
                return 7 ^ _collectionService.GetHashCode();
            }
        }

        /// <summary>
        /// Registers the type as a collection. If no services or names are specified, the
        /// default services will be IList&lt;T&gt;, ICollection&lt;T&gt;, and IEnumerable&lt;T&gt;        
        /// </summary>
        /// <typeparam name="T">The type of the collection elements.</typeparam>
        /// <param name="builder">Container builder.</param>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        public static RegistrationBuilder<T[], SimpleActivatorData, SingleRegistrationStyle>
            RegisterCollection<T>(this ContainerBuilder builder)
        {
            Enforce.ArgumentNotNull(builder, "builder");

            IEnumerable<IComponentRegistration> elements = null;
            IEnumerable<Service> memberServices = null;

            var activator = new DelegateActivator(typeof(T[]), (c, p) =>
            {
                if (elements == null)
                {
                    c.ComponentRegistry.Registered += (sender, e) =>
                    {
                        if (e.ComponentRegistration.Services.Any(sv => memberServices.Contains(sv)))
                            elements = memberServices.SelectMany(s => c.ComponentRegistry.RegistrationsFor(s)).Distinct();
                    };
                    elements = memberServices.SelectMany(s => c.ComponentRegistry.RegistrationsFor(s)).Distinct();
                }

                var items = elements.Select(e => c.Resolve(e, p)).ToArray();

                Array result = Array.CreateInstance(typeof(T), items.Length);
                items.CopyTo(result, 0);
                return result;
            });

            var rb = new RegistrationBuilder<T[], SimpleActivatorData, SingleRegistrationStyle>(
                new SimpleActivatorData(activator),
                new SingleRegistrationStyle());

            builder.RegisterCallback(cr => {
                RegistrationExtensions.RegisterSingleComponent(cr, rb, activator);
                memberServices = rb.RegistrationData.Services.Select(s => new MemberService(s)).Cast<Service>();
            });

            return rb;
        }

        /// <summary>
        /// Include the element explicitly in a collection configured using RegisterCollection.
        /// </summary>
        /// <typeparam name="TLimit">Registration limit type.</typeparam>
        /// <typeparam name="TSingleRegistrationStyle">Registration style.</typeparam>
        /// <typeparam name="TActivatorData">Activator data type.</typeparam>
        /// <param name="registration">Registration to export.</param>
        /// <param name="service">The collection type to include this item in.</param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        public static RegistrationBuilder<TLimit, TActivatorData, TSingleRegistrationStyle>
            MemberOf<TLimit, TActivatorData, TSingleRegistrationStyle>(
                this RegistrationBuilder<TLimit, TActivatorData, TSingleRegistrationStyle> registration,
                Service service)
            where TSingleRegistrationStyle : SingleRegistrationStyle
        {
            return registration.As(new MemberService(service));
        }
    }
}
