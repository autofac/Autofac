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

namespace Autofac.Features.LazyDependencies
{
    /// <summary>
    /// Support the <see cref="System.Lazy{T, TMetadata}"/>
    /// types automatically whenever type T is registered with the container.
    /// Metadata values come from the component registration's metadata.
    /// When a dependency of a lazy type is used, the instantiation of the underlying
    /// component will be delayed until the Value property is first accessed.
    /// </summary>
    class LazyWithMetadataRegistrationSource : IRegistrationSource
    {
        static readonly MethodInfo CreateLazyRegistrationMethod = typeof(LazyWithMetadataRegistrationSource).GetMethod(
            "CreateLazyRegistration", BindingFlags.Static | BindingFlags.NonPublic);

        delegate IComponentRegistration RegistrationCreator(Service service, IComponentRegistration valueRegistration);

        public IEnumerable<IComponentRegistration> RegistrationsFor(Service service, Func<Service, IEnumerable<IComponentRegistration>> registrationAccessor)
        {
            var swt = service as IServiceWithType;
            if (swt == null || !swt.ServiceType.IsClosingTypeOf(typeof(Lazy<,>)))
                return Enumerable.Empty<IComponentRegistration>();

            var valueType = swt.ServiceType.GetGenericArguments()[0];
            var metaType = swt.ServiceType.GetGenericArguments()[1];

            var valueService = swt.ChangeType(valueType);

            var registrationCreator = (RegistrationCreator)Delegate.CreateDelegate(
                typeof(RegistrationCreator),
                CreateLazyRegistrationMethod.MakeGenericMethod(valueType, metaType));

            return registrationAccessor(valueService)
                .Select(v => registrationCreator(service, v));
        }

        public bool IsAdapterForIndividualComponents
        {
            get { return true; }
        }

        // ReSharper disable UnusedMember.Local
        static IComponentRegistration CreateLazyRegistration<T, TMeta>(Service providedService, IComponentRegistration valueRegistration)
        // ReSharper restore UnusedMember.Local
        {
            var rb = RegistrationBuilder.ForDelegate(
                (c, p) =>
                {
                    var context = c.Resolve<IComponentContext>();
                    return new Lazy<T, TMeta>(
                        () => (T) context.ResolveComponent(valueRegistration, p),
                        AttributedModelServices.GetMetadataView<TMeta>(valueRegistration.Target.Metadata));
                })
                .As(providedService)
                .Targeting(valueRegistration);

            return rb.CreateRegistration();
        }
    }
}
#endif