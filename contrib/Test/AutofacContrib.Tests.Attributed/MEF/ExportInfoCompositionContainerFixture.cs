using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Reflection;
using AutofacContrib.Attributed.MEF;
using AutofacContrib.Tests.Attributed.ScenarioTypes;
using NUnit.Framework;

namespace AutofacContrib.Tests.Attributed.MEF
{
    [TestFixture]
    public class ExportInfoCompositionContainerFixture
    {
        [Test]
        public void verify_scenario2_ability_to_discover_local_types()
        {
            var aggregateCatalog = new AggregateCatalog(new AssemblyCatalog(Assembly.GetExecutingAssembly()));
            var container = new ExportInfoCompositionContainer(aggregateCatalog);

            // act
            var exports = container.GetExportsWithTargetType<IExportScenario2, IExportScenario2Metadata>();

            // assert
            Assert.That(exports.Count(), Is.EqualTo(1));
        }

        [Test]
        public void validate_scenario2_discovery_of_the_metadata()
        {
            // arrange
            var aggregateCatalog = new AggregateCatalog(new AssemblyCatalog(Assembly.GetExecutingAssembly()));
            var container = new ExportInfoCompositionContainer(aggregateCatalog);

            // act
            var exports = container.GetExportsWithTargetType<IExportScenario2, IExportScenario2Metadata>();

            // assert
            Assert.That(exports.ToArray()[0].Value.Metadata.Name, Is.EqualTo("Hello2"));
        }

        [Test]
        public void validate_scenario2_concrete_type_identification_of_discovery_process()
        {
            // arrange
            var aggregateCatalog = new AggregateCatalog(new AssemblyCatalog(Assembly.GetExecutingAssembly()));
            var container = new ExportInfoCompositionContainer(aggregateCatalog);

            // act
            var exports = container.GetExportsWithTargetType<IExportScenario2, IExportScenario2Metadata>();

            // assert
            Assert.That(exports.ToArray()[0].InstantiationType, Is.EqualTo(typeof(ExportScenario2)));
        }

        [Test]
        public void validate_scenario2_lazy_instantiation_of_target_instance()
        {
            // arrange
            var aggregateCatalog = new AggregateCatalog(new AssemblyCatalog(Assembly.GetExecutingAssembly()));
            var container = new ExportInfoCompositionContainer(aggregateCatalog);

            // act
            var exports = container.GetExportsWithTargetType<IExportScenario2, IExportScenario2Metadata>();

            // assert
            Assert.That(exports.ToArray()[0].Value.Value, Is.TypeOf<ExportScenario2>());
        }


        [Test]
        public void verify_ability_to_discover_local_types()
        {
            // arrange
            var aggregateCatalog = new AggregateCatalog(new AssemblyCatalog(Assembly.GetExecutingAssembly()));
            var container = new ExportInfoCompositionContainer(aggregateCatalog);

            // act
            var exports = container.GetExportsWithTargetType<IExportScenario1, IExportScenario1Metadata>();

            var array = exports.ToArray();
            // assert
            Assert.That(array.Count(), Is.EqualTo(1));
        }

        [Test]
        public void validate_discovery_of_the_metadata()
        {
            // arrange
            var aggregateCatalog = new AggregateCatalog(new AssemblyCatalog(Assembly.GetExecutingAssembly()));
            var container = new ExportInfoCompositionContainer(aggregateCatalog);

            // act
            var exports = container.GetExportsWithTargetType<IExportScenario1, IExportScenario1Metadata>();

            // assert
            Assert.That(exports.ToArray()[0].Value.Metadata.Name, Is.EqualTo("Hello"));
        }

        [Test]
        public void validate_concrete_type_identification_of_discovery_process()
        {
            // arrange
            var aggregateCatalog = new AggregateCatalog(new AssemblyCatalog(Assembly.GetExecutingAssembly()));
            var container = new ExportInfoCompositionContainer(aggregateCatalog);

            // act
            var exports = container.GetExportsWithTargetType<IExportScenario1, IExportScenario1Metadata>();

            // assert
            Assert.That(exports.ToArray()[0].InstantiationType, Is.EqualTo(typeof(ExportScenario1)));
        }

        [Test]
        public void validate_lazy_instantiation_of_target_instance()
        {
            // arrange
            var aggregateCatalog = new AggregateCatalog(new AssemblyCatalog(Assembly.GetExecutingAssembly()));
            var container = new ExportInfoCompositionContainer(aggregateCatalog);

            // act
            var exports = container.GetExportsWithTargetType<IExportScenario1, IExportScenario1Metadata>();

            // assert
            Assert.That(exports.ToArray()[0].Value.Value, Is.TypeOf<ExportScenario1>());
        }
    }
}