using System;
using Autofac.Core;

namespace Autofac.Features.Decorators
{
    public class DecoratorService : Service, IServiceWithType, IEquatable<DecoratorService>
    {
        public Type ServiceType { get; }

        public Func<IDecoratorContext, bool> Condition { get; }

        public DecoratorService(Type serviceType, Func<IDecoratorContext, bool> condition = null)
        {
            ServiceType = serviceType ?? throw new ArgumentNullException(nameof(serviceType));
            Condition = condition ?? (context => true);
        }

        public override string Description => $"Decorator ({ServiceType.FullName})";

        /// <inheritdoc />
        public Service ChangeType(Type newType)
        {
            if (newType == null) throw new ArgumentNullException(nameof(newType));

            return new DecoratorService(newType, Condition);
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
