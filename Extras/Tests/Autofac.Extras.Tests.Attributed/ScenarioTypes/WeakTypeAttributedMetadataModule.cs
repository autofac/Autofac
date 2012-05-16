using Autofac;
using AutofacContrib.Attributed;

namespace AutofacContrib.Tests.Attributed.ScenarioTypes
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
