// This software is part of the Autofac IoC container
// Copyright © 2007 - 2008 Autofac Contributors
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
using Autofac.Util;

namespace Autofac.Features
{
    public abstract class ImplicitRegistrationSource : IRegistrationSource
    {
        private static readonly MethodInfo CreateRegistrationMethod = typeof(ImplicitRegistrationSource).GetTypeInfo().GetDeclaredMethod(nameof(CreateRegistration));

        private readonly Type _type;

        protected ImplicitRegistrationSource(Type type)
        {
            _type = type ?? throw new ArgumentNullException(nameof(type));

            if (!type.IsGenericType)
            {
                throw new InvalidOperationException(ImplicitRegistrationSourceResources.TypeMustBeGeneric);
            }

            if (type.GetGenericArguments().Length != 1)
            {
                throw new InvalidOperationException(ImplicitRegistrationSourceResources.GenericTypeMustBeUnary);
            }
        }

        public IEnumerable<IComponentRegistration> RegistrationsFor(Service service, Func<Service, IEnumerable<IComponentRegistration>> registrationAccessor)
        {
            if (registrationAccessor == null)
            {
                throw new ArgumentNullException(nameof(registrationAccessor));
            }

            if (!(service is IServiceWithType swt) || !swt.ServiceType.IsGenericTypeDefinedBy(_type))
            {
                return Enumerable.Empty<IComponentRegistration>();
            }

            var valueType = swt.ServiceType.GetTypeInfo().GenericTypeArguments[0];
            var valueService = swt.ChangeType(valueType);

            var registrationCreator = CreateRegistrationMethod.MakeGenericMethod(valueType);

            return registrationAccessor(valueService)
                .Select(v => registrationCreator.Invoke(this, new object[] { service, valueService, v }))
                .Cast<IComponentRegistration>();
        }

        public virtual bool IsAdapterForIndividualComponents => true;

        public virtual string Description => GetType().GetTypeInfo().Name;

        public override string ToString() => Description;

        protected abstract object ResolveInstance<T>(IComponentContext ctx, ResolveRequest request)
            where T : notnull;

        protected virtual IRegistrationBuilder<object, SimpleActivatorData, SingleRegistrationStyle> BuildRegistration(IRegistrationBuilder<object, SimpleActivatorData, SingleRegistrationStyle> registration)
            => registration;

        private IComponentRegistration CreateRegistration<T>(Service providedService, Service valueService, IComponentRegistration valueRegistration)
            where T : notnull
        {
            var registrationDelegate = RegistrationBuilder.ForDelegate(
                (c, p) =>
                {
                    var request = new ResolveRequest(valueService, valueRegistration, p);

                    return ResolveInstance<T>(c, request);
                });

            var rb = BuildRegistration(registrationDelegate)
                .As(providedService)
                .Targeting(valueRegistration, IsAdapterForIndividualComponents)
                .InheritRegistrationOrderFrom(valueRegistration);

            return rb.CreateRegistration();
        }
    }
}
