using Autofac.Extras.Attributed;

namespace Autofac.Extras.Tests.Attributed.ScenarioTypes
{
    public class TypeOfScenarioMetadataModule : MetadataModule<IMetadataModuleScenario, IMetadataModuleScenarioMetadata>
    {
        public override void Register(IMetadataRegistrar<IMetadataModuleScenario, IMetadataModuleScenarioMetadata> registrar)
        {
            registrar.RegisterType(typeof(MetadataModuleScenario), new MetadataModuleScenarioMetadata("sid"));
            registrar.RegisterType(typeof(MetadataModuleScenario), new MetadataModuleScenarioMetadata("nancy"));

            // in addition, we'll register an additional metadata variant of the alternate scenario 4 type
            registrar.RegisterType(typeof(MetadataModuleScenarioAlternate), new MetadataModuleScenarioMetadata("the-cats"));
        }


    }
}
