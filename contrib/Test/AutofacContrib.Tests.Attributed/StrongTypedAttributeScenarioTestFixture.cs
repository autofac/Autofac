using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Autofac;
using AutofacContrib.Attributed;
using AutofacContrib.Tests.Attributed.ScenarioTypes;
using NUnit.Framework;

namespace AutofacContrib.Tests.Attributed
{
    [TestFixture]
    public class StrongTypedAttributeScenarioTestFixture
    {
        [Test]
        public void validate_wireup_of_typed_attributes_to_strongly_typed_metadata_on_resolve()
        {
            // arrange
            var builder = new ContainerBuilder();

            builder.RegisterAssemblyTypes(Assembly.GetExecutingAssembly())
                .AssignableTo(typeof(IStrongTypedScenario))
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
