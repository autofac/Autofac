using Autofac;
using AutofacContrib.AggregateService;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;

namespace AutofacContrib.Tests.AggregateService
{
    [TestFixture]
    public class AggregateServiceGeneratorFixture
    {
        [Test]
        public void CreateInstance_WithGenericInterface_CreatesInstanceOfInterface()
        {
            var instance = AggregateServiceGenerator.CreateInstance<IMyContext>(null);

            Assert.That(instance, Is.InstanceOfType(typeof(IMyContext)));
        }

        [Test]
        public void CreateInstance_WithInterfaceType_CreatesInstanceOfInterface()
        {
            var instance = AggregateServiceGenerator.CreateInstance(typeof(IMyContext), null);

            Assert.That(instance, Is.InstanceOfType(typeof(IMyContext)));
        }
    }
}