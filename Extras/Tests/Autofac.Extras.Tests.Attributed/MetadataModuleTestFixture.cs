using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using Autofac.Extras.Tests.Attributed.ScenarioTypes;
using NUnit.Framework;

namespace Autofac.Extras.Tests.Attributed
{
    [TestFixture] 
    public class MetadataModuleTestFixture
    {

        [Test]
        public void metadata_module_scenario_validate_registration_content()
        {
            // arrange
            var builder = ContainerBuilderFactory.Create();

            builder.RegisterModule(new StrongTypedScenarioMetadataModule());

            // act
            var items = builder.Build().Resolve<IEnumerable<Lazy<IMetadataModuleScenario, IMetadataModuleScenarioMetadata>>>();

            // assert
            Assert.That(items.Where(p => p.Metadata.Name == "sid").Count(), Is.EqualTo(1));
            Assert.That(items.Where(p => p.Metadata.Name == "nancy").Count(), Is.EqualTo(1));
            Assert.That(items.Where(p => p.Metadata.Name == "the-cats").Count(), Is.EqualTo(1));

            // the following was not registered
            Assert.That(items.Where(p => p.Metadata.Name == "the-dogs").Count(), Is.EqualTo(0));
        }

        [Test]
        public void metadata_module_scenario_using_typeof_registration()
        {
            // arrange
            var builder = ContainerBuilderFactory.Create();

            builder.RegisterModule(new TypeOfScenarioMetadataModule());

            // act
            var items = builder.Build().Resolve<IEnumerable<Lazy<IMetadataModuleScenario, IMetadataModuleScenarioMetadata>>>();

            // assert
            Assert.That(items.Where(p => p.Metadata.Name == "sid").Count(), Is.EqualTo(1));
            Assert.That(items.Where(p => p.Metadata.Name == "nancy").Count(), Is.EqualTo(1));
            Assert.That(items.Where(p => p.Metadata.Name == "the-cats").Count(), Is.EqualTo(1));

            // the following was not registered
            Assert.That(items.Where(p => p.Metadata.Name == "the-dogs").Count(), Is.EqualTo(0));

        }
    }
}