using System;
using Autofac.Core;

namespace Autofac.Features.Decorators
{
    public class DecoratorService : Service, IServiceWithType, IEquatable<DecoratorService>
    {
        public Type ServiceType { get; }

        public DecoratorService(Type serviceType)
        {
            ServiceType = serviceType ?? throw new ArgumentNullException(nameof(serviceType));
        }

        public override string Description => $"Decorator ({ServiceType.FullName})";

        /// <inheritdoc />
        public Service ChangeType(Type newType)
        {
            if (newType == null) throw new ArgumentNullException(nameof(newType));

            return new DecoratorService(newType);
        }

        public bool Equals(DecoratorService other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return ServiceType == other.ServiceType;
        }

        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((DecoratorService)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (GetType().GetHashCode() * 397) ^ ServiceType.GetHashCode();
            }
        }
    }
}
