namespace Autofac.Tests.Scenarios.ScannedAssembly
{
    public class BModule : ModuleBase
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.Register(c => new BComponent()).As<BComponent>();
        }
    }
}