using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Autofac.Extras.Attributed;
using Autofac.Extras.Tests.Attributed.ScenarioTypes;
using Autofac.Integration.Mef;
using NUnit.Framework;

namespace Autofac.Extras.Tests.Attributed
{
    [TestFixture]
    public class StrongTypedAttributeScenarioTestFixture
    {
        [Test]
        public void validate_wireup_of_typed_attributes_to_strongly_typed_metadata_on_resolve()
        {
            // arrange
            var builder = new ContainerBuilder();
            builder.RegisterMetadataRegistrationSources();

            builder.RegisterAssemblyTypes(Assembly.GetExecutingAssembly())
                .As<IStrongTypedScenario>()
                .WithAttributedMetadata<IStrongTypedScenarioMetadata>();

            // act
            var items = builder.Build().Resolve<IEnumerable<Lazy<IStrongTypedScenario, IStrongTypedScenarioMetadata>>>();

            // assert
            Assert.That(items.Count(), Is.EqualTo(2));
            Assert.That(items.Where(p => p.Metadata.Name == "Hello" && p.Metadata.Age == 42).Count(), Is.EqualTo(1));
            Assert.That(items.Where(p => p.Metadata.Name == "Goodbye" && p.Metadata.Age == 24).Count(), Is.EqualTo(1));

            Assert.That(items.Where(p => p.Metadata.Name == "Hello" && p.Metadata.Age == 42).First().Value, Is.TypeOf<StrongTypedScenario>());
            Assert.That(items.Where(p => p.Metadata.Name == "Goodbye" && p.Metadata.Age == 24).First().Value, Is.TypeOf<AlternateStrongTypedScenario>());

        }

    }
}
