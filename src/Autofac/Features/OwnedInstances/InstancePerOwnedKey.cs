using System;
using Autofac.Core;

namespace Autofac.Features.OwnedInstances
{
    internal class InstancePerOwnedKey : IEquatable<IServiceWithType>
    {
        private readonly TypedService _serviceWithType;

        public InstancePerOwnedKey(TypedService typedService)
            => _serviceWithType = typedService;

        public bool Equals(IServiceWithType other)
            => other != null && _serviceWithType.ServiceType == other.ServiceType;

        public override bool Equals(object obj)
            => obj is IServiceWithType serviceWithType && Equals(serviceWithType);

        public override int GetHashCode()
            => _serviceWithType.ServiceType.GetHashCode();
    }
}
