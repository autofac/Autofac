using System;
using System.Collections.Generic;
using System.Linq;
using Autofac.Extras.Tests.Attributed.ScenarioTypes;
using NUnit.Framework;

namespace Autofac.Extras.Tests.Attributed
{
    [TestFixture]
    public class WeakTypedScenarioMetadataModuleTestFixture
    {
        [Test]
        public void verify_single_attribute_scan_results_from_test_fixture()
        {
            // arrange
            var builder = new ContainerBuilder();

            builder.RegisterModule(new WeakTypedScenarioMetadataModule(true));

            // act
            var items = builder.Build().Resolve<IEnumerable<Lazy<IWeakTypedScenario, IWeakTypedScenarioMetadata>>>();

            // assert
            Assert.That(items.Count(), Is.EqualTo(1));
            Assert.That(items.Where(p => p.Metadata.Name == "Hello").Count(), Is.EqualTo(1));
        }

        [Test]
        public void verify_single_attribute_scan_results_from_test_fixture_using_non_generic()
        {
            // arrange
            var builder = new ContainerBuilder();

            builder.RegisterModule(new WeakTypedScenarioMetadataModule(false));

            // act
            var items = builder.Build().Resolve<IEnumerable<Lazy<IWeakTypedScenario, IWeakTypedScenarioMetadata>>>();

            // assert
            Assert.That(items.Count(), Is.EqualTo(1));
            Assert.That(items.Where(p => p.Metadata.Name == "Hello").Count(), Is.EqualTo(1));
        }
    }
}
