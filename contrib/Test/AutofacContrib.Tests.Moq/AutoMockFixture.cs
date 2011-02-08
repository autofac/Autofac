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
                RunWithSingleSetupationTest(mock);
            }
        }

        [Test]
        public void GetFromRepositoryUsesLooseBehaviorSetOnRepository()
        {
            using (var mock = AutoMock.GetFromRepository(new MockRepository(MockBehavior.Loose)))
            {
                RunWithSingleSetupationTest(mock);
            }
        }

        [Test]
        [ExpectedException(typeof(MockException))]
        public void GetFromRepositoryUsesStrictBehaviorSetOnRepository()
        {
            using (var mock = AutoMock.GetFromRepository(new MockRepository(MockBehavior.Strict)))
            {
                RunWithSingleSetupationTest(mock);
            }
        }


        [Test]
        public void ProvideInstance()
        {
            using (var mock = AutoMock.GetLoose())
            {
                var mockA = new Mock<IServiceA>();
                mockA.Setup(x => x.RunA());
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
        public void UnmetSetupationWithStrictMocksThrowsException()
        {
            using (var mock = AutoMock.GetStrict())
            {
                RunWithSingleSetupationTest(mock);
            }
        }

        [Test]
        public void LooseWorksWithUnmetSetupations()
        {
            using (var loose = AutoMock.GetLoose())
            {
                RunWithSingleSetupationTest(loose);
            }
        }

        [Test]
        public void StrictWorksWithAllSetupationsMet()
        {
            using (var strict = AutoMock.GetStrict())
            {
                RunTest(strict);
            }
        }

        [Test]
        public void NormalSetupationsAreNotVerifiedByDefault()
        {
            using (var mock = AutoMock.GetLoose())
            {
                SetUpSetupations(mock);
            }
        }

        [Test]
        [ExpectedException(typeof(MockException))]
        public void UnmetVerifiableSetupationsCauseExceptionByDefault()
        {
            using (var mock = AutoMock.GetLoose())
            {
                SetUpVerifableSetupations(mock);
            }
        }

        [Test]
        public void VerifyAllSetTrue_SetupationsAreVerified()
        {
            using (var mock = AutoMock.GetLoose())
            {
                mock.VerifyAll = true;
                RunTest(mock);
            }
        }

        [Test]
        [ExpectedException(typeof(MockException))]
        public void VerifyAllSetTrue_UnmetSetupationsCauseException()
        {
            using (var mock = AutoMock.GetLoose())
            {
                mock.VerifyAll = true;
                SetUpSetupations(mock);
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
            SetUpSetupations(mock);

            var component = mock.Create<TestComponent>();
            component.RunAll();
        }

        private static void SetUpSetupations(AutoMock mock)
        {
            mock.Mock<IServiceB>().Setup(x => x.RunB());
            mock.Mock<IServiceA>().Setup(x => x.RunA());
        }

        private static void SetUpVerifableSetupations(AutoMock mock)
        {
            mock.Mock<IServiceB>().Setup(x => x.RunB()).Verifiable();
            mock.Mock<IServiceA>().Setup(x => x.RunA()).Verifiable();
        }

        private static void RunWithSingleSetupationTest(AutoMock mock)
        {
            mock.Mock<IServiceB>().Setup(x => x.RunB());

            var component = mock.Create<TestComponent>();
            component.RunAll();
        }
    }
}
