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

        #region local discovery related test types

        [MetadataAttribute]
        public class FooMetadataAttribute : Attribute
        {
            public string Name { get; private set; }

            public FooMetadataAttribute(string name)
            {
                Name = name;
            }
        }

        
        
        public interface IFooMetadata
        {
            string Name { get; }
        }

        public interface IFoo
        {}

        [FooMetadata("Hello")]
        [Export(typeof(IFoo))]
        public class Foo : IFoo{}

        // here we have another export type that declares our metadata in a slightly different fashion



        #endregion

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
        public void verify_ability_to_discover_local_types()
        {
            // arrange
            var aggregateCatalog = new AggregateCatalog(new AssemblyCatalog(Assembly.GetExecutingAssembly()));
            var container = new ExportInfoCompositionContainer(aggregateCatalog);

            // act
            var exports = container.GetExportsWithTargetType<IFoo, IFooMetadata>();

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
            var exports = container.GetExportsWithTargetType<IFoo, IFooMetadata>();

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
            var exports = container.GetExportsWithTargetType<IFoo, IFooMetadata>();

            // assert
            Assert.That(exports.ToArray()[0].InstantiationType, Is.EqualTo(typeof(Foo)));
        }

        [Test]
        public void validate_lazy_instantiation_of_target_instance()
        {
            // arrange
            var aggregateCatalog = new AggregateCatalog(new AssemblyCatalog(Assembly.GetExecutingAssembly()));
            var container = new ExportInfoCompositionContainer(aggregateCatalog);

            // act
            var exports = container.GetExportsWithTargetType<IFoo, IFooMetadata>();

            // assert
            Assert.That(exports.ToArray()[0].Value.Value, Is.TypeOf<Foo>());
        }
    }
}