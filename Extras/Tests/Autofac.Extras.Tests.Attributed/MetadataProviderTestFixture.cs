using Autofac.Extras.Attributed;
using Autofac.Extras.Tests.Attributed.ScenarioTypes;
using Autofac.Features.Metadata;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Autofac.Extras.Tests.Attributed
{
    [TestFixture]
    public class MetadataProviderTestFixture
    {
        [Test]
        public void load_provided_into_weak_metadata()
        {
            var builder = new ContainerBuilder();
            builder.RegisterModule<AttributedMetadataModule>();
            builder.RegisterType<MetadataProviderScenario>()
                .As<IMetadataProviderScenario>();

            var container = builder.Build();
            var withMetadata = container.Resolve<Meta<IMetadataProviderScenario>>();

            Assert.That(withMetadata, Is.Not.Null);
            var value1 = withMetadata.Metadata.Where(kv => kv.Key == "Key1").FirstOrDefault();
            var value2 = withMetadata.Metadata.Where(kv => kv.Key == "Key2").FirstOrDefault();

            Assert.That(value1, Is.Not.Null);
            Assert.That(value2, Is.Not.Null);
            Assert.That(value1.Value, Is.EqualTo("Value1"));
            Assert.That(value2.Value, Is.EqualTo("Value2"));
        }

        public void load_provided_into_strong_metadata()
        {
            var builder = new ContainerBuilder();
            builder.RegisterModule<AttributedMetadataModule>();
            builder.RegisterType<MetadataProviderScenario>()
                .As<IMetadataProviderScenario>();

            var container = builder.Build();
            var withMetadata = container.Resolve<Meta<IMetadataProviderScenario, ProvidedMetadata>>();

            Assert.That(withMetadata, Is.Not.Null);
            Assert.That(withMetadata.Metadata, Is.Not.Null);
            Assert.That(withMetadata.Metadata.Key1, Is.EqualTo("Value1"));
            Assert.That(withMetadata.Metadata.Key2, Is.EqualTo("Value2"));
        }
    }
}
