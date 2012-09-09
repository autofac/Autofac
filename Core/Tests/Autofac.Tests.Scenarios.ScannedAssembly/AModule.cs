namespace Autofac.Tests.Scenarios.ScannedAssembly
{
    public class AModule : ModuleBase
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.Register(c => new AComponent()).As<AComponent>();
        }
    }
}