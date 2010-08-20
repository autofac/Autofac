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
    public class WeakTypedAttributeScenarioTestFixture
    {
        [Test]
        public void validate_wireup_of_generic_attributes_to_strongly_typed_metadata_on_resolve()
        {
            // arrange
            var builder = new ContainerBuilder();

            builder.RegisterAssemblyTypes(Assembly.GetExecutingAssembly())
                .AssignableTo(typeof(IWeakTypedScenario))
                .As<IWeakTypedScenario>()
                .WithAttributedMetadata();

            // act
            var items = builder.Build().Resolve<IEnumerable<Lazy<IWeakTypedScenario, IWeakTypedScenarioMetadata>>>();

            // assert
            Assert.That(items.Count(), Is.EqualTo(1));
            Assert.That(items.Where(p => p.Metadata.Name == "Hello").Count(), Is.EqualTo(1));
        }
    }
}
