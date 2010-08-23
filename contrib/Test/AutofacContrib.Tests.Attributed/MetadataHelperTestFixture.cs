using System.Linq;
using AutofacContrib.Attributed;
using AutofacContrib.Tests.Attributed.ScenarioTypes;
using NUnit.Framework;

namespace AutofacContrib.Tests.Attributed
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
    }
}
