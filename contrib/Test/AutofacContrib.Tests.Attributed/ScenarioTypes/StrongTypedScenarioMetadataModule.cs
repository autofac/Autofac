using AutofacContrib.Attributed;

namespace AutofacContrib.Tests.Attributed.ScenarioTypes
{
    /// <summary>
    /// This class demonstrates programmatic or non-attribute based discovery of metadata types.  This could also be
    /// used to provide variable, non-compile time wireup to the registration of objects. 
    /// </summary>
    public class StrongTypedScenarioMetadataModule : MetadataModule<IMetadataModuleScenario,IMetadataModuleScenarioMetadata>
    {
        public override void Register(IMetadataRegistrar<IMetadataModuleScenario, IMetadataModuleScenarioMetadata> registrar)
        {
            registrar.RegisterType<MetadataModuleScenario>(new MetadataModuleScenarioMetadata("sid"));
            registrar.RegisterType<MetadataModuleScenario>(new MetadataModuleScenarioMetadata("nancy"));

            // in addition, we'll register an additional metadata variant of the alternate scenario 4 type
            registrar.RegisterType<MetadataModuleScenarioAlternate>(new MetadataModuleScenarioMetadata("the-cats"));
        }
    }
}
