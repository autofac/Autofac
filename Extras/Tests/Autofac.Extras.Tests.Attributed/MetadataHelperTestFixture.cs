using System.Linq;
using Autofac.Extras.Attributed;
using Autofac.Extras.Tests.Attributed.ScenarioTypes;
using NUnit.Framework;

namespace Autofac.Extras.Tests.Attributed
{
    [TestFixture]
    public class MetadataHelperTestFixture
    {
        [Test]
        public void scan_multiple_attributes_into_one_enumerable_set()
        {
            var metadata = MetadataHelper.GetMetadata(typeof (CombinationalWeakTypedScenario));

            Assert.That(metadata.Count(), Is.EqualTo(2));
            Assert.That(metadata.Where(p => p.Key == "Name").FirstOrDefault().Value, Is.EqualTo("Hello"));
            Assert.That(metadata.Where(p => p.Key == "Age").FirstOrDefault().Value, Is.EqualTo(42));
        }

        [Test]
        public void scan_single_attribute_into_an_enumerable_set()
        {
            var metadata = MetadataHelper.GetMetadata(typeof (WeakTypedScenario));

            Assert.That(metadata.Count(), Is.EqualTo(1));
            Assert.That(metadata.Where(p => p.Key == "Name").FirstOrDefault().Value, Is.EqualTo("Hello"));
        }

        [Test]
        public void scan_strongly_typed_attribute_into_an_enumerable_set()
        {
            var metadata = MetadataHelper.GetMetadata<IStrongTypedScenarioMetadata>(typeof (StrongTypedScenario));

            Assert.That(metadata.Count(), Is.EqualTo(2));
            Assert.That(metadata.Where(p => p.Key == "Name").FirstOrDefault().Value, Is.EqualTo("Hello"));
            Assert.That(metadata.Where(p => p.Key == "Age").FirstOrDefault().Value, Is.EqualTo(42));
        }

        [Test]
        public void verify_that_unfound_strong_typed_attribute_results_in_empty_property_set()
        {
            var metadata = MetadataHelper.GetMetadata<IMetadataModuleScenarioMetadata>(typeof (MetadataModuleScenario));
            
            Assert.That(metadata.Count(), Is.EqualTo(0));
        }

        [Test]
        public void verify_that_unfound_weakly_typed_attribute_results_in_empty_property_set()
        {
            var metadata = MetadataHelper.GetMetadata(typeof(MetadataModuleScenario));

            Assert.That(metadata.Count(), Is.EqualTo(0));
        }

    }
}
