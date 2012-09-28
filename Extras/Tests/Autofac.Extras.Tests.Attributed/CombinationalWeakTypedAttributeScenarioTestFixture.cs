using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Autofac;
using Autofac.Extras.Attributed;
using Autofac.Extras.Tests.Attributed.ScenarioTypes;
using NUnit.Framework;

namespace Autofac.Extras.Tests.Attributed
{
    [TestFixture]
    public class CombinationalWeakTypedAttributeScenarioTestFixture
    {
        /// <summary>
        /// This is a test that demonstrates the ability to combine multiple weak-typed attributes to 
        /// constitute a single strongly-typed metadata instance resolved from the container
        /// </summary>
        [Test]
        public void validate_wireup_of_generic_attributes_to_strongly_typed_metadata_on_resolve()
        {
            // arrange
            var builder = ContainerBuilderFactory.Create();

            builder.RegisterAssemblyTypes(Assembly.GetExecutingAssembly())
                .As<ICombinationalWeakTypedScenario>()
                .WithAttributedMetadata();

            // act
            var items = builder.Build().Resolve<IEnumerable<Lazy<ICombinationalWeakTypedScenario, ICombinationalWeakTypedScenarioMetadata>>>();

            // assert
            Assert.That(items.Count(), Is.EqualTo(1));
            Assert.That(items.Where(p => p.Metadata.Name == "Hello").Count(), Is.EqualTo(1));
            Assert.That(items.Where(p => p.Metadata.Age == 42).Count(), Is.EqualTo(1));
        }
    }
}
