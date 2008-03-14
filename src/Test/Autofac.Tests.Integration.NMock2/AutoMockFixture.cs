using NMock2;
using NMock2.Internal;
using NUnit.Framework;

namespace Autofac.Tests.Integration.NMock2
{
	[TestFixture]
	public sealed class AutoMockFixture
	{
		#region stubs

		public interface IServiceA
		{
			void RunA();
		}
		public interface IServiceB
		{
			void RunB();
		}

		public sealed class TestComponent
		{

			private readonly IServiceA _serviceA;
			private readonly IServiceB _serviceB;

			public TestComponent(IServiceA serviceA, IServiceB serviceB)
			{
				_serviceA = serviceA;
				_serviceB = serviceB;
			}

			public void RunAll()
			{
				_serviceA.RunA();
				_serviceB.RunB();
			}
		}

		#endregion

		[Test]
		public void DefaultConstructorIsUnordered()
		{
			using (var mock = new AutoMock())
			{
				var component = mock.Create<TestComponent>();

				Expect.Once.On(mock.Resolve<IServiceB>()).Method("RunB");
				Expect.Once.On(mock.Resolve<IServiceA>()).Method("RunA");

				component.RunAll();
			}
		}

		[Test]
		[ExpectedException(typeof(ExpectationException))]
		public void UnorderedThrowsException()
		{
			using (var mock = AutoMock.GetOrdered())
			{
				var component = mock.Create<TestComponent>();

				Expect.Once.On(mock.Resolve<IServiceB>()).Method("RunB");
				Expect.Once.On(mock.Resolve<IServiceA>()).Method("RunA");

				component.RunAll();
			}
		}
	}
}
