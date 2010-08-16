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
            // first we'll register two metadata variants of export scenario 4
            registrar.RegisterTypedMetadata<ExportScenario4>(new[]
                                                                 {
                                                                     new ExportScenario4Metadata("sid"),
                                                                     new ExportScenario4Metadata("nancy")
         
                                                                 });

            // in addition, we'll register an additional metadata variant of the alternate scenario 4 type
            registrar.RegisterTypedMetadata<ExportScenario4Alternate>(new[] {new ExportScenario4Metadata("the-cats")});
        }
    }
}
