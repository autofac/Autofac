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
using Moq;

namespace Autofac.Extras.Moq
{
    /// <summary>
    /// Wrapper around <see cref="Autofac"/> and <see cref="Moq"/>
    /// </summary>
    [SecurityCritical]
    public class AutoMock : IDisposable
    {
        private bool _disposed;

        /// <summary> 
        /// <see cref="MockRepository"/> instance responsible for expectations and mocks. 
        /// </summary>
        public MockRepository MockRepository { [SecurityCritical]get; [SecurityCritical]private set; }

        /// <summary>
        /// <see cref="IContainer"/> that handles the component resolution.
        /// </summary>
        public IContainer Container { get; private set; }

        private AutoMock(MockBehavior behavior)
            : this(new MockRepository(behavior))
        {
        }

        private AutoMock(MockRepository repository)
        {
            MockRepository = repository;
            var builder = new ContainerBuilder();
            builder.RegisterInstance(MockRepository);
            builder.RegisterSource(new AnyConcreteTypeNotAlreadyRegisteredSource());
            builder.RegisterSource(new MoqRegistrationHandler());
            Container = builder.Build();
            VerifyAll = false;
        }

        /// <summary>
        /// Create new <see cref="AutoMock"/> instance with loose mock behavior.
        /// </summary>
        /// <seealso cref="MockRepository"/>
        /// <returns>Container initialized for loose behavior.</returns>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public static AutoMock GetLoose()
        {
            return new AutoMock(MockBehavior.Loose);
        }

        /// <summary>
        /// Create new <see cref="AutoMock"/> instance with strict mock behavior.
        /// </summary>
        /// <seealso cref="MockRepository"/>
        /// <returns>Container initialized for loose behavior.</returns>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public static AutoMock GetStrict()
        {
            return new AutoMock(MockBehavior.Strict);
        }

        /// <summary>
        /// Create new <see cref="AutoMock"/> instance that will create mocks with behavior defined by <c>repository</c>.
        /// </summary>
        /// <param name="repository"></param>
        /// <returns></returns>
        public static AutoMock GetFromRepository(MockRepository repository)
        {
            return new AutoMock(repository);
        }

        /// <summary>
        /// Verifies mocks and disposes internal container.
        /// </summary>
        [SecuritySafeCritical]
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Handles disposal of managed and unmanaged resources.
        /// </summary>
        /// <param name="disposing">
        /// <see langword="true" /> to dispose of managed resources (during a manual execution
        /// of <see cref="Autofac.Extras.Moq.AutoMock.Dispose()"/>); or
        /// <see langword="false" /> if this is getting run as part of finalization where
        /// managed resources may have already been cleaned up.
        /// </param>
        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // We can only verify things with the mock
                    // repository if it hasn't already been garbage
                    // collected during finalization.
                    try
                    {
                        if (VerifyAll)
                        {
                            MockRepository.VerifyAll();
                        }
                        else
                        {
                            MockRepository.Verify();
                        }
                    }
                    finally
                    {
                        Container.Dispose();
                    }
                }
                _disposed = true;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether all mocks are verified.
        /// </summary>
        /// <value><c>true</c> to verify all mocks. <c>false</c> (default) to verify only mocks marked Verifiable.</value>
        public bool VerifyAll { get; set; }

        /// <summary>
        /// Finds (creating if needed) the actual mock for the provided type
        /// </summary>
        /// <typeparam name="T">Type to mock</typeparam>
        /// <param name="parameters">Optional parameters</param>
        /// <returns>Mock</returns>
        public Mock<T> Mock<T>(params Parameter[] parameters) where T : class
        {
            var obj = (IMocked<T>)Create<T>(parameters);
            return obj.Mock;
        }

        /// <summary>
        /// Resolve the specified type in the container (register it if needed)
        /// </summary>
        /// <typeparam name="T">Service</typeparam>
        /// <param name="parameters">Optional parameters</param>
        /// <returns>The service.</returns>
        public T Create<T>(params Parameter[] parameters)
        {
            return Container.Resolve<T>(parameters);
        }

        /// <summary>
        /// Resolve the specified type in the container (register it if needed)
        /// </summary>
        /// <typeparam name="TService">Service</typeparam>
        /// <typeparam name="TImplementation">The implementation of the service.</typeparam>
        /// <param name="parameters">Optional parameters</param>
        /// <returns>The service.</returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "The component registry is responsible for registration disposal.")]
        public TService Provide<TService, TImplementation>(params Parameter[] parameters)
        {
            Container.ComponentRegistry.Register(
                RegistrationBuilder.ForType<TImplementation>().As<TService>().InstancePerLifetimeScope().CreateRegistration());

            return Container.Resolve<TService>(parameters);
        }

        /// <summary>
        /// Resolve the specified type in the container (register specified instance if needed)
        /// </summary>
        /// <typeparam name="TService">Service</typeparam>
        /// <returns>The instance resolved from container.</returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "The component registry is responsible for registration disposal.")]
        public TService Provide<TService>(TService instance)
            where TService : class
        {
            Container.ComponentRegistry.Register(
                RegistrationBuilder.ForDelegate((c, p) => instance).InstancePerLifetimeScope().CreateRegistration());

            return Container.Resolve<TService>();
        }
    }
}
