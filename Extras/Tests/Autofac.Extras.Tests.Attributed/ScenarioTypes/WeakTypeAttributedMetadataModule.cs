using Autofac;
using Autofac.Extras.Attributed;
using Autofac.Integration.Mef;

namespace Autofac.Extras.Tests.Attributed.ScenarioTypes
{
    public class WeakTypeAttributedMetadataModule : AttributedMetadataModule 
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterMetadataRegistrationSources();
            builder.RegisterType<WeakTypedScenario>().As<IWeakTypedScenario>();
            builder.RegisterType<CombinationalWeakTypedScenario>().As<ICombinationalWeakTypedScenario>();
        }
    }
}
