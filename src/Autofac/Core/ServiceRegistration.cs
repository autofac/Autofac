using System.Collections.Generic;
using Autofac.Builder;
using Autofac.Core.Resolving.Pipeline;

namespace Autofac.Core
{
    public struct ServiceRegistration
    {
        public ServiceRegistration(IResolvePipeline pipeline, IComponentRegistration registration)
        {
            Pipeline = pipeline;
            Registration = registration;
        }

        public IResolvePipeline Pipeline { get; }

        public IComponentRegistration Registration { get; }

        public IDictionary<string, object?> Metadata => Registration.Metadata;

        public long GetRegistrationOrder() => Registration.GetRegistrationOrder();
    }
}
