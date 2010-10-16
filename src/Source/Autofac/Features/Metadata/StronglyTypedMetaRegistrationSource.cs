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
#if !WINDOWS_PHONE
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
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
    class StronglyTypedMetaRegistrationSource : IRegistrationSource
    {
        static readonly MethodInfo CreateMetaRegistrationMethod = typeof(StronglyTypedMetaRegistrationSource).GetMethod(
            "CreateMetaRegistration", BindingFlags.Static | BindingFlags.NonPublic);

        public IEnumerable<IComponentRegistration> RegistrationsFor(Service service, Func<Service, IEnumerable<IComponentRegistration>> registrationAccessor)
        {
            var swt = service as IServiceWithType;
            if (swt == null || !swt.ServiceType.IsClosingTypeOf(typeof(Meta<,>)))
                return Enumerable.Empty<IComponentRegistration>();

            var valueType = swt.ServiceType.GetGenericArguments()[0];
            var metaType = swt.ServiceType.GetGenericArguments()[1];

            var valueService = swt.ChangeType(valueType);

            var registrationCreator = CreateMetaRegistrationMethod.MakeGenericMethod(valueType, metaType);

            return registrationAccessor(valueService)
                .Select(v => registrationCreator.Invoke(null, new object[] { service, v }))
                .Cast<IComponentRegistration>();
        }

        public bool IsAdapterForIndividualComponents
        {
            get { return true; }
        }

        static IComponentRegistration CreateMetaRegistration<T, TMetadata>(Service providedService, IComponentRegistration valueRegistration)
        {
            var rb = RegistrationBuilder.ForDelegate(
                (c, p) =>
                {
                    var context = c.Resolve<IComponentContext>();
                    return new Meta<T, TMetadata>(
                        (T) context.ResolveComponent(valueRegistration, p),
                        AttributedModelServices.GetMetadataView<TMetadata>(valueRegistration.Target.Metadata));
                })
                .As(providedService)
                .Targeting(valueRegistration);

            return rb.CreateRegistration();
        }
    }
}
#endif
