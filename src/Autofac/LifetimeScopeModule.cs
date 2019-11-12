using Autofac.Core;

namespace Autofac
{
    public abstract class LifetimeScopeModule : Module<LifetimeScopeBuilder>
    {
        protected sealed override LifetimeScopeBuilder CreateModuleBuilder(IComponentRegistry componentRegistry)
        {
            return new LifetimeScopeBuilder(componentRegistry.Properties);
        }
    }
}
