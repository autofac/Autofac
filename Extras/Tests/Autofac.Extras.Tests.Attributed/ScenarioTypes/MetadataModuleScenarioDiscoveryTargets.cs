
namespace Autofac.Extras.Tests.Attributed.ScenarioTypes
{
    public class MetadataModuleScenario : IMetadataModuleScenario
    {
    }

    public class MetadataModuleScenarioAlternate : IMetadataModuleScenario
    {
    }

    public class MetadataModuleScenarioMetadata : IMetadataModuleScenarioMetadata
    {
        public MetadataModuleScenarioMetadata(string name)
        {
            Name = name;
        }

        #region IExportScenario4Metadata Members

        public string Name { get; private set;} 

        #endregion
    }

}
