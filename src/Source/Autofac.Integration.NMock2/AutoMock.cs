using System;
using Autofac;
using Autofac.Builder;
using NMock2;

namespace NMock2
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
		/// <returns></returns>
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