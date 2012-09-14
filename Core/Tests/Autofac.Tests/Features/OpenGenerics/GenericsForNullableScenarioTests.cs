using Autofac.Tests.Util;
using NUnit.Framework;

namespace Autofac.Tests.Features.OpenGenerics
{
    public interface IItemProducer<T> { }

    public class NullableProducer<T> : IItemProducer<T?>
        where T : struct
    {      
    }

    [TestFixture]
    public class GenericsForNullableScenarioTests
    {
        IContainer _container;

        [SetUp]
        public void SetUp()
        {
            var builder = new ContainerBuilder();
            builder.RegisterGeneric(typeof(NullableProducer<>)).As(typeof(IItemProducer<>));
            _container = builder.Build();
        }

        [Test]
        public void TheServiceIsAvailable()
        {
            Assert.IsTrue(_container.IsRegistered<IItemProducer<byte?>>());
        }

        [Test]
        public void TheImplementationTypeParametersAreMapped()
        {
            var np = _container.Resolve<IItemProducer<byte?>>();
            Assert.IsInstanceOf<NullableProducer<byte>>(np);
        }

        [Test]
        public void IncompatibleTypeParametersAreIgnored()
        {
            Assert.IsFalse(_container.IsRegistered<IItemProducer<byte>>());
        }
    }
}
