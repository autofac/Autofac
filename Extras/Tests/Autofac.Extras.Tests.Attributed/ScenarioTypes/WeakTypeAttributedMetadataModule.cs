using Autofac;
using Autofac.Extras.Attributed;

namespace Autofac.Extras.Tests.Attributed.ScenarioTypes
{
    public class WeakTypeAttributedMetadataModule : AttributedMetadataModule 
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<WeakTypedScenario>().As<IWeakTypedScenario>();
            builder.RegisterType<CombinationalWeakTypedScenario>().As<ICombinationalWeakTypedScenario>();
        }
    }
}
