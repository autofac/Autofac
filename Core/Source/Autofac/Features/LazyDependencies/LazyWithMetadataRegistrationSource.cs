﻿// This software is part of the Autofac IoC container
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
using System.Reflection;
using Autofac.Builder;
using Autofac.Core;
using Autofac.Features.Metadata;
using Autofac.Util;

namespace Autofac.Features.LazyDependencies
{
    /// <summary>
    /// Support the <c>System.Lazy&lt;T, TMetadata&gt;</c>
    /// types automatically whenever type T is registered with the container.
    /// Metadata values come from the component registration's metadata.
    /// When a dependency of a lazy type is used, the instantiation of the underlying
    /// component will be delayed until the Value property is first accessed.
    /// </summary>
    class LazyWithMetadataRegistrationSource : IRegistrationSource
    {
        static readonly MethodInfo CreateLazyRegistrationMethod = typeof(LazyWithMetadataRegistrationSource).GetTypeInfo().GetDeclaredMethod("CreateLazyRegistration");

        delegate IComponentRegistration RegistrationCreator(Service service, IComponentRegistration valueRegistration);

        public IEnumerable<IComponentRegistration> RegistrationsFor(Service service, Func<Service, IEnumerable<IComponentRegistration>> registrationAccessor)
        {
            if (registrationAccessor == null)
            {
                throw new ArgumentNullException("registrationAccessor");
            }
            var swt = service as IServiceWithType;
            var lazyType = GetLazyType(swt);
            if (swt == null || lazyType == null || !swt.ServiceType.IsGenericTypeDefinedBy(lazyType))
                return Enumerable.Empty<IComponentRegistration>();

            var genericTypeArguments = swt.ServiceType.GetTypeInfo().GenericTypeArguments.ToArray();
            var valueType = genericTypeArguments[0];
            var metaType = genericTypeArguments[1];

            if (!metaType.GetTypeInfo().IsClass)
                return Enumerable.Empty<IComponentRegistration>();

            var valueService = swt.ChangeType(valueType);

            var registrationCreator = (RegistrationCreator)(CreateLazyRegistrationMethod.MakeGenericMethod(
                valueType, metaType)).CreateDelegate(typeof(RegistrationCreator));

            return registrationAccessor(valueService)
                .Select(v => registrationCreator(service, v));
        }

        public bool IsAdapterForIndividualComponents
        {
            get { return true; }
        }

        public override string ToString()
        {
            return LazyWithMetadataRegistrationSourceResources.LazyWithMetadataRegistrationSourceDescription;
        }

        // ReSharper disable UnusedMember.Local
        static IComponentRegistration CreateLazyRegistration<T, TMeta>(Service providedService, IComponentRegistration valueRegistration)
        // ReSharper restore UnusedMember.Local
        {
            var metadataProvider = MetadataViewProvider.GetMetadataViewProvider<TMeta>();
            var metadata = metadataProvider(valueRegistration.Target.Metadata);

            var rb = RegistrationBuilder.ForDelegate(
                (c, p) =>
                {
                    var context = c.Resolve<IComponentContext>();
                    var lazyType = ((IServiceWithType)providedService).ServiceType;
                    var valueFactory = new Func<T>(() => (T)context.ResolveComponent(valueRegistration, p));
                    return Activator.CreateInstance(lazyType, valueFactory, metadata);
                })
                .As(providedService)
                .Targeting(valueRegistration);

            return rb.CreateRegistration();
        }

        static Type GetLazyType(IServiceWithType serviceWithType)
        {
            return serviceWithType != null
                   && serviceWithType.ServiceType.GetTypeInfo().IsGenericType
                   && serviceWithType.ServiceType.GetGenericTypeDefinition().FullName == "System.Lazy`2"
                       ? serviceWithType.ServiceType.GetGenericTypeDefinition()
                       : null;
        }
    }
}
