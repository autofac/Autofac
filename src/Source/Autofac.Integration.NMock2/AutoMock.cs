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
using NMock2;

namespace Autofac.Integration.NMock2
{
	/// <summary>
	/// Wrapper around the Autofac and NMock2
	/// </summary>
	public class AutoMock : IDisposable
	{
		/// <summary> 
		/// <see cref="NMock2.Mockery"/> instance responsible for expectations and mocks. 
		/// </summary>
		public Mockery Mockery { get; private set; }

		/// <summary>
		/// <see cref="IContainer"/> that handles the component resolution.
		/// </summary>
		public IContainer Container { get; private set;}

		private AutoMock(Mockery mockery, IContainer container)
		{
			Mockery = mockery;
			Container = container;

			Initialize();
		}

		private void Initialize()
		{
			Container.AddRegistrationSource(new NMockRegistrationHandler());
		}

		/// <summary>
		/// Create new AutoMock instance with unordered expectations 
		/// (to be verified when the container is disposed).
		/// </summary>
		public AutoMock()
		{
			Mockery = new Mockery();
			var builder = new ContainerBuilder();
			builder.Register(Mockery).OwnedByContainer();
			Container = builder.Build();

			Initialize();
		}

		/// <summary>
		/// Same as the default constructor
		/// </summary>
		/// <returns></returns>
		public static AutoMock GetUnordered()
		{
			return new AutoMock();
		}

		/// <summary>
		/// Create new <see cref="AutoMock"/> instance with ordered expectations
		/// (to be verified when the container is disposed)
		/// </summary>
		/// <seealso cref="Mockery"/>
		public static AutoMock GetOrdered()
		{
			var mockery = new Mockery();
			var builder = new ContainerBuilder();
			builder.Register(mockery).ExternallyOwned();
			builder.Register(mockery.Ordered).OwnedByContainer();
			return new AutoMock(mockery, builder.Build());
		}

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
		public void Dispose()
		{
			Container.Dispose();
		}

		/// <summary>
		/// Finds (creating if needed) the mock for the provided type
		/// </summary>
		/// <typeparam name="T">Type to mock</typeparam>
		/// <returns>Mock</returns>
		public T Resolve<T>()
		{
			return Container.Resolve<T>();
		}

		/// <summary>
		/// Resolve the specified type in the container (register it if needed)
		/// </summary>
		/// <typeparam name="T">Service</typeparam>
		/// <param name="parameters">Optional parameters</param>
		/// <returns>The (mocked) service.</returns>
		public T Create<T>(params Parameter[] parameters)
		{
			if (!Container.IsRegistered<T>())
			{
				var builder = new ContainerBuilder();

				builder.Register<T>().ContainerScoped();
				builder.Build(this.Container);
			}
		
			return this.Container.Resolve<T>(parameters);
		}
	}
}