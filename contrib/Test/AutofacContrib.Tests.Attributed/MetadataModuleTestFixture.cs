using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using AutofacContrib.Tests.Attributed.ScenarioTypes;
using NUnit.Framework;

namespace AutofacContrib.Tests.Attributed
{
    [TestFixture] 
    public class MetadataModuleTestFixture
    {

        [Test]
        public void scenario_4_validate_registration_content()
        {
            // arrange
            var builder = new ContainerBuilder();

            builder.RegisterModule(new Scenario4MetadataModule());

            // act
            var items = builder.Build().Resolve<IEnumerable<Lazy<IExportScenario4, IExportScenario4Metadata>>>();

            // assert
            Assert.That(items.Where(p => p.Metadata.Name == "sid").Count(), Is.EqualTo(1));
            Assert.That(items.Where(p => p.Metadata.Name == "nancy").Count(), Is.EqualTo(1));
            Assert.That(items.Where(p => p.Metadata.Name == "the-cats").Count(), Is.EqualTo(1));

            // the following was not registered
            Assert.That(items.Where(p => p.Metadata.Name == "the-dogs").Count(), Is.EqualTo(0));
        }
    }
}