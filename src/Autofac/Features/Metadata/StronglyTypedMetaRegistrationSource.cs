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
using System.Reflection;
using Autofac.Builder;
using Autofac.Core;
using Autofac.Util;

namespace Autofac.Features.Metadata
{
    /// <summary>
    /// Support the <see cref="Meta{T, TMetadata}"/>
    /// types automatically whenever type T is registered with the container.
    /// Metadata values come from the component registration's metadata.
    /// </summary>
    internal class StronglyTypedMetaRegistrationSource : IRegistrationSource
    {
        private static readonly MethodInfo CreateMetaRegistrationMethod = typeof(StronglyTypedMetaRegistrationSource).GetTypeInfo().GetDeclaredMethod("CreateMetaRegistration");

        private delegate IComponentRegistration RegistrationCreator(Service service, IComponentRegistration valueRegistration);

        public IEnumerable<IComponentRegistration> RegistrationsFor(Service service, Func<Service, IEnumerable<IComponentRegistration>> registrationAccessor)
        {
            if (registrationAccessor == null) throw new ArgumentNullException(nameof(registrationAccessor));

            var swt = service as IServiceWithType;
            if (swt == null || !swt.ServiceType.IsGenericTypeDefinedBy(typeof(Meta<,>)))
                return Enumerable.Empty<IComponentRegistration>();

            var genericArguments = swt.ServiceType.GetTypeInfo().GenericTypeArguments.ToArray();
            var valueType = genericArguments[0];
            var metaType = genericArguments[1];

            if (!metaType.GetTypeInfo().IsClass)
                return Enumerable.Empty<IComponentRegistration>();

            var valueService = swt.ChangeType(valueType);
            var methodInfo = CreateMetaRegistrationMethod.MakeGenericMethod(valueType, metaType);
            var registrationCreator = (RegistrationCreator)methodInfo.CreateDelegate(
                typeof(RegistrationCreator), null);

            return registrationAccessor(valueService)
                .Select(v => registrationCreator.Invoke(service, v));
        }

        public bool IsAdapterForIndividualComponents => true;

        public override string ToString()
        {
            return MetaRegistrationSourceResources.StronglyTypedMetaRegistrationSourceDescription;
        }

        private static IComponentRegistration CreateMetaRegistration<T, TMetadata>(Service providedService, IComponentRegistration valueRegistration)
        {
            var metadata = MetadataViewProvider.GetMetadataViewProvider<TMetadata>()(valueRegistration.Target.Metadata);

            var rb = RegistrationBuilder
                .ForDelegate((c, p) => new Meta<T, TMetadata>((T)c.ResolveComponent(valueRegistration, p), metadata))
                .As(providedService)
                .Targeting(valueRegistration);

            return rb.CreateRegistration();
        }
    }
}
