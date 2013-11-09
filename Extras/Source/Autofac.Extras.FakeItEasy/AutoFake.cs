// This software is part of the Autofac IoC container
// Copyright (c) 2013 Autofac Contributors
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
using System.Diagnostics.CodeAnalysis;
using System.Security;
using Autofac.Builder;
using Autofac.Core;
using Autofac.Features.ResolveAnything;

namespace Autofac.Extras.FakeItEasy
{
    /// <summary>
    /// Wrapper around <see cref="Autofac"/> and <see cref="FakeItEasy"/>
    /// </summary>
    [SecurityCritical]
    public class AutoFake : IDisposable
    {
        private readonly IContainer _container;
        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="AutoFake" /> class.
        /// </summary>
        /// <param name="strict">
        /// <see langword="true" /> to create strict fakes.
        /// This means that any calls to the fakes that have not been explicitly configured will throw an exception.
        /// </param>
        /// <param name="callsBaseMethods">
        /// <see langword="true" /> to delegate configured method calls to the base method of the faked method.
        /// </param>
        /// <param name="callsDoNothing">
        /// <see langword="true" /> to configure fake calls to do nothing when called.
        /// </param>
        /// <param name="builder">The container builder to use to build the container.</param>
        /// <param name="onFakeCreated">Specifies an action that should be run over a fake object once it's created.</param>
        public AutoFake(
            bool strict = false,
            bool callsBaseMethods = false,
            bool callsDoNothing = false,
            Action<object> onFakeCreated = null,
            ContainerBuilder builder = null)
        {
            if (builder == null)
            {
                builder = new ContainerBuilder();
            }

            builder.RegisterSource(new AnyConcreteTypeNotAlreadyRegisteredSource().WithRegistrationsAs(b => b.InstancePerLifetimeScope()));
            builder.RegisterSource(new FakeRegistrationHandler(strict, callsBaseMethods, callsDoNothing, onFakeCreated));
            this._container = builder.Build();
            this._container.BeginLifetimeScope();
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="AutoFake"/> class.
        /// </summary>
        [SecuritySafeCritical]
        ~AutoFake()
        {
            this.Dispose(false);
        }

        /// <summary>
        /// Gets the <see cref="IContainer"/> that handles the component resolution.
        /// </summary>
        public IContainer Container
        {
            get { return this._container; }
        }

        /// <summary>
        /// Disposes internal container.
        /// </summary>
        [SecuritySafeCritical]
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Resolve the specified type in the container (register it if needed)
        /// </summary>
        /// <typeparam name="T">The type of the service.</typeparam>
        /// <param name="parameters">Optional parameters</param>
        /// <returns>The service.</returns>
        public T Resolve<T>(params Parameter[] parameters)
        {
            return this.Container.Resolve<T>(parameters);
        }

        [Obsolete("Use Resolve<T>() instead")]
        public T Create<T>(params Parameter[] parameters)
        {
            return this.Resolve<T>(parameters);
        }

        /// <summary>
        /// Resolve the specified type in the container (register it if needed)
        /// </summary>
        /// <typeparam name="TService">The type of the service.</typeparam>
        /// <typeparam name="TImplementation">The implementation of the service.</typeparam>
        /// <param name="parameters">Optional parameters</param>
        /// <returns>The service.</returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "The component registry is responsible for registration disposal.")]
        public TService Provide<TService, TImplementation>(params Parameter[] parameters)
        {
            this.Container.ComponentRegistry.Register(
                RegistrationBuilder.ForType<TImplementation>().As<TService>().InstancePerLifetimeScope().CreateRegistration());

            return this.Container.Resolve<TService>(parameters);
        }

        /// <summary>
        /// Resolve the specified type in the container (register specified instance if needed)
        /// </summary>
        /// <typeparam name="TService">The type of the service.</typeparam>
        /// <param name="instance">The instance to register if needed.</param>
        /// <returns>The instance resolved from container.</returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "The component registry is responsible for registration disposal.")]
        public TService Provide<TService>(TService instance)
            where TService : class
        {
            this.Container.ComponentRegistry.Register(
                RegistrationBuilder.ForDelegate((c, p) => instance).InstancePerLifetimeScope().CreateRegistration());

            return this.Container.Resolve<TService>();
        }

        /// <summary>
        /// Handles disposal of managed and unmanaged resources.
        /// </summary>
        /// <param name="disposing">
        /// <see langword="true" /> to dispose of managed resources (during a manual execution
        /// of <see cref="Autofac.Extras.FakeItEasy.AutoFake.Dispose()"/>); or
        /// <see langword="false" /> if this is getting run as part of finalization where
        /// managed resources may have already been cleaned up.
        /// </param>
        private void Dispose(bool disposing)
        {
            if (!this._disposed)
            {
                if (disposing)
                {
                    this.Container.Dispose();
                }

                this._disposed = true;
            }
        }
    }
}
