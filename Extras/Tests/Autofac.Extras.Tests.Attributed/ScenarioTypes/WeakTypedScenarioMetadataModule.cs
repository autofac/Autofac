using AutofacContrib.Attributed;

namespace AutofacContrib.Tests.Attributed.ScenarioTypes
{
    /// <summary>
    /// this class demonstratees the ability to search for metadata based on metadata attributes instead of
    /// providing the metadata directly
    /// </summary>
    public class WeakTypedScenarioMetadataModule : MetadataModule<IWeakTypedScenario, IWeakTypedScenarioMetadata>
    {
        readonly bool _useGeneric;

        public WeakTypedScenarioMetadataModule(bool useGeneric)
        {
            _useGeneric = useGeneric;
        }
        public override void Register(IMetadataRegistrar<IWeakTypedScenario, IWeakTypedScenarioMetadata> registrar)
        {
            if (_useGeneric)
            {
                registrar.RegisterAttributedType<WeakTypedScenario>();
                return;
            }

            registrar.RegisterAttributedType(typeof(WeakTypedScenario));
        }
    }
}
