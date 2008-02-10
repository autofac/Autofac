// This software is part of the Autofac IoC container
// Copyright (c) 2007 Nicholas Blumhardt
// nicholas.blumhardt@gmail.com
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
using Autofac.Component.Registration;

namespace Autofac.Builder
{
    /// <summary>
    /// Extends the ContainerBuilder to allow items to be added to previously-registered
    /// service collections.
    /// </summary>
    public static class CollectionRegistrationBuilder
    {
        public static IConcreteRegistrar RegisterCollection<T>(this ContainerBuilder builder)
        {
            Enforce.ArgumentNotNull(builder, "builder");

            var result = new CollectionRegistrar<T>();
            builder.RegisterModule(result);
            return result
                .WithOwnership(builder.DefaultOwnership)
                .WithScope(builder.DefaultScope);
        }

        public static void MemberOf<TSyntax>(this IConcreteRegistrar<TSyntax> registrar, string serviceName)
            where TSyntax : IConcreteRegistrar<TSyntax>
        {
            Enforce.ArgumentNotNull(registrar, "registrar");
            Enforce.ArgumentNotNullOrEmpty(serviceName, "serviceName");
            MemberOf(registrar, new NamedService(serviceName));
        }

        public static void MemberOf<TSyntax>(this IConcreteRegistrar<TSyntax> registrar, Type serviceType)
            where TSyntax : IConcreteRegistrar<TSyntax>
        {
            Enforce.ArgumentNotNull(registrar, "registrar");
            Enforce.ArgumentNotNull(serviceType, "serviceType");
            MemberOf(registrar, new TypedService(serviceType));
        }

        public static void MemberOf<TSyntax>(this IConcreteRegistrar<TSyntax> registrar, Service service)
            where TSyntax : IConcreteRegistrar<TSyntax>
        {
            Enforce.ArgumentNotNull(registrar, "registrar");
            Enforce.ArgumentNotNull(service, "service");

            var key = new UniqueService();
            var next = registrar.As(key);
            registrar.OnRegistered((sender, e) =>
            {
                IDisposer disposer;
                IComponentRegistration serviceListRegistration;
                bool found = ((IRegistrationContext)e.Container)
                    .TryGetLocalRegistration(
                        service,
                        out serviceListRegistration,
                        out disposer);

                if (!found)
                    throw new ComponentNotRegisteredException(service);

                var serviceList = (IServiceListRegistration)serviceListRegistration;
                serviceList.Add(key);
            });
        }
    }
}
