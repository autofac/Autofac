using Autofac.Core;

namespace Autofac
{
    public abstract class ContainerModule : Module<ContainerBuilder>
    {
        protected sealed override ContainerBuilder CreateModuleBuilder(IComponentRegistry componentRegistry)
        {
            return new ContainerBuilder(componentRegistry.Properties);
        }
    }
}
