using AutofacContrib.Attributed;

namespace AutofacContrib.Tests.Attributed.ScenarioTypes
{
    /// <summary>
    /// This class demonstrates programmatic or non-attribute based discovery of metadata types.  This could also be
    /// used to provide variable, non-compile time wireup to the registration of objects. 
    /// </summary>
    public class Scenario4MetadataModule : MetadataModule<IExportScenario4,IExportScenario4Metadata>
    {
        public override void Register(IMetadataRegistrar<IExportScenario4, IExportScenario4Metadata> registrar)
        {
            registrar.RegisterType<ExportScenario4>(new ExportScenario4Metadata("sid"));
            registrar.RegisterType<ExportScenario4>(new ExportScenario4Metadata("nancy"));

            // in addition, we'll register an additional metadata variant of the alternate scenario 4 type
            registrar.RegisterType<ExportScenario4Alternate>(new ExportScenario4Metadata("the-cats"));
        }
    }
}
