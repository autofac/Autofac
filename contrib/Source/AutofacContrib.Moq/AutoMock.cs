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
using Autofac;
using Autofac.Builder;
using Autofac.Core;
using Autofac.Features.ResolveAnything;
using Moq;

namespace AutofacContrib.Moq
{
    /// <summary>
    /// Wrapper around the <see cref="Autofac"/> and <see cref="Moq"/>
    /// </summary>
    public class AutoMock : IDisposable
    {
        /// <summary> 
        /// <see cref="MockFactory"/> instance responsible for expectations and mocks. 
        /// </summary>
        public MockFactory MockFactory { get; private set; }

        /// <summary>
        /// <see cref="IContainer"/> that handles the component resolution.
        /// </summary>
        public IContainer Container { get; private set; }

        private AutoMock(MockBehavior behavior)
        {
            MockFactory = new MockFactory(behavior);
            var builder = new ContainerBuilder();
            builder.RegisterInstance(MockFactory);
            builder.RegisterSource(new AnyConcreteTypeNotAlreadyRegisteredSource());
            builder.RegisterSource(new MoqRegistrationHandler());
            Container = builder.Build();
            VerifyAll = false;
        }

        /// <summary>
        /// Create new <see cref="AutoMock"/> instance with strict loose behavior.
        /// </summary>
        /// <seealso cref="MockFactory"/>
        /// <returns>Container initialized for loose behavior.</returns>
        public static AutoMock GetLoose()
        {
            return new AutoMock(MockBehavior.Loose);
        }

        /// <summary>
        /// Create new <see cref="AutoMock"/> instance with strict mock behavior.
        /// </summary>
        /// <seealso cref="MockFactory"/>
        /// <returns>Container initialized for loose behavior.</returns>
        public static AutoMock GetStrict()
        {
            return new AutoMock(MockBehavior.Strict);
        }

        /// <summary>
        /// Verifies mocks and disposes internal container.
        /// </summary>
        public void Dispose()
        {
            try
            {
                if (VerifyAll)
                    MockFactory.VerifyAll();
                else
                    MockFactory.Verify();
            }
            finally
            {
                Container.Dispose();
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
        public Mock<T> Mock<T>(params object[] parameters) where T : class
        {
            var obj = (IMocked<T>)Create<T>();
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
        public TService Provide<TService>(TService instance)
            where TService : class
        {
            Container.ComponentRegistry.Register(
                RegistrationBuilder.ForDelegate((c, p) => instance).InstancePerLifetimeScope().CreateRegistration());

            return Container.Resolve<TService>();
        }
    }
}
