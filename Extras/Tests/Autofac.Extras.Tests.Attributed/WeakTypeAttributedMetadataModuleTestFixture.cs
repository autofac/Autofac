using System;
using Autofac;
using Autofac.Extras.Tests.Attributed.ScenarioTypes;
using NUnit.Framework;

namespace Autofac.Extras.Tests.Attributed
{
    [TestFixture]
    public class WeakTypeAttributedMetadataModuleTestFixture
    {
        [Test]
        public void verify_automatic_scanning_with_the_attributed_metadata_module()
        {
            var builder = new ContainerBuilder();
            builder.RegisterModule(new WeakTypeAttributedMetadataModule());

            var container = builder.Build();

            var weakTyped = container.Resolve < Lazy<IWeakTypedScenario, IWeakTypedScenarioMetadata>>();

            Assert.That(weakTyped.Metadata.Name, Is.EqualTo("Hello"));
        }

        [Test]
        public void verify_automatic_scanning_with_the_multiple_attributions_by_the_module()
        {
            var builder = new ContainerBuilder();
            builder.RegisterModule(new WeakTypeAttributedMetadataModule());

            var container = builder.Build();

            var weakTyped = container.Resolve<Lazy<ICombinationalWeakTypedScenario, ICombinationalWeakTypedScenarioMetadata>>();

            Assert.That(weakTyped.Metadata.Name, Is.EqualTo("Hello"));
            Assert.That(weakTyped.Metadata.Age, Is.EqualTo(42));
        }

    }
}
