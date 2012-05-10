namespace Autofac.Tests.Scenarios.ScannedAssembly
{
    public class AModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.Register(c => new AComponent()).As<AComponent>();
        }
    }
}