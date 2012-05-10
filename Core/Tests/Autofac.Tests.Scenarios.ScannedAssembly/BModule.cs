namespace Autofac.Tests.Scenarios.ScannedAssembly
{
    public class BModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.Register(c => new BComponent()).As<BComponent>();
        }
    }
}