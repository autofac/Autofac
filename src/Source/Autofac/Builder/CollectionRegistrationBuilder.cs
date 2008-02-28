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
using Autofac.Registrars;
using Autofac.Registrars.Collection;

namespace Autofac.Builder
{
    /// <summary>
    /// Extends the ContainerBuilder to allow items to be added to previously-registered
    /// service collections.
    /// </summary>
    public static class CollectionRegistrationBuilder
    {
        /// <summary>
        /// Registers the type as a collection.
        /// </summary>
        /// <typeparam name="T">Collection item type</typeparam>
        /// <param name="builder">The builder.</param>
        /// <returns></returns>
        public static IConcreteRegistrar RegisterCollection<T>(this ContainerBuilder builder)
        {
            Enforce.ArgumentNotNull(builder, "builder");

            return builder.AttachRegistrar<IConcreteRegistrar>(
            	new CollectionRegistrar<T>());
        }

        /// <summary>
        /// Adds the registration to a previously registered collection.
        /// </summary>
        /// <typeparam name="TSyntax">The registrar's self-type.</typeparam>
        /// <param name="registrar">The registrar.</param>
        /// <param name="serviceName">Name of the service.</param>
        public static TSyntax MemberOf<TSyntax>(this TSyntax registrar, string serviceName)
            where TSyntax : class, IConcreteRegistrar<TSyntax>
        {
            Enforce.ArgumentNotNull(registrar, "registrar");
            Enforce.ArgumentNotNullOrEmpty(serviceName, "serviceName");
            return MemberOf(registrar, new NamedService(serviceName));
        }

        /// <summary>
        /// Adds the registration to a previously registered collection.
        /// </summary>
        /// <typeparam name="TSyntax">The registrar's self-type.</typeparam>
        /// <param name="registrar">The registrar.</param>
        /// <param name="serviceType">Type of the service.</param>
        public static TSyntax MemberOf<TSyntax>(this TSyntax registrar, Type serviceType)
            where TSyntax : class, IConcreteRegistrar<TSyntax>
        {
            Enforce.ArgumentNotNull(registrar, "registrar");
            Enforce.ArgumentNotNull(serviceType, "serviceType");
            return MemberOf(registrar, new TypedService(serviceType));
        }

        /// <summary>
        /// Adds the registration to a previously registered collection.
        /// </summary>
        /// <typeparam name="TSyntax">The registrar's self-type.</typeparam>
        /// <param name="registrar">The registrar.</param>
        /// <param name="service">The service.</param>
        public static TSyntax MemberOf<TSyntax>(this TSyntax registrar, Service service)
            where TSyntax : class, IConcreteRegistrar<TSyntax>
        {
            Enforce.ArgumentNotNull(registrar, "registrar");
            Enforce.ArgumentNotNull(service, "service");

            new CollectionRegistrationAppender(service).Add(registrar);
            
            return registrar;
        }
    }
}
