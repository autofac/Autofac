// This software is part of the Autofac IoC container
// Copyright © 2018 Autofac Contributors
// http://autofac.org
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.

using System;
using Autofac.Core;

namespace Autofac.Features.Decorators
{
    /// <summary>
    /// A service that has been registered for the purpose of decorating other components.
    /// </summary>
    /// <remarks>
    /// This service type is used to locate decorator services and is intended for internal use only.
    /// </remarks>
    public sealed class DecoratorService : Service, IServiceWithType, IEquatable<DecoratorService>
    {
        /// <inheritdoc />
        public Type ServiceType { get; }

        /// <summary>
        /// Gets the condition that must be met for the decorator to be applied.
        /// </summary>
        public Func<IDecoratorContext, bool> Condition { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DecoratorService"/> class.
        /// </summary>
        /// <param name="serviceType">The service type for the decorator.</param>
        /// <param name="condition">The condition that must be met for the decorator to be applied.</param>
        public DecoratorService(Type serviceType, Func<IDecoratorContext, bool> condition = null)
        {
            ServiceType = serviceType ?? throw new ArgumentNullException(nameof(serviceType));
            Condition = condition ?? (context => true);
        }

        /// <inheritdoc />
        public override string Description => $"Decorator ({ServiceType.FullName})";

        /// <inheritdoc />
        public Service ChangeType(Type newType)
        {
            if (newType == null) throw new ArgumentNullException(nameof(newType));

            return new DecoratorService(newType, Condition);
        }

        /// <inheritdoc />
        public bool Equals(DecoratorService other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return ServiceType == other.ServiceType;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((DecoratorService)obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                return (GetType().GetHashCode() * 397) ^ ServiceType.GetHashCode();
            }
        }
    }
}
