using Autofac.Core.Resolving.Pipeline;
using System;
using System.Collections.Generic;
using System.Text;

namespace Autofac.Core.Registration
{
    internal class ExternalRegistryServicePipelineSource : IServiceMiddlewareSource
    {
        private readonly IComponentRegistry _componentRegistry;

        public ExternalRegistryServicePipelineSource(IComponentRegistry componentRegistry)
        {
            _componentRegistry = componentRegistry;
        }

        public void ConfigureServicePipeline(Service service, IComponentRegistryServices availableServices, IServicePipelineBuilder pipelineConfiguration)
        {
            pipelineConfiguration.UseRange(_componentRegistry.ServiceMiddlewareFor(service));
        }
    }
}
