using AutofacContrib.NMock2;
using NMock2;
using NMock2.Internal;
using NUnit.Framework;

namespace AutofacContrib.Tests.NMock2
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
				RunReversedTest(mock);
			}
		}

		[Test]
		[ExpectedException(typeof (ExpectationException))]
		public void OrderedThrowsExceptionOnReversedTest()
		{
			using (var mock = AutoMock.GetOrdered())
			{
				RunReversedTest(mock);
			}
		}

		[Test]
		public void UnorderedWorksWithReversedTest()
		{
			using (var unordered = AutoMock.GetUnordered())
			{
				RunTest(unordered);
			}
		}

		[Test]
		public void ProperInitializationIsPerformed()
		{
			AssertProperties(new AutoMock());
			AssertProperties(AutoMock.GetOrdered());
			AssertProperties(AutoMock.GetUnordered());
		}

		private static void AssertProperties(AutoMock mock)
		{
			Assert.IsNotNull(mock.Container);
			Assert.IsNotNull(mock.Mockery);
		}

		private static void RunTest(AutoMock mock)
		{
            Expect.Once.On(mock.Resolve<IServiceA>()).Method("RunA");
            Expect.Once.On(mock.Resolve<IServiceB>()).Method("RunB");

			var component = mock.Create<TestComponent>();
			component.RunAll();
		}

		private static void RunReversedTest(AutoMock mock)
		{
			Expect.Once.On(mock.Resolve<IServiceB>()).Method("RunB");
			Expect.Once.On(mock.Resolve<IServiceA>()).Method("RunA");

			var component = mock.Create<TestComponent>();
			component.RunAll();
		}
	}
}