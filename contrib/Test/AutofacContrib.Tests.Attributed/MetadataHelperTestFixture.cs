using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutofacContrib.Attributed;
using AutofacContrib.Tests.Attributed.ScenarioTypes;
using NUnit.Framework;

namespace AutofacContrib.Tests.Attributed
{
    [TestFixture]
    public class MetadataHelperTestFixture
    {
        [Test]
        public void scan_multiple_properties_into_one_enumerable_set()
        {
            var metadata = MetadataHelper.GetMetadata(typeof (CombinationalWeakTypedScenario));

            Assert.That(metadata.Where(p => p.Key == "Name").FirstOrDefault().Value, Is.EqualTo("Hello"));
            Assert.That(metadata.Where(p => p.Key == "Age").FirstOrDefault().Value, Is.EqualTo(42));
        }
    }
}
