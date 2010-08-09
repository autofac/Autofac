using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Autofac;
using Autofac.Features.Metadata;
using AutofacContrib.Attributed;
using AutofacContrib.Tests.Attributed.ScenarioTypes;
using NUnit.Framework;

namespace AutofacContrib.Tests.Attributed
{
    [TestFixture] 
    public class ContainerBuilderExtensionTestFixture
    {
        [Test]
        public void container_wireup_scenario_1_test()
        {
            // arrange
            var builder = new ContainerBuilder();

            builder.RegisterUsingMetadataAttributes<IExportScenario1, IExportScenario1Metadata>(
                Assembly.GetExecutingAssembly());

            // act
            var items =builder.Build().Resolve < IEnumerable<Lazy<IExportScenario1, IExportScenario1Metadata>>>();

            // assert
            Assert.That(items.Count(), Is.EqualTo(1));


        }

        [Test]
        public void container_wireup_scenario_2_test()
        {
            // arrange
            var builder = new ContainerBuilder();

            builder.RegisterUsingMetadataAttributes<IExportScenario2, IExportScenario2Metadata>(Assembly.GetExecutingAssembly());

            // act
            var items = builder.Build().Resolve<IEnumerable<Lazy<IExportScenario2, IExportScenario2Metadata>>>();

            // assert
            Assert.That(items.Count(), Is.EqualTo(2));
        }

        [Test]
        public void meta_wireup_on_scenario_2_test()
        {
            // arrange
            var builder = new ContainerBuilder();

            builder.RegisterUsingMetadataAttributes<IExportScenario2, IExportScenario2Metadata>(Assembly.GetExecutingAssembly());

            // act
            var items = builder.Build().Resolve<IEnumerable<Meta<IExportScenario2, IExportScenario2Metadata>>>();

            // assert
            Assert.That(items.Count(), Is.EqualTo(2));
        }

        [Test]
        public void inclusion_predicate_on_wireup_scenario_2()
        {
            // arrange
            var builder = new ContainerBuilder();

            builder.RegisterUsingMetadataAttributes<IExportScenario2, IExportScenario2Metadata>(p => p.Name != "Hello2",
                Assembly.GetExecutingAssembly());

            // act
            var items = builder.Build().Resolve<IEnumerable<Lazy<IExportScenario2, IExportScenario2Metadata>>>();

            // assert
            Assert.That(items.Count(), Is.EqualTo(1));
            Assert.That(items.ToArray()[0].Metadata.Name, Is.EqualTo("scenario2"));
        }

        [Test]
        public void properly_handle_passing_a_null_predicate()
        {
            // arrange
            var builder = new ContainerBuilder();
            Predicate<IExportScenario2Metadata> predicate = null;

            // act
            var details = Assert.Throws<ArgumentNullException>(() =>
                                                               builder.RegisterUsingMetadataAttributes
                                                                   <IExportScenario2, IExportScenario2Metadata>(
                                                                       predicate,
                                                                       Assembly.
                                                                           GetExecutingAssembly
                                                                           ()));

            // assert
            Assert.That(details.ParamName, Is.EqualTo("inclusionPredicate"));
        }
    }
}