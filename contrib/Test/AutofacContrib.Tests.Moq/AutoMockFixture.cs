using AutofacContrib.Moq;
using Moq;
using NUnit.Framework;

namespace AutofacContrib.Tests.Moq
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

        public class ServiceA : IServiceA
        {
            public void RunA() { }
        }

        public sealed class TestComponent
        {
            private readonly IServiceA _serviceA;
            private readonly IServiceB _serviceB;

            public TestComponent(IServiceA serviceA, IServiceB serviceB)
            {
                this._serviceA = serviceA;
                this._serviceB = serviceB;
            }

            public void RunAll()
            {
                this._serviceA.RunA();
                this._serviceB.RunB();
            }
        }
        #endregion

        /// <summary>
        /// Defaults the constructor is loose.
        /// </summary>
        [Test]
        public void DefaultConstructorIsLoose()
        {
            using (var mock = AutoMock.GetLoose())
            {
                RunWithSingleExpectationTest(mock);
            }
        }

        [Test]
        public void ProvideInstance()
        {
            using (var mock = AutoMock.GetLoose())
            {
                var mockA = new Mock<IServiceA>();
                mockA.Expect(x => x.RunA());
                mock.Provide(mockA.Object);

                var component = mock.Create<TestComponent>();
                component.RunAll();

                mockA.VerifyAll();
            }
        }

        [Test]
        public void ProvideImplementation()
        {
            using (var mock = AutoMock.GetLoose())
            {
                var serviceA = mock.Provide<IServiceA, ServiceA>();

                Assert.IsNotNull(serviceA);
                Assert.IsFalse(serviceA is IMocked<IServiceA>);
            }
        }

        [Test]
        public void DefaultConstructorWorksWithAllTests()
        {
            using (var mock = AutoMock.GetLoose())
            {
                RunTest(mock);
            }
        }

        [Test]
        [ExpectedException(typeof(MockException))]
        public void UnmetExpectationWithStrictMocksThrowsException()
        {
            using (var mock = AutoMock.GetStrict())
            {
                RunWithSingleExpectationTest(mock);
            }
        }

        [Test]
        public void LooseWorksWithUnmetExpectations()
        {
            using (var loose = AutoMock.GetLoose())
            {
                RunWithSingleExpectationTest(loose);
            }
        }

        [Test]
        public void StrictWorksWithAllExpectationsMet()
        {
            using (var strict = AutoMock.GetStrict())
            {
                RunTest(strict);
            }
        }

        [Test]
        public void NormalExpectationsAreNotVerifiedByDefault()
        {
            using (var mock = AutoMock.GetLoose())
            {
                SetUpExpectations(mock);
            }
        }

        [Test]
        [ExpectedException(typeof(MockException))]
        public void UnmetVerifiableExpectationsCauseExceptionByDefault()
        {
            using (var mock = AutoMock.GetLoose())
            {
                SetUpVerifableExpectations(mock);
            }
        }

        [Test]
        public void VerifyAllSetTrue_ExpectationsAreVerified()
        {
            using (var mock = AutoMock.GetLoose())
            {
                mock.VerifyAll = true;
                RunTest(mock);
            }
        }

        [Test]
        [ExpectedException(typeof(MockException))]
        public void VerifyAllSetTrue_UnmetExpectationsCauseException()
        {
            using (var mock = AutoMock.GetLoose())
            {
                mock.VerifyAll = true;
                SetUpExpectations(mock);
            }
        }

        [Test]
        public void ProperInitializationIsPerformed()
        {
            AssertProperties(AutoMock.GetLoose());
            AssertProperties(AutoMock.GetStrict());
        }

        private static void AssertProperties(AutoMock mock)
        {
            Assert.IsNotNull(mock.Container);
            Assert.IsNotNull(mock.MockFactory);
        }

        private static void RunTest(AutoMock mock)
        {
            SetUpExpectations(mock);

            var component = mock.Create<TestComponent>();
            component.RunAll();
        }

        private static void SetUpExpectations(AutoMock mock)
        {
            mock.Mock<IServiceB>().Expect(x => x.RunB());
            mock.Mock<IServiceA>().Expect(x => x.RunA());
        }

        private static void SetUpVerifableExpectations(AutoMock mock)
        {
            mock.Mock<IServiceB>().Expect(x => x.RunB()).Verifiable();
            mock.Mock<IServiceA>().Expect(x => x.RunA()).Verifiable();
        }

        private static void RunWithSingleExpectationTest(AutoMock mock)
        {
            mock.Mock<IServiceB>().Expect(x => x.RunB());

            var component = mock.Create<TestComponent>();
            component.RunAll();
        }
    }
}
