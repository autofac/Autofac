// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;

namespace Autofac.Features.Decorators
{
    /// <summary>
    /// Defines the context interface used during the decoration process.
    /// </summary>
    public interface IDecoratorContext
    {
        /// <summary>
        /// Gets the implementation type of the service that is being decorated.
        /// </summary>
        Type ImplementationType { get; }

        /// <summary>
        /// Gets the service type of the service that is being decorated.
        /// </summary>
        Type ServiceType { get; }

        /// <summary>
        /// Gets the implementation types of the decorators that have been applied.
        /// </summary>
        IReadOnlyList<Type> AppliedDecoratorTypes { get; }

        /// <summary>
        /// Gets the decorator instances that have been applied.
        /// </summary>
        IReadOnlyList<object> AppliedDecorators { get; }

        /// <summary>
        /// Gets the current instance in the decorator chain. This will be initialized
        /// to the service being decorated and will then become the decorated instance
        /// as each decorator is applied.
        /// </summary>
        object CurrentInstance { get; }
    }
}
