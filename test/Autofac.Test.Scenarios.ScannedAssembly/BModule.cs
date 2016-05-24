using Autofac.Core;

namespace Autofac.Test.Scenarios.ScannedAssembly
{
    public class BModule : ModuleBase
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.Register(c => new BComponent()).As<BComponent>();
        }

        public override bool Equals(IModule other)
        {
            if (other == null) return false;
            return other.GetType() == GetType();
        }
    }
}