using Autofac.Core.Resolving.Pipeline;

namespace Autofac.Core.Registration
{
    public interface IServiceMiddlewareSource
    {
        void ConfigureServicePipeline(Service service, IComponentRegistryServices availableServices, IServicePipelineBuilder pipelineConfiguration);
    }
}
