using AutofacContrib.Attributed;

namespace AutofacContrib.Tests.Attributed.ScenarioTypes
{
    /// <summary>
    /// this class demonstratees the ability to search for metadata based on metadata attributes instead of
    /// providing the metadata directly
    /// </summary>
    public class WeakTypedScenarioMetadataModule : MetadataModule<IWeakTypedScenario, IWeakTypedScenarioMetadata>
    {
        public override void Register(IMetadataRegistrar<IWeakTypedScenario, IWeakTypedScenarioMetadata> registrar)
        {
            registrar.RegisterAttributedType<WeakTypedScenario>();
        }
    }
}
