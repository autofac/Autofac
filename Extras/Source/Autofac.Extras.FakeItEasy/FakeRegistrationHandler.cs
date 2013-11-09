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
using System.Linq.Expressions;
using System.Reflection;
using System.Security;
using Autofac.Builder;
using Autofac.Core;
using FakeItEasy;
using FakeItEasy.Creation;

namespace Autofac.Extras.FakeItEasy
{
    /// <summary> Resolves unknown interfaces and Fakes. </summary>
    internal class FakeRegistrationHandler : IRegistrationSource
    {
        private readonly MethodInfo _createMethod;
        private readonly bool _strict;
        private readonly bool _callsBaseMethods;
        private readonly bool _callsDoNothing;
        private readonly Action<object> _onFakeCreated;

        /// <summary>
        /// Initializes a new instance of the <see cref="FakeRegistrationHandler" /> class.
        /// </summary>
        /// <param name="strict">Whether fakes should be created with strict semantics.</param>
        /// <param name="callsBaseMethods">Whether fakes should call base methods.</param>
        /// <param name="callsDoNothing">Whether calls to fakes should do nothing.</param>
        /// <param name="onFakeCreated">An action to perform on a fake when it is created.</param>
        [SecurityCritical]
        public FakeRegistrationHandler(bool strict, bool callsBaseMethods, bool callsDoNothing, Action<object> onFakeCreated)
        {
            this._strict = strict;
            this._callsBaseMethods = callsBaseMethods;
            this._callsDoNothing = callsDoNothing;
            this._onFakeCreated = onFakeCreated;

            // NOTE (adamralph): inspired by http://blog.functionalfun.net/2009/10/getting-methodinfo-of-generic-method.html
            Expression<Action> create = () => this.CreateFake<object>();
            this._createMethod = (create.Body as MethodCallExpression).Method.GetGenericMethodDefinition();
        }

        /// <summary>
        /// Gets whether the registrations provided by this source are 1:1 adapters on top
        /// of other components (I.e. like Meta, Func or Owned.)
        /// </summary>
        public bool IsAdapterForIndividualComponents
        {
            get { return false; }
        }

        /// <summary>
        /// Retrieve registrations for an unregistered service, to be used
        /// by the container.
        /// </summary>
        /// <param name="service">The service that was requested.</param>
        /// <param name="registrationAccessor">A function that will return existing registrations for a service.</param>
        /// <returns>Registrations providing the service.</returns>
        [SecuritySafeCritical]
        public IEnumerable<IComponentRegistration> RegistrationsFor(
            Service service, Func<Service, IEnumerable<IComponentRegistration>> registrationAccessor)
        {
            if (service == null)
            {
                throw new ArgumentNullException("service");
            }

            var typedService = service as TypedService;
            if (typedService == null ||
                (!typedService.ServiceType.IsInterface && !typedService.ServiceType.IsAbstract) ||
                (typedService.ServiceType.IsGenericType && typedService.ServiceType.GetGenericTypeDefinition() == typeof(IEnumerable<>)) ||
                typedService.ServiceType.IsArray ||
                typeof(IStartable).IsAssignableFrom(typedService.ServiceType))
            {
                return Enumerable.Empty<IComponentRegistration>();
            }

            var rb = RegistrationBuilder.ForDelegate((c, p) => this.CreateFake(typedService))
                .As(service)
                .InstancePerLifetimeScope();

            return new[] { rb.CreateRegistration() };
        }

        /// <summary>
        /// Creates a fake object.
        /// </summary>
        /// <param name="typedService">The typed service.</param>
        /// <returns>A fake object.</returns>
        [SecuritySafeCritical]
        private object CreateFake(TypedService typedService)
        {
            return this._createMethod.MakeGenericMethod(new[] { typedService.ServiceType }).Invoke(this, null);
        }

        [SecuritySafeCritical]
        private T CreateFake<T>()
        {
            var fake = A.Fake<T>(this.ApplyOptions);

            if (this._callsBaseMethods)
            {
                A.CallTo(fake).CallsBaseMethod();
            }

            if (this._callsDoNothing)
            {
                A.CallTo(fake).DoesNothing();
            }

            return fake;
        }

        [SecuritySafeCritical]
        private void ApplyOptions<T>(IFakeOptionsBuilder<T> options)
        {
            if (this._strict)
            {
                options.Strict();
            }

            if (this._onFakeCreated != null)
            {
                options.OnFakeCreated(x => this._onFakeCreated(x));
            }
        }
    }
}
