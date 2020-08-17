// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using Autofac.Core;

namespace Autofac.Features.OpenGenerics
{
    /// <summary>
    /// Activator data for the open generic delegate activator. Holds the specified factory method.
    /// </summary>
    public class OpenGenericDelegateActivatorData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OpenGenericDelegateActivatorData"/> class.
        /// </summary>
        /// <param name="factory">The factory method that will create a closed generic instance.</param>
        public OpenGenericDelegateActivatorData(Func<IComponentContext, Type[], IEnumerable<Parameter>, object> factory)
        {
            Factory = factory ?? throw new ArgumentNullException(nameof(factory));
        }

        /// <summary>
        /// Gets the factory method that will create a closed generic instance.
        /// </summary>
        public Func<IComponentContext, Type[], IEnumerable<Parameter>, object> Factory { get; }
    }
}
